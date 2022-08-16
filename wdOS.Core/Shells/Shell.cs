using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wdOS.Core.Shells
{
    internal abstract class Shell
    {
        internal abstract string Name { get; }
        internal abstract int MajorVersion { get; }
        internal abstract int MinorVersion { get; }
        internal abstract int PatchVersion { get; }
        internal abstract void Run();
        internal abstract void BeforeRun();
    }
}
