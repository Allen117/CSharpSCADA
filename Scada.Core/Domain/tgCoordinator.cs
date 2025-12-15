namespace Scada.Core.Domain
{
    public class tgCoordinator
    {
        // ===== DB 對應欄位 =====
        public ushort ushtMAC;
        public string strName;
        public int intConnPort;
        public string strConnSettings;
        public short intBufferSize;

        public string strPicFile;
        public int intModbusIDCount;
        public uint lngMac;
        public string strMAC;

        public int intPosX;
        public int intPosY;

        public byte u8Type;
        public ushort u16SleepTime;

        public byte[] u16PANID = new byte[2];
        public byte u8Channel;

        public byte[] u16ProfileID = new byte[2];

        public byte u8SrcEndNode;
        public byte u8ClusterID;
        public byte u8DstEndNode;

        public int intConnectType;
        public bool blnMonitorEnabledWhenOpen;
        public bool blnMonitorEnabled;

        public string strModbusAddress;
        public string strModbusFormat;

        // ===== Runtime 狀態（Phase 1 先留）=====
        public short intMonitorStatus;
        public int ModbusNoResponseMax;
        public int ModbusNoResponseIndex;
        public bool forceReconnect;
        public bool NeedConnect;

        // ===== 陣列（只宣告，不處理）=====
        public byte[] bytReceiveData;
        public byte[] bytModbusData;

        public byte[] bytThermoConModbusData, bytMCommand, bytMLength;
        public byte bytPacketStart, bytconstDataUbound, bytTypeIDPos;

        public int intModbusID, intModbusPacketMax, intModbusIIndex;

        public float sngFan, sngTemp_RA, sngTemp_SP, sngTemp_Eff, sngH, sngC, sngP, sngMode, sngTempSensorMode;

        public byte bytFanRight, bytTemp_RARight, bytTemp_SPRight, bytTemp_EffRight, bytHRight, bytCRight, bytPRight, bytModeRight;

        public float sngFineTLowerL, sngFineTUpperL, sngDeadBandWidth, sngDeadBandShift, sngValveClosePara, sngWindSpeedPara, sngIceWaterCloseT, sngIceWaterOpenT;

        // ===== 壓力測試系統（EnddeviceInitializer 會用到 PressureNodeSensorCount / intModbusNode）=====
        public int PressureNodeUbound;
        public int[] intModbusNode = new int[500];
        public string strLocalIP;
        public string[] PressureNodeAddressDef, PressureNodeScaleDef, PressureSensorNameDef, PressureSensorUnitDef;
        public string[] PressureSensorRowNumDef, PressureSensorIOTypeDef, PressureSensorSNumberDef;
        public string[] PressureNodeAddressDefType;

        public string[,] PressureNodeAddressNow, PressureNodeScaleNow, PressureSensorNameNow, PressureSensorUnitNow;

        public string[,] PressureNodeAddress;
        public int[] PressureNodeSensorCount; // ✅ EnddeviceInitializer 需要用到
        public float[,] PressureNodeScale, PressureNodeScaleX, PressureNodeScaleC;

        public string[,] PressureNodeScaleCodeType, PressureNodeScaleRatio;

        public int PressureModbusCommandCount;
        public int PressureModbusCommandUbound;

        public byte[] PressureAddressH = { 0, 0, 0, 0, 0, 1, 1, 2, 2, 3, 3, 0 };
        public byte[] PressureAddressL = { 0, 0, 0, 110, 220, 74, 184, 38, 148, 2, 112, 0 };
        public byte[] PressureLengthH = { 0, 0, 0, 0, 0, 0 };
        public byte[] PressureLengthL = { 8, 80, 110, 110, 110, 110, 110, 110, 110, 110, 78, 9 };
        public byte[] PressureCommand = { 1, 2, 4, 4, 4, 4, 4, 4, 4, 4, 4, 3 };
        public byte[] PressureReturnLen = { 1, 10, 220, 220, 220, 220, 220, 220, 220, 220, 156, 18 };
        public int[] PressureReturnPos = { 3000, 3100, 0, 220, 440, 660, 880, 1100, 1320, 1540, 1760, 2500 };

        public int PressureDIBitCount, PressureDOBitCount;

        public int intMBSendMode, intReturnLen, intTimeout, intMBPacketID, intPacketCount, intDeviceMode, intReConnectTCPAgain;
        public byte[] bytSendTemp = new byte[256];
        public byte u8EDType;

        public int int00000Min, int10000Min, int30000Min, int40000Min, int60000Const;

        private string AssignModbusAddress, AssignModbusLength;
        private bool mblnControlRequestNow = false;

        private int mintTimeoutOnly;
        private bool mblnDataMode;
        private byte[] mbytTCPBuffer = new byte[4096];
        private int intModbusTCPConunter;

    }
}
