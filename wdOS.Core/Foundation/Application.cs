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
        internal LEF32File CurrentLEFFile;
        internal bool IsRunning;
        internal bool IsRunningElevated;
        internal int StartupFileType = 0;
        internal uint RegisterEAX = 0;
        internal uint RegisterEBX = 0;
        internal uint RegisterECX = 0;
        internal uint RegisterEDX = 0;
        internal uint RegisterESI = 0;
        internal uint RegisterEDI = 0;
        internal uint RegisterEIP = 0;
        internal uint RegisterEFL = 0;
        internal uint StackPointer = 0;
        internal uint StackSize = 0;
        internal uint BasePointer = 0;
    }
}
