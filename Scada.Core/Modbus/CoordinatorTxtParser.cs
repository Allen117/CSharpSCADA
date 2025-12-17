using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Scada.Core.Modbus
{ 
    public static class CoordinatorTxtParser
    {
        // 例如：Nodedef(0)=...
        private static readonly Regex KeyWithIndexRegex =
            new Regex(@"^(?<key>[A-Za-z_]\w*)\((?<idx>\d+)\)$", RegexOptions.Compiled);

        public static ParsedCoordinatorFile ParseFile(string filePath)
        {
            var lines = File.ReadAllLines(filePath);
            return ParseLines(lines);
        }

        public static ParsedCoordinatorFile ParseText(string text)
        {
            var lines = text.Replace("\r\n", "\n").Split('\n');
            return ParseLines(lines);
        }

        public static ParsedCoordinatorFile ParseLines(IEnumerable<string> lines)
        {
            var result = new ParsedCoordinatorFile();

            // 暫存：每個 groupIndex 對應一份 builder
            var builders = new Dictionary<int, GroupBuilder>();

            foreach (var raw in lines)
            {
                var line = raw?.Trim();
                if (string.IsNullOrWhiteSpace(line)) continue;
                if (line.StartsWith("'") || line.StartsWith("#") || line.StartsWith("//")) continue;

                var eq = line.IndexOf('=');
                if (eq <= 0) continue;

                var left = line.Substring(0, eq).Trim();
                var right = line.Substring(eq + 1).Trim();

                // TypeID / TypeName（無 index）
                if (left.Equals("TypeID", StringComparison.OrdinalIgnoreCase))
                {
                    if (int.TryParse(right, NumberStyles.Integer, CultureInfo.InvariantCulture, out var typeId))
                        result.TypeID = typeId;
                    continue;
                }
                if (left.Equals("TypeName", StringComparison.OrdinalIgnoreCase))
                {
                    result.TypeName = right;
                    continue;
                }

                // Nodedef(0) / Nodescale(0) / Nodename(0) / NodeUnit(0)
                var m = KeyWithIndexRegex.Match(left);
                if (!m.Success) continue;

                var key = m.Groups["key"].Value;
                var idx = int.Parse(m.Groups["idx"].Value, CultureInfo.InvariantCulture);

                if (!builders.TryGetValue(idx, out var gb))
                {
                    gb = new GroupBuilder(idx);
                    builders[idx] = gb;
                }

                switch (key.ToLowerInvariant())
                {
                    case "nodedef":
                        gb.Addresses = ParseIntCsv(right);
                        gb.RawNodeDef = right;
                        break;

                    case "nodescale":
                        gb.Scales = ParseIntCsv(right);
                        break;

                    case "nodename":
                        gb.Names = ParseStringCsv(right);
                        break;

                    case "nodeunit":
                        gb.Units = ParseStringCsvPreserveEmpty(right);
                        break;

                    default:
                        // 其他 key 先忽略（未來要加再擴充）
                        break;
                }
            }

            // Builder → ParsedNodeGroup
            foreach (var gb in builders.Values.OrderBy(x => x.GroupIndex))
            {
                result.Groups.Add(gb.Build());
            }

            // （可選）基本一致性檢查：若你希望先「只解析不驗證」，可以把下面註解掉
            //Validate(result);

            return result;
        }

        // --- CSV helpers ---

        private static int[] ParseIntCsv(string csv)
        {
            // 允許空字串、允許有多餘逗點（會忽略空欄）
            return csv.Split(',')
                      .Select(s => s.Trim())
                      .Where(s => !string.IsNullOrEmpty(s))
                      .Select(s => int.Parse(s, CultureInfo.InvariantCulture))
                      .ToArray();
        }

        private static string[] ParseStringCsv(string csv)
        {
            // 一般字串欄位：空欄會被忽略（Nodename 通常不需要保留空欄）
            return csv.Split(',')
                      .Select(s => s.Trim())
                      .Where(s => !string.IsNullOrEmpty(s))
                      .ToArray();
        }

        private static string[] ParseStringCsvPreserveEmpty(string csv)
        {
            // NodeUnit 常常是 ",,,,," 這種，空欄要保留長度（對齊 address 數量）
            return csv.Split(',')
                      .Select(s => s.Trim())
                      .ToArray();
        }

        // --- Builder ---

        private sealed class GroupBuilder
        {
            public int GroupIndex { get; }
            public int[]? Addresses;
            public int[]? Scales;
            public string[]? Names;
            public string[]? Units;
            public string? RawNodeDef;

            public GroupBuilder(int idx) => GroupIndex = idx;

            public ParsedNodeGroup Build()
            {
                return new ParsedNodeGroup
                {
                    GroupIndex = GroupIndex,
                    Addresses = Addresses ?? Array.Empty<int>(),
                    Scales = Scales ?? Array.Empty<int>(),
                    Names = Names ?? Array.Empty<string>(),
                    Units = Units ?? Array.Empty<string>(),
                    RawNodeDef = RawNodeDef
                };
            }
        }

        // （可選）一致性驗證：確保各欄長度能對齊 Nodedef
        private static void Validate(ParsedCoordinatorFile file)
        {
            foreach (var g in file.Groups)
            {
                var n = g.Addresses.Length;
                if (n == 0) continue;

                if (g.Scales.Length != 0 && g.Scales.Length != n)
                    throw new FormatException($"Nodescale({g.GroupIndex}) count={g.Scales.Length} 不等於 Nodedef count={n}");

                if (g.Names.Length != 0 && g.Names.Length != n)
                    throw new FormatException($"Nodename({g.GroupIndex}) count={g.Names.Length} 不等於 Nodedef count={n}");

                if (g.Units.Length != 0 && g.Units.Length != n)
                    throw new FormatException($"NodeUnit({g.GroupIndex}) count={g.Units.Length} 不等於 Nodedef count={n}");
            }
        }
    }
}
