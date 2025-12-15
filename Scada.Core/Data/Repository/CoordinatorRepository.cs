using System;
using System.Collections.Generic;
using Scada.Core.Data;
using Scada.Core.Domain;

namespace Scada.Core.Data.Repositories
{
    public class CoordinatorRepository
    {
        private readonly DbReader _reader;

        public CoordinatorRepository(DbReader reader)
        {
            _reader = reader;
        }

        /// <summary>
        /// 讀取所有 Coordinator（只做資料對應）
        /// </summary>
        public IEnumerable<tgCoordinator> GetCoordinator()
        {
            const string sql = @"SELECT * FROM Coordinator where u8Type = 30";   // 先做單ID Modbus

            return _reader.Query(sql, r =>
            {
                uint mlngMAC = Convert.ToUInt32(r["u32MACAddressLo"]);
                string mstrMAC = mlngMAC.ToString();

                var mobjCoordinator = new tgCoordinator();

                // ===== 完全對齊你 VB 的寫法 =====
                mobjCoordinator.ushtMAC = (ushort)(mlngMAC % 65536);
                mobjCoordinator.strName = r["CO_Name"].ToString();
                mobjCoordinator.intConnPort = Convert.ToInt32(r["ConnPort"]);
                mobjCoordinator.strConnSettings = r["ConnSettings"].ToString();
                mobjCoordinator.intBufferSize = Convert.ToInt16(r["BufferSize"]);
                mobjCoordinator.intModbusID = Convert.ToInt32(r["u32MACAddressHi"]);

                mobjCoordinator.lngMac = mlngMAC;
                mobjCoordinator.strMAC = mstrMAC;

                mobjCoordinator.u8Type = Convert.ToByte(r["u8Type"]);
                mobjCoordinator.u16SleepTime = Convert.ToUInt16(r["u16SleepTime"]);

                ushort pan = Convert.ToUInt16(r["u16PANID"]);
                mobjCoordinator.u16PANID[0] = (byte)(pan / 256);
                mobjCoordinator.u16PANID[1] = (byte)(pan % 256);

                mobjCoordinator.u8Channel = Convert.ToByte(r["u8Channel"]);

                ushort profile = Convert.ToUInt16(r["u16ProfileID"]);
                mobjCoordinator.u16ProfileID[0] = (byte)(profile / 256);
                mobjCoordinator.u16ProfileID[1] = (byte)(profile % 256);

                mobjCoordinator.u8SrcEndNode = Convert.ToByte(r["u8SrcEndNode"]);
                mobjCoordinator.u8ClusterID = Convert.ToByte(r["u8ClusterID"]);
                mobjCoordinator.u8DstEndNode = Convert.ToByte(r["u8DstEndNode"]);

                mobjCoordinator.intConnectType = Convert.ToInt32(r["ConnType"]);
                mobjCoordinator.blnMonitorEnabledWhenOpen =
                    Convert.ToBoolean(r["MonitorEnabled"]);
                mobjCoordinator.blnMonitorEnabled =
                    Convert.ToBoolean(r["MonitorEnabled"]);

                mobjCoordinator.strModbusAddress =
                    r["ModbusAddress"] == DBNull.Value ? "" : r["ModbusAddress"].ToString();

                mobjCoordinator.strModbusFormat =
                    r["ModbusFormat"] == DBNull.Value ? "" : r["ModbusFormat"].ToString();

                return mobjCoordinator;
            });
        }
    }
}
