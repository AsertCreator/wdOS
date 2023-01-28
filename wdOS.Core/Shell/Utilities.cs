using Cosmos.Core;
using Cosmos.HAL;
using System;
using System.Collections.Generic;
using System.Linq;
using wdOS.Core.Foundation;

namespace wdOS.Core.Shell
{
    internal static unsafe class Utilities
    {
        internal const string EnglishAlphabet = "qwertyuiopasdfghjklzxcvbnm";
        internal static T[] SkipArray<T>(T[] array, int count)
        {
            List<T> values = array.ToList();
            if (count < values.Count)
            { for (int i = 0; i < count; i++) { values.RemoveAt(0); } }
            return values.ToArray();
        }
        internal static byte[] FromStructure<T>(T struc) where T : struct
        {
            byte[] bytes = new byte[512];
            byte* point = (byte*)GCImplementation.GetSafePointer(struc);
            for (int i = 0; i < 512; i++) bytes[i] = point[i];
            return bytes;
        }
        internal static void WaitFor(uint timeout)
        {
            Cosmos.HAL.Global.PIT.Wait(timeout);
        }
        internal static string ConnectArgs(string[] args) => ConnectArgs(args, ' ');
        internal static string ConnectArgs(string[] args, char sep) => string.Join(sep, args);
        internal static bool HasFlag(int value, int match) => (value & match) != 0;
    }
}
