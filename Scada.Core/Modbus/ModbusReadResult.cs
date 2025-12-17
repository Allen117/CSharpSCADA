using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scada.Core.Modbus
{
    public sealed class ModbusReadResult
    {
        public ModbusReadCommand Command { get; init; }

        public bool IsException { get; init; }
        public byte? ExceptionCode { get; init; }

        // ⭐ 解碼前 raw data（先用 ushort[] 就好）
        public ushort[]? Registers { get; init; }
        public bool[]? Coils { get; init; }
    }
}
