using Cosmos.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IL2CPU.API.Attribs;
using Cosmos.Core.Memory;
using XSharp.Assembler;
using XSharp;

namespace wdOS.Platform
{
    internal static unsafe class Utilities
    {
        internal const string LowerCaseEnglishAlphabet = "abcdefghijklmnopqrstuvwxyz";
        internal const string UpperCaseEnglishAlphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        internal static void WaitFor(uint timeout)
        {
            var target = DateTime.Now.AddMilliseconds(timeout);
            while (DateTime.Now < target) { }
        }
        [PlugMethod(Assembler = typeof(UtilitiesImpl))]
        internal static int Call(uint address) => 0; 
        internal static int FromOctal(byte* str, int size)
        {
            int n = 0;
            byte* c = str;
            while (size-- > 0)
            {
                n *= 8;
                n += *c - '0';
                c++;
            }
            return n;
        }
        internal static string FromCString(char* c)
        {
            StringBuilder sb = new();
            byte* ptr = (byte*)c;
            for (int i = 0; ptr[i] != 0; i++)
                sb.Append((char)(ptr[i]));
            return sb.ToString();
        }
        internal static int StringLength(byte* c)
        {
            int length = 0;
            for (int i = 0; c[i] != 0; i++)
                length++;
            return length;
        }
        internal static bool StringCheck(byte* str1, byte* str2, int length)
        {
            for (int i = 0; i < length; i++)
                if (str1[i] != str2[i])
                    return false;
            return true;
        }
        internal static char* ToCString(string str)
        {
            char* c = (char*)Heap.Alloc((uint)(str.Length + 1));
            for (int i = 0; i < str.Length; i++) c[i] = str[i];
            c[str.Length] = '\0';
            return c;
        }
        internal static string ConcatArray(string[] args) => ConnectArgs(args, ' ');
        internal static string ConnectArgs(string[] args, char sep) => string.Join(sep, args);
        internal static bool HasFlag(int value, int match) => (value & match) != 0;
    }
    internal class UtilitiesImpl : AssemblerMethod
    {
        public override void AssembleNew(Assembler aAssembler, object aMethodInfo)
        {
            XS.Set(XSRegisters.EAX, XSRegisters.ESP, sourceIsIndirect: true, sourceDisplacement: 4);
            XS.Call(XSRegisters.EAX);
            XS.Return();
        }
    }
}
