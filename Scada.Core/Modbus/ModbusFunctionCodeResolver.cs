using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scada.Core.Modbus
{
    public static class ModbusFunctionCodeResolver
    {
        public static byte Resolve(int address)
        {
            if (address < 10000)
                return 0x01; // Coil

            if (address >= 10001 && address <= 30000)
                return 0x02; // Discrete Input

            int head = address / 10000;

            return head switch
            {
                3 => 0x04, // Input Register
                4 => 0x03, // Holding Register
                _ => throw new NotSupportedException(
                    $"Unsupported Modbus address: {address}")
            };
        }
    }

}
