using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wdOS.Core.Foundation.Threading
{
    internal class Context
    {
        internal uint RegisterEAX = 0;
        internal uint RegisterEBX = 0;
        internal uint RegisterECX = 0;
        internal uint RegisterEDX = 0;
        internal uint RegisterESP = 0;
        internal uint RegisterEBP = 0;
        internal uint RegisterESI = 0;
        internal uint RegisterEDI = 0;
        internal uint RegisterEFL = 0;
        internal uint RegisterEIP = 0;
        internal byte[] StackData;
    }
}
