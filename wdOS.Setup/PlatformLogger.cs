using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wdOS.Setup.Platform
{
    internal static class PlatformLogger
    {
        internal static StringBuilder SystemLog = new();
        internal static bool VerboseMode = false;
        internal static void Log(string message, string component, LogLevel level = LogLevel.Info)
        {
            if (PlatformManager.SystemSettings.EnableLogging)
            {
                string data = "[" + component + "][" + GetLogLevelAsString(level) +
                    "][" + PlatformManager.GetTimeAsString() + "] " + message;
                if (VerboseMode) Console.WriteLine(data);
                Bootstrapper.KernelDebugger.Send(data);
                SystemLog.Append(data + '\n');
            }
        }
        internal static StringBuilder GetSystemLog() => SystemLog;
        internal static string GetLogLevelAsString(LogLevel level) => level switch
        {
            LogLevel.Info => "info ",
            LogLevel.Warning => "warn ",
            LogLevel.Error => "error",
            LogLevel.Fatal => "fatal",
            _ => "unknw",
        };
    }
    internal enum LogLevel { Info, Warning, Error, Fatal }
}
