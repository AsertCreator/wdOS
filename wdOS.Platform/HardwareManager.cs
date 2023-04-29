using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IL2CPU.API.Attribs;
using wdOS.Platform.Plugs;

namespace wdOS.Platform
{
    internal static class HardwareManager
    {
        private static bool initialized = false;
        internal static void Initialize()
        {
            if (!initialized)
            {
                // todo: hardware manager
                initialized = true;
            }
        }
        [PlugMethod(Assembler = typeof(HardwareManagerFRPC))]
        public static void ForceRestartPC() { }
    }
}
