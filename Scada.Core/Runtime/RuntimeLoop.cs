using Scada.Core.Binding;
using Scada.Core.Data.Repository;
using Scada.Core.DeviceClass;
using Scada.Core.Domain;
using Scada.Core.Logging;
using Scada.Core.Modbus;
using Scada.Core.Modbus.Decode;
using Scada.Core.Control;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Channels; // ⭐ 使用 Channel 實作高效能隊列

namespace Scada.Core.Runtime
{
    public class RuntimeLoop
    {
        private readonly List<clsControlDevice> _controlDevices;
        private readonly Dictionary<string, List<ModbusReadCommand>> _commandsMap;
        private readonly SensorResolver _resolver;
        private readonly ModbusTcpExecutor _executor;
        private readonly Logger _logger;
        private CancellationTokenSource? _cts;
        private HistoryRepository _historyRepo;

        // ⭐ 新增：非同步指令通道 (Producer-Consumer)
        private readonly Channel<ControlCommand> _controlChannel = Channel.CreateUnbounded<ControlCommand>();

        public RuntimeLoop(List<clsControlDevice> controlDevices,
                           Dictionary<string, List<ModbusReadCommand>> commandsMap,
                           HistoryRepository historyRepo,
                           Logger logger)
        {
            _controlDevices = controlDevices;
            _commandsMap = commandsMap;
            _logger = logger;
            _historyRepo = historyRepo;
            _resolver = new SensorResolver();
            _executor = new ModbusTcpExecutor();
        }

        public void Start()
        {
            if (_cts != null) return;
            _cts = new CancellationTokenSource();
            var token = _cts.Token;

            // 1️⃣ 為每一台 Coordinator 啟動讀取執行緒 (AI 採集)
            foreach (var co in ScadaRuntime.gcolCoordinator.Values)
            {
                var currentCo = co;
                if (_commandsMap.TryGetValue(currentCo.strMAC, out var myCmds))
                {
                    Task.Run(() => CoordinatorWorker(currentCo, myCmds, token), token);
                }
            }

            // 2️⃣ 啟動控制指令背景執行緒 (AO 控制)
            Task.Run(() => ControlWorker(token), token);

            // 3️⃣ 啟動 UI 顯示任務 (每秒一次)
            Task.Run(async () =>
            {
                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        Console.WriteLine($"\n===== 監控畫面 @ {DateTime.Now:HH:mm:ss} =====");
                        foreach (var dev in _controlDevices)
                        {
                            if (dev is clsAI ai)
                            {
                                Console.WriteLine($"AI Device: {ai.Name} (MAC: {ai.MacId})");
                                for (int i = 0; i < ai.InputSIDs.Length; i++)
                                {
                                    string sid = ai.InputSIDs[i];
                                    if (string.IsNullOrWhiteSpace(sid)) continue;
                                    var sp = _resolver.ResolveBySID(sid);
                                    if (sp != null && sp.Value.HasValue)
                                    {
                                        Console.WriteLine($"  [{i + 1}] {ai.InputNames[i]} = {sp.Value:F2}");
                                    }
                                }
                            }
                            if (dev is clsAO AO)
                            {
                                Console.WriteLine($"AO Device: {AO.Name} (MAC: {AO.MacId})");


                                // 💡 修改處：將隨機數改為當前秒數 (0-59)
                                double currentSecond = (double)DateTime.Now.Second;
                                for (int i = 0; i < AO.OutputCIDs.Length; i++)
                                {
                                    string cid = AO.OutputCIDs[i];
                                    if (string.IsNullOrWhiteSpace(cid)) continue;

                                    // 2. 建立控制指令
                                    // 注意：Description 必須對應到 AddressProfile 的 Name，ControlWorker 才能找到位址
                                    ControlCommand cmd = new ControlCommand
                                    {
                                        MacId = AO.MacId, // 來源自 clsControlDevice
                                        CID = cid,        // 來源自 clsAO
                                        Description = AO.OutputNames[i], // 用於在 Worker 中比對 Modbus 位址
                                        Value = currentSecond,
                                        Timestamp = DateTime.Now
                                    };

                                    // 3. 丟入隊列排隊
                                    if(cmd.CID != "??")
                                        EnqueueControl(cmd); // 這會觸發 RuntimeLoop 中的 _controlChannel

                                    _logger.Info($"[測試] 已將 {cmd.Description} 寫入指令加入隊列，目標值: {cmd.Value:F2}");
                                }
                            }
                        }
                    }
                    catch (Exception ex) { Console.WriteLine($"UI Error: {ex.Message}"); }
                    await Task.Delay(1000, token);
                }
            }, token);

