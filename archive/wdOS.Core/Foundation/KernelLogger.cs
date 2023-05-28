using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using static wdOS.Core.Foundation.SystemDatabase;

namespace wdOS.Core.Foundation
{
    public static class KernelLogger
    {
        public static void Log(string message, LogLevel level = LogLevel.Info)
        {
            if (SystemSettings.EnableLogging)
            {
                string data = "[" + Kernel.AssemblyName + "][" + GetLogLevelAsString(level) + 
                    "][" + GetTimeAsString() + "] " + message;
                Kernel.KernelDebugger.Send(data);
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
