using Cosmos.Core;
using Cosmos.HAL;
using System;
using System.Collections.Generic;
using System.Linq;
using wdOS.Core.Foundation;

namespace wdOS.Core.Shell
{
    public static unsafe class Utilities
    {
        public const string EnglishAlphabet = "qwertyuiopasdfghjklzxcvbnm";
        public static byte[] FromStructure<T>(T struc) where T : struct
        {
            byte[] bytes = new byte[512];
            byte* point = (byte*)GCImplementation.GetSafePointer(struc);
            for (int i = 0; i < 512; i++) bytes[i] = point[i];
            return bytes;
        }
        public static void WaitFor(uint timeout)
        {
            Cosmos.HAL.Global.PIT.Wait(timeout);
        }
        public static string ConcatArray(string[] args) => ConnectArgs(args, ' ');
        public static string ConnectArgs(string[] args, char sep) => string.Join(sep, args);
        public static bool HasFlag(int value, int match) => (value & match) != 0;
    }
}
