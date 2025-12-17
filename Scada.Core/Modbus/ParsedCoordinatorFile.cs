using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scada.Core.Modbus
{
    public sealed class ParsedCoordinatorFile
    {
        public int? TypeID { get; set; }
        public string? TypeName { get; set; }

        public List<ParsedNodeGroup> Groups { get; } = new();
    }

}
