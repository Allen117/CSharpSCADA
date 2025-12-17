using Scada.Core.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scada.Core.Modbus
{
    internal interface IModbusExecutor
    {
        ModbusReadResult Execute(
    tgCoordinator coordinator,
    ModbusReadCommand command);
    }
}
