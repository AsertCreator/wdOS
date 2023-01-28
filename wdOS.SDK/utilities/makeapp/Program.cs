using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace makeapp
{
    internal static class Program
    {
        internal static void Main()
        {
            Console.WriteLine("makeapp - wdOS app maker");
            var file = Create32BitFile();
            var bytes = ToByteArray(file);
            DumpArray(bytes);
            DumpStruct(file.lef_Header);
            DumpStruct(file.lef_CodeSectionHdr);
            DumpStruct(file.lef_DataSectionHdr);
            DumpStruct(file.lef_WdatSectionHdr);
        }
        internal static void DumpArray(byte[] array) => Console.WriteLine(DumpArrayAsString(array));
        internal static string DumpArrayAsString(byte[] array)
        {
            StringBuilder text = new();
            for (int i = 0; i < array.Length; i++) text.Append($"{array[i]:X2} ");
            return text.ToString();
        }
        internal static unsafe void DumpStruct<T>(T struc, int tabs = 0) where T : unmanaged
        {
            Type type = typeof(T);
            Console.WriteLine($"\"{type.FullName}\" instance dump:");
            var bytes = ToByteArray(struc);
            var fields = type.GetRuntimeFields();
            foreach (var field in fields)
            {
                if (!field.IsStatic)
                {
                    var ftyp = field.FieldType;
                    var size = Marshal.SizeOf(field.GetValue(struc));
                    var fbyte = new byte[size];

                    fixed (byte* ptr = fbyte)
                    {
                        Marshal.StructureToPtr(field.GetValue(struc), (nint)ptr, false);
                    }
                    var fstr = DumpArrayAsString(fbyte);
                    Console.WriteLine($"\t{field.Name,-20} ({ftyp.Name,10}) : {fstr} ");
                }
            }
        }
        internal static LEF32File Create32BitFile()
        {
            var file = new LEF32File();
            var hdr = new LEF32Header();
            var codehdr = new LEF32SectionHeader();
            var datahdr = new LEF32SectionHeader();
            var wdathdr = new LEF32SectionHeader();
            hdr.lef_Magic = LEF32Header.Magic;
            hdr.lef_Version = 1;
            hdr.lef_Machine = LEF32Header.Machine_x86;
            hdr.lef_Location = LEF32Header.Location0;
            codehdr.lef_NameChar0 = (byte)'c';
            codehdr.lef_NameChar1 = (byte)'o';
            codehdr.lef_NameChar2 = (byte)'d';
            codehdr.lef_NameChar3 = (byte)'e';
            codehdr.lef_DataLength = 0;
            codehdr.lef_DataPointer = 0;
            datahdr.lef_NameChar0 = (byte)'d';
            datahdr.lef_NameChar1 = (byte)'a';
            datahdr.lef_NameChar2 = (byte)'t';
            datahdr.lef_NameChar3 = (byte)'a';
            datahdr.lef_DataLength = 0;
            datahdr.lef_DataPointer = 0;
            wdathdr.lef_NameChar0 = (byte)'w';
            wdathdr.lef_NameChar1 = (byte)'d';
            wdathdr.lef_NameChar2 = (byte)'a';
            wdathdr.lef_NameChar3 = (byte)'t';
            wdathdr.lef_DataLength = 0;
            wdathdr.lef_DataPointer = 0;
            file.lef_Header = hdr;
            file.lef_CodeSectionHdr = codehdr;
            file.lef_DataSectionHdr = datahdr;
            file.lef_WdatSectionHdr = wdathdr;
            return file;
        }
        internal static unsafe byte[] ToByteArray<T>(T str) where T : unmanaged
        {
            byte[] bytes = new byte[sizeof(T)];
            byte* ptr = (byte*)Marshal.AllocHGlobal(sizeof(T));
            Marshal.StructureToPtr(str, (nint)ptr, false);
            for (int i = 0; i < sizeof(T); i++) bytes[i] = ptr[i];
            Marshal.FreeHGlobal((nint)ptr);
            return bytes;
        }
    }
}
