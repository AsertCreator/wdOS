using System.Collections.Generic;
using System.Linq;

namespace wdOS.Core
{
    internal static class Utilities
    {
        internal static T[] SkipArray<T>(T[] array, int count)
        {
            List<T> values = array.ToList();
            if (count < values.Count)
            { for (int i = 0; i < count; i++) { values.RemoveAt(0); } }
            return values.ToArray();
        }
        internal static string ConnectArgs(string[] args) => ConnectArgs(args, ' ');
        internal static string ConnectArgs(string[] args, char sep) => string.Join(sep, args);
        internal static bool HasFlag(int value, int match) => (value & match) != 0;
    }
}
