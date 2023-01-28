using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wdOS.Core.Foundation.Threading
{
    internal class Mutex
    {
        private bool Locked;
        public void Lock()
        {
            while (!Locked) { }
            Locked = true;
        }
        public void Unlock()
        {
            Locked = false;
        }
    }
}
