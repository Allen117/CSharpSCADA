using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scada.Core.Modbus
{
    public static class ModbusReadPlanner
    {

        public static List<ModbusReadCommand> Build(
            string addressCsv,
            int maxRegisters = 100,
            int registerDecodeWidth = 4) // double
        {
            if (string.IsNullOrWhiteSpace(addressCsv))
                return new List<ModbusReadCommand>();

            var addresses = addressCsv
                .Split(',')
                .Select(x => int.Parse(x.Trim()))
                .Distinct()
                .OrderBy(x => x)
                .ToList();

            var result = new List<ModbusReadCommand>();

            // ① 依 Function Code 分群
            var groups = addresses
                .GroupBy(ModbusFunctionCodeResolver.Resolve);

            foreach (var group in groups)
            {
                byte fc = group.Key;
                var groupAddresses = group.OrderBy(x => x).ToList();

                BuildGroup(
                    result,
                    fc,
                    groupAddresses,
                    maxRegisters,
                    registerDecodeWidth);
            }

            return result;
        }

        private static void BuildGroup(
            List<ModbusReadCommand> result,
            byte functionCode,
            List<int> addresses,
            int maxRegisters,
            int registerDecodeWidth)
        {
            int padding = GetPadding(functionCode, registerDecodeWidth);

            int segmentStart = addresses[0];
            int lastRequired = addresses[0];

            for (int i = 1; i < addresses.Count; i++)
            {
                int addr = addresses[i];

                int potentialEnd = addr + padding;
                int lengthIfMerged = potentialEnd - segmentStart + 1;

                if (lengthIfMerged <= maxRegisters)
                {
                    lastRequired = addr;
                    continue;
                }

                AddCommand(result, functionCode, segmentStart, lastRequired, padding);

                segmentStart = addr;
                lastRequired = addr;
            }

            AddCommand(result, functionCode, segmentStart, lastRequired, padding);
        }

        private static void AddCommand(
            List<ModbusReadCommand> result,
            byte functionCode,
            int segmentStart,
            int lastRequired,
            int padding)
        {
            int endAddress = lastRequired + padding;
            int length = endAddress - segmentStart + 1;

            result.Add(new ModbusReadCommand(functionCode, segmentStart, length));
        }

        private static int GetPadding(byte functionCode, int registerDecodeWidth)
        {
            // 01 / 02：bit 型態，不需要 padding
            if (functionCode == 0x01 || functionCode == 0x02)
                return 0;

            // 03 / 04：register 型態
            return registerDecodeWidth - 1; // double = 4 → +3
        }

    }

}
