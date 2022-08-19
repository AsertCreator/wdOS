using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace wdOS.Core
{
    internal struct LEFHeader
    {
        internal const ushort RequiredMagic = 0x104F;
        internal ushort LEF_Magic;
        internal byte LEF_Class;
        internal byte LEF_Version;
        internal uint LEF_FO_CodeSection;
        internal uint LEF_FO_DataSection;
        internal uint LEF_FO_Entrypoint;
    }
    internal struct LEFSectionHeader
    {
        internal byte LEF_Name0;
        internal byte LEF_Name1;
        internal byte LEF_Name2;
        internal byte LEF_Name3;
        internal uint LEF_FileOffset;
    }
    internal enum LEFClass : byte
    {
        Executable32, Library32,
        Executable64, Library64,
    }
}
