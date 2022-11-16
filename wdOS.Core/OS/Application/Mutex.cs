using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wdOS.Core.OS.Application
{
    internal class Mutex
    {
        private bool Locked;
        internal void Lock() { while (Locked) { } Locked = true; }
        internal void Unlock() => Locked = false;
    }
}
