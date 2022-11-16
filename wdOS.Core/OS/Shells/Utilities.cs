using Cosmos.Core;
using System.Collections.Generic;
using System.Linq;
using wdOS.Core.OS.Foundation;

namespace wdOS.Core.OS.Shells
{
    internal static unsafe class Utilities
    {
        internal static T[] SkipArray<T>(T[] array, int count)
        {
            List<T> values = array.ToList();
            if (count < values.Count)
            { for (int i = 0; i < count; i++) { values.RemoveAt(0); } }
            return values.ToArray();
        }
        internal static string GetLogTypeAsString(LogType type) => type switch
        {
            LogType.Info => "INFO",
            LogType.Debug => "DEBG",
            LogType.Warning => "WARN",
            LogType.Error => "ERRO",
            _ => "UNKW",
        };
        internal static byte[] FromStructure<T>(T struc) where T : struct
        {
            byte[] bytes = new byte[512];
            byte* point = (byte*)GCImplementation.GetSafePointer(struc);
            for (int i = 0; i < 512; i++) bytes[i] = point[i];
            return bytes;
        }
        internal static 
        internal static string ConnectArgs(string[] args) => ConnectArgs(args, ' ');
        internal static string ConnectArgs(string[] args, char sep) => string.Join(sep, args);
        internal static bool HasFlag(int value, int match) => (value & match) != 0;
    }
}
