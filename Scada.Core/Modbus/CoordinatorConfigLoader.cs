using Scada.Core.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scada.Core.Modbus
{
    public class CoordinatorConfigLoader
    {
        public ParsedCoordinatorFile Load(
        tgCoordinator co,
        string basePath)
        {
            string filePath = Path.Combine(
                basePath,
                "ModbusTCP",
                co.strModbusAddress);

            // 舊系統用 Encoding.Default 是合理的
            string text;
            using (var sr = new StreamReader(filePath, Encoding.Default))
            {
                text = sr.ReadToEnd();
            }

            // 交給 Parser（不碰 IO）
            return CoordinatorTxtParser.ParseText(text);
        }
    }
}
