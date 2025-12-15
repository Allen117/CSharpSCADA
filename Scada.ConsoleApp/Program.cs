using System;
using System.Reflection.PortableExecutable;
using System.Threading;
using Scada.Core.Config;
using Scada.Core.Data;
using Scada.Core.Data.Repositories;
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
    switch (co.u8Type)
    {
        case 30:
            defLoader.LoadType30(co, basePath);
            break;
    }
}

Console.WriteLine($"Coordinator count = {ScadaRuntime.gcolCoordinator.Count}");
// 4. 測試 ModbusReadPlanner
var cmds = ModbusReadPlanner.Build("2001,40001,2002,40003");

foreach (var c in cmds)
{
    Console.WriteLine(c);
}

Console.ReadKey();
