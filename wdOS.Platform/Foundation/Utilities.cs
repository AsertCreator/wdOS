using Cosmos.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wdOS.Platform.Foundation
{
    public static unsafe class Utilities
    {
        public const string EnglishAlphabet = "qwertyuiopasdfghjklzxcvbnm";
        public static void WaitFor(uint timeout)
        {
            Cosmos.HAL.Global.PIT.Wait(timeout);
        }
        public static string ConcatArray(string[] args) => ConnectArgs(args, ' ');
        public static string ConnectArgs(string[] args, char sep) => string.Join(sep, args);
        public static bool HasFlag(int value, int match) => (value & match) != 0;
    }
}
