using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static wdOS.Platform.Foundation.PlatformManager;

namespace wdOS.Platform.Foundation
{
    public static class PlatformLogger
    {
        public static void Log(string message, LogLevel level = LogLevel.Info)
        {
            if (SystemSettings.EnableLogging)
            {
                string data = "[" + Bootstrapper.AssemblyName + "][" + GetLogLevelAsString(level) +
                    "][" + GetTimeAsString() + "] " + message;
                Bootstrapper.KernelDebugger.Send(data);
                SystemLog.Append(data + '\n');
            }
        }
        public static string GetLogLevelAsString(LogLevel level) => level switch
        {
            LogLevel.Info => "Info ",
            LogLevel.Warning => "Warn ",
            LogLevel.Error => "Error",
            LogLevel.Fatal => "Fatal",
            _ => "Info ",
        };
    }
    public enum LogLevel { Info, Warning, Error, Fatal }
}
