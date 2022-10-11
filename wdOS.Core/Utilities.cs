using System.Collections.Generic;
using System.IO;
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
        internal static string ConnectArgs(string[] args, char sep)
        {
            string text = "";
            foreach (string str in args)
            { text += str + sep; }
            return text;
        }
        internal static string CanonicalPath(bool doubleup, params string[] xpath)
        {
            if (doubleup) return Path.GetFullPath(Path.Combine(xpath).Replace("..", "..\\.."));
            return Path.GetFullPath(Path.Combine(xpath));
        }
        internal static bool HasFlag(int value, int match) => (value & match) != 0;
    }
}
