using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scada.Core.Modbus
{
    public sealed class ModbusReadCommand
    {
        public byte FunctionCode { get; }
        public int StartAddress { get; }
        public int Length { get; }

        public int EndAddress => StartAddress + Length - 1;

        public ModbusReadCommand(byte functionCode, int startAddress, int length)
        {
            FunctionCode = functionCode;
            StartAddress = startAddress;
            Length = length;
        }

        public override string ToString()
            => $"FC={FunctionCode}, Start={StartAddress}, Length={Length}";
    }

}
