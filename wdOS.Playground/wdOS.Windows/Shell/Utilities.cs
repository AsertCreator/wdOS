using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
        internal static void WaitFor(uint timeout)
        {
            Thread.Sleep((int)timeout);
        }
        internal static string ConnectArgs(string[] args) => ConnectArgs(args, ' ');
        internal static string ConnectArgs(string[] args, char sep) => string.Join(sep, args);
        internal static bool HasFlag(int value, int match) => (value & match) != 0;
    }
}
