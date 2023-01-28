using System;
using System.Runtime.InteropServices;

namespace wdOS.Core.Foundation
{
    [StructLayout(LayoutKind.Explicit)]
    public unsafe struct LEF32File
    {
        internal const uint Magic = 12523;
        internal const byte Machine_x86 = 0x03;
        internal const byte Machine_x64 = 0x06;
        internal const byte Machine_Arm32 = 0x13;
        internal const byte Machine_Arm64 = 0x16;
        internal const byte Location0 = 0x01;
        internal const byte Location1 = 0x02;
        internal const uint Location0Value = 128 * 1024 * 1024;
        internal const uint Location1Value = 64 * 1024 * 1024;
        [FieldOffset(0)] internal LEF32Header lef_Header;
        [FieldOffset(8 + 16 * 0)] internal LEF32SectionHeader lef_CodeSectionHdr;
        [FieldOffset(8 + 16 * 1)] internal LEF32SectionHeader lef_DataSectionHdr;
        internal unsafe static LEF32File Parse(byte[] data)
        {
            try
            {
                fixed (byte* ptr = &data[0])
                {
                    LEF32File file = new();
                    file.lef_Header = *(LEF32Header*)ptr;
                    file.lef_CodeSectionHdr = *(LEF32SectionHeader*)(ptr + (8 + 16 * 0));
                    file.lef_DataSectionHdr = *(LEF32SectionHeader*)(ptr + (8 + 16 * 1));
                    return file;
                }
            }
            catch
            {
                Console.WriteLine("Unable to parse LEF32 file!");
                return default;
            }
        }
    }
    [StructLayout(LayoutKind.Explicit)]
    internal struct LEF32Header
    {
        [FieldOffset(0)] internal uint lef_Magic;
        [FieldOffset(4)] internal byte lef_Version;
        [FieldOffset(5)] internal byte lef_Machine;
        [FieldOffset(6)] internal byte lef_Location;
        [FieldOffset(7)] internal byte lef_Reserved;
    }

    [StructLayout(LayoutKind.Explicit)]
    internal unsafe struct LEF32SectionHeader
    {
        [FieldOffset(0)] internal fixed char lef_Name[4];
        [FieldOffset(4)] internal uint lef_Reserved;
        [FieldOffset(8)] internal uint lef_DataPointer;
        [FieldOffset(12)] internal uint lef_DataLength;
    }
}
