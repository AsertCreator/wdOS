using Cosmos.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IL2CPU.API.Attribs;
using Cosmos.Core.Memory;

namespace wdOS.Platform
{
    internal static unsafe class Utilities
    {
        internal const string LowerCaseEnglishAlphabet = "abcdefghijklmnopqrstuvwxyz";
        internal const string UpperCaseEnglishAlphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        internal static void WaitFor(uint timeout)
        {
            Cosmos.HAL.Global.PIT.Wait(timeout);
        }
        internal static int Call(uint address) => 0;
        internal static string FromCString(char* c)
        {
            StringBuilder sb = new();
            byte* ptr = (byte*)c;
            for (int i = 0; ptr[i] != 0; i++)
                sb.Append((char)(ptr[i]));
            return sb.ToString();
        }
        // uses process's memory
        internal static char* ToCStringU(string str)
        {
            char* c = (char*)PlatformManager.CurrentProcess.Alloc((uint)(str.Length + 1));
            for (int i = 0; i < str.Length; i++) c[i] = str[i];
            c[str.Length] = '\0';
            return c;
        }
        // uses kernel memory
        internal static char* ToCStringK(string str)
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
    internal static class UtilitiesImpl
    {

    }
}
