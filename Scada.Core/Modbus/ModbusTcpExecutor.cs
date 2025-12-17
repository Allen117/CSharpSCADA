using Scada.Core.Domain;
using Scada.Core.Modbus;
using System.Net.Sockets;
using NModbus;
using System.Windows.Input;

public sealed class ModbusTcpExecutor : IModbusExecutor
{
    public ModbusReadResult Execute(
        tgCoordinator coordinator,
        ModbusReadCommand command)
    {
        try
        {
            using var tcp = new TcpClient();
            tcp.Connect(coordinator.strConnSettings, coordinator.intConnPort);

            var factory = new ModbusFactory();
            var master = factory.CreateMaster(tcp);

            master.Transport.Retries = 0;
            master.Transport.ReadTimeout = 1000;

            // 🔑 安全轉型（避免 int 超過 ushort）
            ushort start = checked((ushort)ToModbusOffset(command.StartAddress, command.FunctionCode));
            ushort length = checked((ushort)command.Length);
            byte slaveId = (byte)coordinator.intModbusID;

            switch (command.FunctionCode)
            {
                case 3: // Holding Registers
                    {
                        var regs = master.ReadHoldingRegisters(
                            slaveId,
                            start,
                            length);

                        return new ModbusReadResult
                        {
                            Command = command,
                            Registers = regs,
                            IsException = false
                        };
                    }

                case 4: // Input Registers
                    {
                        var regs = master.ReadInputRegisters(
                            slaveId,
                            start,
                            length);

                        return new ModbusReadResult
                        {
                            Command = command,
                            Registers = regs,
                            IsException = false
                        };
                    }

                case 1: // Coils
                    {
                        var coils = master.ReadCoils(
                            slaveId,
                            start,
                            length);

                        return new ModbusReadResult
                        {
                            Command = command,
                            Coils = coils,
                            IsException = false
                        };
                    }

                case 2: // Discrete Inputs
                    {
                        var inputs = master.ReadInputs(
                            slaveId,
                            start,
                            length);

                        return new ModbusReadResult
                        {
                            Command = command,
                            Coils = inputs,
                            IsException = false
                        };
                    }

                default:
                    return new ModbusReadResult
                    {
                        Command = command,
                        IsException = true,
                        ExceptionCode = 1 // Illegal Function
                    };
            }
        }
        catch
        {
            // ⚠️ 目前策略：任何例外一律 Bad
            return new ModbusReadResult
            {
                Command = command,
                IsException = true,
                ExceptionCode = null
            };
        }
    }

    public static int ToModbusOffset(int address, byte functionCode)
    {
        return functionCode switch
        {
            // Holding Registers (4xxxx / 4xxxxx)
            3 => address >= 40001 ? address - 40001 : address,

            // Input Registers (3xxxx / 3xxxxx)
            4 => address >= 30001 ? address - 30001 : address,

            // Coils
            1 => address >= 1 ? address - 1 : address,

            // Discrete Inputs
            2 => address >= 10001 ? address - 10001 : address,

            _ => throw new ArgumentOutOfRangeException(nameof(functionCode))
        };
    }

}
