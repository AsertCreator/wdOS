using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using wdOS.Platform.Shell.UI;

namespace wdOS.Platform.Core
{
    public static class ShellManager
    {
        public static void Start()
        {
            WindowManager.Initialize();
            WindowManager.Start();

            return;
            // nope
            if (Bootstrapper.AlterShell == "debugsh")
            {
                Console.WriteLine("Debug Shell ended with code " + DebugShellManager.RunDebugShell());
                while (true) { }
            }
        }
    }
}
