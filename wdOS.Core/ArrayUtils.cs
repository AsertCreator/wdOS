using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wdOS.Core
{
    internal static class ArrayUtils
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
    }
}
