using System;
using System.Collections.Generic;
using System.Runtime.Versioning;
using System.Text;
using wdOS.Platform;
using wdOS.Platform.Core;
using Sys = Cosmos.System;

namespace wdOS
{
    // simple bootstrapper
    // for actual code, go to wdOS.Platform
    [SupportedOSPlatform("windows")]
    public class Kernel : Sys.Kernel
    {
        protected override void BeforeRun()
        {
            Bootstrapper.Main(mDebugger);

            ShellManager.Start();

            PlatformManager.ShutdownSystem(ShutdownType.SoftShutdown);
        }
        protected override void Run() { }
    }
}
