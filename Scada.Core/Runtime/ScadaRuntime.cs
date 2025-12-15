using System.Collections.Generic;
using Scada.Core.Domain;

namespace Scada.Core.Runtime
{
    /// <summary>
    /// SCADA 執行期間全域狀態
    /// （等價 VB 的 gcolXXX）
    /// </summary>
    public static class ScadaRuntime
    {
        /// <summary>
        /// gcolCoordinator
        /// Key = strMAC
        /// </summary>
        public static Dictionary<string, tgCoordinator> gcolCoordinator
            = new Dictionary<string, tgCoordinator>();

        /// <summary>
        /// gcolEndDeviceNode
        /// Key = lngMac
        /// </summary>
        public static Dictionary<long, tgEndDevice> gcolEndDeviceNode
            = new Dictionary<long, tgEndDevice>();
    }
}
