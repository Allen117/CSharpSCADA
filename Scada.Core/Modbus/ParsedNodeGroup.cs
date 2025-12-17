using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scada.Core.Modbus
{
    public sealed class ParsedNodeGroup
    {
        public int GroupIndex { get; set; }
        public string? RawNodeDef { get; set; }
        public int[] Addresses { get; set; } = Array.Empty<int>();
        public int[] Scales { get; set; } = Array.Empty<int>();
        public string[] Names { get; set; } = Array.Empty<string>();
        public string[] Units { get; set; } = Array.Empty<string>();
    }
}
