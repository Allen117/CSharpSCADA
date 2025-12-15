using System;
using System.Text;

namespace Scada.Core.Domain
{
    public class tgEndDevice
    {
        // Node Settings
        private byte mbytNodeType;
        public string strNodeType;
        public string strMAC, strSystemID, strNodePos;
        public long lngMac;
        public int intNodePosX, intNodePosY;

        public string[] strSensor = new string[9];
        public string[] strS_Unit = new string[9];
        public string strCoordinatorAddress, strRouterAddress;

        public float[] sngSensor_WarnU = new float[9];
        public float[] sngSensor_WarnL = new float[9];

        public ushort ushtNWKAddress;
        public byte[] bytDOSensor = new byte[9];
        public bool blnDONotFirst;

        public DateTime datEvent, datRecordTime, datDataTime;
        public uint intErrorCode;
        public string strTreatResult;

        public bool[] blnHasSimplified = new bool[9];
        public byte bytLQI, bytToParentLQI;
        public float sngLQIRate;
        public bool blnFail = true;

        public int intSendTimes, intFail;
        public ushort ushtIndexOfRows, ushtCoordinatorIndex, ushtSleepTime;

        public int[] intSensorTransformU = new int[9];
        public int[] intSensorTransformL = new int[9];
        public byte[] bytSensorType = new byte[9];
        public byte[] bytSensorTableA = new byte[9];

        public byte[] bytSensorDecimal = new byte[9];
        public bool blnHasNewData;

        public float[] msngSensor = new float[9];
        public float[] msngSensorTransformA = new float[9];
        public float[] msngSensorTransformB = new float[9];
        public string mstrSensorDecimal;

        public byte[] bytNegativeCal = new byte[2];
        public byte[] bytSensorPNType = new byte[9];

        public int intCOConnectCheckTimes;

        public StringBuilder strMeterHistory = new StringBuilder();

        public int intCalCount, intCalItem;
        public bool blnCalibration;

        public float sng15ExkWh;
        public long lngWakePeriod, lngWakeTimeNow;
        public bool blnUpdateLatestData;

        // 你 VB 裡面是 Double(5000)
        public double[] sngMoreSensor = new double[5001];

        private int mintSQLTableIndex, mintSensorMode, mintSensingCount;
        private bool mintDemandData;

        // ===== 你 VB 的 Property：bytNodeType（含 side-effect）=====
        public byte bytNodeType
        {
            get { return mbytNodeType; }
            set
            {
                mbytNodeType = value;
                mintDemandData = false;

                switch (mbytNodeType)
                {
                    case 53: // UserPLC Multiple ModbusID
                        mintSensingCount = intCalCount;
                        mintSQLTableIndex = 1;
                        mintSensorMode = 1;
                        break;
                    case 30: // UserPLC
                        mintSensingCount = intCalCount;
                        mintSQLTableIndex = 3;
                        mintSensorMode = 1;
                        break;
                    default: // S9
                        mintSensingCount = 9;
                        mintSQLTableIndex = 0;
                        mintSensorMode = 0;
                        break;
                }
            }
        }

        public uint intDemandData
        {
            get { return (uint)(mintDemandData ? 1 : 0); }
        }

        public uint intSensorMode
        {
            get { return (uint)mintSensorMode; }
        }

        public uint intSQLTableIndex
        {
            get { return (uint)mintSQLTableIndex; }
        }

        public int intSensingCount
        {
            get { return mintSensingCount; }
        }

        public tgEndDevice()
        {
            datEvent = DateTime.Now;
            datDataTime = datEvent;
            datRecordTime = datEvent;

            bytDOSensor[4] = 255;
            bytDOSensor[5] = 255;
            bytDOSensor[6] = 255;
        }
    }
}
