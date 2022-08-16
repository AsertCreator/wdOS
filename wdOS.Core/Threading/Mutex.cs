using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wdOS.Core.Threading
{
    internal class Mutex
    {
        private bool MutexLock;
        internal void Lock() { while (MutexLock) { } MutexLock = true; }
        internal void Unlock() => MutexLock = false;
    }
}
