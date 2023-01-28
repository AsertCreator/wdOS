using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wdOS.Core.Foundation
{
    internal class Application
    {
        internal static uint NextPID = 0;
        internal uint PID = NextPID++;
        internal bool IsRunning;
        internal bool IsRunningElevated;
    }
}
