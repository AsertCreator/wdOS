using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wdOS.Core.Shell
{
    public abstract class ShellBase
    {
        public const int ShellExitReasonRegular = 0;
        public const int ShellExitReasonLogoff = 1;
        public const int ShellExitReasonError = 2;
        public abstract string ShellName { get; }
        public abstract string ShellDesc { get; }
        public abstract int ShellMajorVersion { get; }
        public abstract int ShellMinorVersion { get; }
        public abstract int ShellPatchVersion { get; }
        public bool IsRunning;
        public int ExitReason;
        public abstract void ShellAfterRun();
        public abstract void ShellBeforeRun();
        public abstract void ShellRun();
    }
}
