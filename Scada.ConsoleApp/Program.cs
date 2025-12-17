using System;
using System.Reflection.PortableExecutable;
using System.Threading;
using Scada.Core.Config;
using Scada.Core.Data;
using Scada.Core.Data.Repositories;
using Scada.Core.Domain;
using Scada.Core.Logging;
using Scada.Core.Modbus;
using Scada.Core.Runtime;
using Scada.Core.Services;

// ===== 這裡就是 Main() 內容 =====

// 1. 讀設定檔
var settings = new SettingsReader("Setting/Settings.xml");
string connStr = settings.GetSqlConnectionString();

// 2. 建立 DB Reader
var reader = new DbReader(connStr);

// 3. 讀取 Coordinator
var coRepo = new CoordinatorRepository(reader);

ScadaRuntime.gcolCoordinator.Clear();

foreach (var mobjCoordinator in coRepo.GetCoordinator())
{
    ScadaRuntime.gcolCoordinator[mobjCoordinator.strMAC] = mobjCoordinator;

    Console.WriteLine(
        $"MAC={mobjCoordinator.strMAC}, " +
        $"ConnSettings={mobjCoordinator.strConnSettings}, " +
        $"ConnPort={mobjCoordinator.intConnPort}"
    );
}

var defLoader = new CoordinatorDefLoader();
string basePath = AppDomain.CurrentDomain.BaseDirectory;

foreach (var co in ScadaRuntime.gcolCoordinator.Values)
{
    // 讀取文字檔
    switch (co.u8Type)
    {
        case 30:
            defLoader.LoadType30(co, basePath);
            break;
    }
    // 計算Modbus指令
    var loader = new CoordinatorConfigLoader();

    ParsedCoordinatorFile parsed =
        loader.Load(co, basePath);

    // 看結果
    Console.WriteLine(parsed.TypeID);
    Console.WriteLine(parsed.Groups.Count);
    // 4. 測試 ModbusReadPlanner
    var allCommands = new List<ModbusReadCommand>();

    foreach (var group in parsed.Groups)
    {
        if (string.IsNullOrWhiteSpace(group.RawNodeDef))
            continue;

        // ⭐ Build 吃 string，這裡就餵 string
        var cmds = ModbusReadPlanner.Build(group.RawNodeDef);
        foreach (var c in cmds)
        {
            Console.WriteLine(c);
        }
        allCommands.AddRange(cmds);
    }

    var executor = new ModbusTcpExecutor();

    foreach (var cmd in allCommands)
    {
        var result = executor.Execute(co, cmd);

        if (result.IsException)
            Console.WriteLine("Read failed");
        else
        {
            Console.WriteLine($"Read OK: {cmd.StartAddress} len={cmd.Length}");
            if (!result.IsException && result.Registers != null)
            {
                for (int i = 0; i < result.Registers.Length; i++)
                {
                    int addr = cmd.StartAddress + i;
                    Console.WriteLine($"Addr {addr} = {result.Registers[i]}");
                }
            }
        }
            
    }



}

Console.WriteLine($"Coordinator count = {ScadaRuntime.gcolCoordinator.Count}");


Console.ReadKey();