            // 4️⃣ 啟動歷史資料存檔任務 (每分鐘一次)
            Task.Run(async () =>
            {
                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        int secondsToWait = 60 - DateTime.Now.Second;
                        await Task.Delay(secondsToWait * 1000, token);
                        var allPoints = ScadaRuntime.gcolEndDeviceNode.Values.SelectMany(d => d.Sensors.Values).ToList();
                        _historyRepo.BatchSaveHistory(allPoints);
                        _logger.Info($"[系統] 已存入 {allPoints.Count} 筆歷史數據");
                    }
                    catch (Exception ex) { _logger.Error("存檔任務失敗", ex); }
                }
            }, token);
        }

        // ⭐ 提供給外部 (AI 或 UI) 的控制輸入介面
        public void EnqueueControl(ControlCommand cmd)
        {
            _controlChannel.Writer.TryWrite(cmd);
        }

        // ⭐ 控制指令專用的背景消費者
        private async Task ControlWorker(CancellationToken token)
        {
            _logger.Info("[系統] ControlWorker 啟動中...");
            await foreach (var cmd in _controlChannel.Reader.ReadAllAsync(token))
            {
                try
                {
                    // 1. 解析 CID (格式: MAC-SN，例如 123456-S1)
                    string[] parts = cmd.CID.Split('-');
                    if (parts.Length < 2) continue;

                    // 提取 MAC (例如 "123456") 與 索引字串 (例如 "S1")
                    string macStr = parts[0];
                    string indexStr = parts[1].Replace("S", ""); // 將 "S1" 變成 "1"

                    if (long.TryParse(macStr, out long targetMac) && int.TryParse(indexStr, out int targetIndex))
                    {
                        // 2. 尋找設備
                        var device = ScadaRuntime.gcolEndDeviceNode.Values
                            .FirstOrDefault(d => d.lngMac == targetMac);

                        if (device == null)
                        {
                            _logger.Warn($"[控制失敗] 找不到設備 MAC: {targetMac}");
                            continue;

                        }

                        // 3. 根據 SequenceIndex 尋找對應的 AddressProfile
                        // 💡 不再比對 p.Name，而是直接比對 p.SequenceIndex
                        var profile = device.AddressProfiles.Values
                            .FirstOrDefault(p => p.SequenceIndex == targetIndex);

                        if (profile != null)
                        {
                            // 4. 取得 Coordinator
                            string coMac = (device.lngMac / 65536L).ToString();
                            if (ScadaRuntime.gcolCoordinator.TryGetValue(coMac, out var co))
                            {
                                // 5. 執行寫入
                                // 💡 從 Profile 取得原始功能碼 (例如 1 代表 Coil, 3 代表 Register)
                                byte fc = (byte)profile.FunctionCode;

                                double writeVal = cmd.Value / profile.Scale; // 考慮倍率轉換

                                // 呼叫統一的寫入入口
                                bool success = _executor.ExecuteWrite(co, profile.Address, writeVal, fc);

                                if (success)
                                    _logger.Info($"[控制成功] {cmd.Description} (Index:{targetIndex}) => {cmd.Value} @ Addr:{profile.Address}");
                                else
                                    _logger.Warn($"[控制失敗] Modbus 寫入失敗: {cmd.Description}");
                            }
                        }
                        else
                        {
                            _logger.Warn($"[控制失敗] 設備 {targetMac} 找不到 SequenceIndex = {targetIndex} 的點位");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error("ControlWorker 執行異常", ex);
                }

                await Task.Delay(50, token); // 保護硬體通訊
            }
        }

        private async Task CoordinatorWorker(tgCoordinator co, List<ModbusReadCommand> myCmds, CancellationToken token)
        {
            var myDevices = ScadaRuntime.gcolEndDeviceNode.Values
                .Where(d => (d.lngMac / 65536L) == (long)co.lngMac).ToList();

            while (!token.IsCancellationRequested)
            {
                try
                {
                    foreach (var cmd in myCmds)
                    {
                        var result = _executor.Execute(co, cmd);
                        if (result.IsException || result.Registers == null) continue;

                        ushort[] regs = result.Registers;
                        int start = cmd.StartAddress;

                        foreach (var dev in myDevices)
                        {
                            foreach (var profile in dev.AddressProfiles.Values)
                            {
                                if (profile.Address < start || (profile.Address + profile.WordCount - 1) >= start + regs.Length)
                                    continue;

                                double value = ModbusValueDecoder.Decode(regs, start, profile);
                                if (!dev.Sensors.TryGetValue(profile.Address, out var sp))
                                {
                                    sp = new SensorPoint { Address = profile.Address, SID = $"{dev.lngMac}-S{profile.SequenceIndex}" };
                                    dev.Sensors[profile.Address] = sp;
                                }
                                sp.Value = value;
                                sp.LastUpdate = DateTime.Now;
                            }
                        }
                    }
                }
                catch { }
                await Task.Delay(1000, token);
            }
        }

        public void Stop() => _cts?.Cancel();
    }
}