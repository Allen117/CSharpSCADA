using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Scada.Core.Domain;

namespace Scada.Core.Services
{
    public partial class CoordinatorDefLoader
    {
        /// <summary>
        /// UserPLC（Type 30）定義檔讀取
        /// 對應 VB: ReadDefDataOfType30FromFile
        /// </summary>
        public void LoadType30(tgCoordinator co, string basePath)
        {
            int mintIndex;
            string[] mstrText;

            Dictionary<string, string> mcolCreateTable =
                new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            try
            {
                // ===== 1. 讀主定義檔 =====
                string filePath = Path.Combine(basePath, "ModbusTCP", co.strModbusAddress);

                using (var sr = new StreamReader(filePath, Encoding.Default))
                {
                    while (!sr.EndOfStream)
                    {
                        mstrText = sr.ReadLine().Split('=');
                        if (mstrText.Length == 2)
                        {
                            mcolCreateTable[mstrText[0].Trim().ToUpper()] =
                                mstrText[1].Trim();
                        }
                    }
                }

                // ===== 2. 基本欄位 =====
                co.u8EDType = Convert.ToByte(mcolCreateTable["TYPEID"]);
                co.strLocalIP = mcolCreateTable["TYPENAME"];

                // Type30 為單一 ModbusID
                co.intModbusIDCount = 0;

                // ===== 3. Packet / Node 數量 =====
                // VB: PressureNodeUbound = (Count - 2) \ 4 - 1
                co.intModbusPacketMax = (mcolCreateTable.Count - 2) / 4;
                co.PressureNodeUbound = co.intModbusPacketMax - 1;

                // ===== 4. 配置陣列（重點）=====
                co.PressureNodeAddressDef = new string[co.PressureNodeUbound + 1];
                co.PressureNodeScaleDef = new string[co.PressureNodeUbound + 1];
                co.PressureSensorNameDef = new string[co.PressureNodeUbound + 1];
                co.PressureSensorUnitDef = new string[co.PressureNodeUbound + 1];
                co.PressureNodeSensorCount = new int[co.PressureNodeUbound + 1];

                // ===== 5. 填值 =====
                for (mintIndex = 0; mintIndex <= co.PressureNodeUbound; mintIndex++)
                {
                    co.PressureNodeAddressDef[mintIndex] =
                        mcolCreateTable[$"NODEDEF({mintIndex})"];

                    co.PressureNodeScaleDef[mintIndex] =
                        mcolCreateTable[$"NODESCALE({mintIndex})"];

                    co.PressureSensorNameDef[mintIndex] =
                        mcolCreateTable[$"NODENAME({mintIndex})"];

                    co.PressureSensorUnitDef[mintIndex] =
                        mcolCreateTable[$"NODEUNIT({mintIndex})"];

                    // Type 30：一個 node 對應一個 sensor
                    co.PressureNodeSensorCount[mintIndex] = 1;
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"LoadType30 failed, MAC={co.strMAC}", ex);
            }
        }
    }
}
