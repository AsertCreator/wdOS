using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wdOS.Core.OS.Application
{
    internal struct Resource
    {
        internal ResType RIID;
        internal string Name;
        internal byte[] Data;
    }
    internal enum ResType
    {
        TextTable, Bitmap, Audio
    }
}
