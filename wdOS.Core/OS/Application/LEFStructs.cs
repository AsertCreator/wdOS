using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace makeapp
{
    [StructLayout(LayoutKind.Explicit)]
    internal struct LEF32File
    {
        [FieldOffset(0)] internal LEF32Header lef_Header;
        [FieldOffset(8 + (16 * 0))] internal LEF32SectionHeader lef_CodeSectionHdr;
        [FieldOffset(8 + (16 * 1))] internal LEF32SectionHeader lef_DataSectionHdr;
        [FieldOffset(8 + (16 * 2))] internal LEF32SectionHeader lef_WdatSectionHdr;
        [FieldOffset(8 + (16 * 3))] internal LEF32SectionHeader lef_SymbSectionHdr;
        internal unsafe static LEF32File Parse(byte[] data)
        {
            LEF32File file = new();
            byte[] hdrdata = data[0..56];
            LEF32Header* hdr = null;
            fixed (byte* ptr = hdrdata) hdr = (LEF32Header*)ptr;
            file.lef_Header = *hdr;
            return file;
        }
    }
    [StructLayout(LayoutKind.Explicit)]
    internal struct LEF32Header
    {
        internal const uint Magic = 0x7EF32000;
        internal const byte Machine_x86 = 0x03;
        internal const byte Machine_x64 = 0x06;
        internal const byte Machine_Arm32 = 0x13;
        internal const byte Machine_Arm64 = 0x16;
        internal const byte Location0 = 0xF8;
        internal const byte Location1 = 0xF0;
        [FieldOffset(0)] internal uint lef_Magic;
        [FieldOffset(4)] internal byte lef_Version;
        [FieldOffset(5)] internal byte lef_Machine;
        [FieldOffset(6)] internal byte lef_Location;
        [FieldOffset(7)] internal byte lef_Reserved;
    }

    [StructLayout(LayoutKind.Explicit)]
    internal struct LEF32SectionHeader
    {
        [FieldOffset(0)] internal byte lef_NameChar0;
        [FieldOffset(1)] internal byte lef_NameChar1;
        [FieldOffset(2)] internal byte lef_NameChar2;
        [FieldOffset(3)] internal byte lef_NameChar3;
        [FieldOffset(4)] internal uint lef_Reserved;
        [FieldOffset(8)] internal uint lef_DataPointer;
        [FieldOffset(12)] internal uint lef_DataLength;
    }

    [StructLayout(LayoutKind.Explicit)]
    internal struct LEF32SymbolSectionHeader
    {
        [FieldOffset(0)] internal ushort lef_Magic;
        [FieldOffset(2)] internal ushort lef_Count;
    }
    [StructLayout(LayoutKind.Explicit)]
    internal struct LEF32SymbolSectionEntry
    {
        [FieldOffset(0)] internal ushort lef_id;
        [FieldOffset(2)] internal ushort lef_flags;
        [FieldOffset(4)] internal uint lef_codeoffset;
    }
}
