using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wdOS.Platform
{
    public sealed class ShellManager
    {
        public void Start()
        {
            if (Bootstrapper.AlterShell == "debugsh")
            {
                Console.WriteLine("Debug Shell ended with code " + DebugShellManager.RunDebugShell());
                while (true) { }
            }
        }
    }
}
