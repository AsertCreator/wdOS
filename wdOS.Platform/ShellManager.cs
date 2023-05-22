using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wdOS.Platform
{
    internal sealed class ShellManager
    {
        internal void Start()
        {
            if (Bootstrapper.AlterShell == "debugsh")
            {
                Console.WriteLine("Debug Shell ended with code " + DebugShell.RunDebugShell());
                while (true) { }
            }
        }
    }
}
