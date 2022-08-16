using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using wdOS.Core.Threading;

namespace wdOS.Core
{
    internal class Application
    {
        internal static int NextPID = 0;
        internal int PID = NextPID++;
        internal string FullPath;
        internal LEF32Header ExecHeader;
        internal LEF32SectionHeader CodeSectionHeader;
        internal LEF32SectionHeader DataSectionHeader;
        internal byte[] ExecData = new byte[] { };
        internal Queue<string> ToWrite = new();
        internal Queue<string> ToRead = new();
        internal bool TryingRead;
        internal Thread MainThread = new();
        internal CrashInfo Info;
        internal int NMTStart(string[] args)
        {
            //ProcessorScheduler.JumpTo(ExecDataExecHeader.LEF_FO_Entrypoint)
            return 0;
        }
    }
    internal struct CrashInfo
    {
        internal string Message;
    }
}
