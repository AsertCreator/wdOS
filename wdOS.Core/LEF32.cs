using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace wdOS.Core
{
    [StructLayout(LayoutKind.Explicit)]
    internal struct LEF32Header
    {
        [FieldOffset(0x00)] internal ushort LEF_Magic;
        [FieldOffset(0x02)] internal byte LEF_Class;
        [FieldOffset(0x03)] internal byte LEF_Version;
        [FieldOffset(0x04)] internal uint LEF_FO_CodeSection;
        [FieldOffset(0x08)] internal uint LEF_FO_DataSection;
        [FieldOffset(0x0C)] internal uint LEF_FO_Entrypoint;
    }
    [StructLayout(LayoutKind.Explicit)]
    internal struct LEF32SectionHeader
    {
        [FieldOffset(0x00)] internal byte LEF_Name0;
        [FieldOffset(0x01)] internal byte LEF_Name1;
        [FieldOffset(0x02)] internal byte LEF_Name2;
        [FieldOffset(0x03)] internal byte LEF_Name3;
        [FieldOffset(0x04)] internal uint LEF_ReadWrite;
        [FieldOffset(0x08)] internal uint LEF_Entrypoint;
    }
    internal enum LEFClass : byte
    {
        Executable32, Library32,
        Executable64, Library64,
    }
}
