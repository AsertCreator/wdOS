using Cosmos.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static wdOS.Platform.Bootstrapper;

namespace wdOS.Platform
{
    internal static class FailureManager
    {
        internal static Dictionary<uint, string> ErrorTexts = new()
        {
            [0] = "MANUALLY_INITIATED_CRASH",
            [1] = "INVALID_CPU_OPCODE",
            [2] = "NO_BLOCK_DEVICES",
            [3] = "NO_INPUT_DEVICES",
            [4] = "GENERAL_PROTECTION_FAULT",
            [5] = "SYSTEM_EXCEPTION",
            [6] = "SYSTEM_SHUTDOWN",
            [7] = "INVALID_CPUID"
        };
        internal static int FailureDepth = 0;
        internal static bool initialized = false;
        internal static void Initialize()
        {
            if (!initialized)
            {
                PlatformManager.Log("set up basic error handling!", "failuremanager");
                initialized = true;
            }
        }
        internal static void HandleNETException(Exception exc)
        {
            string text = "unhandled platform exception, type: " + exc.GetType().Name + ", msg: " + exc.Message;
            Console.WriteLine(text);
            Panic(text);
        }
        internal static void Panic(uint message)
        {
            PlatformManager.SessionAge = 3;
            if (FailureDepth != 1)
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.BackgroundColor = ConsoleColor.Black;
                string text0 = "!!! panic !!! " + ErrorTexts[message];
                string text1 = "current kernel version: " + PlatformManager.GetPlatformVersion();
                PlatformManager.Log(text0, "failuremanager", LogLevel.Fatal);
                PlatformManager.Log(text1, "failuremanager", LogLevel.Fatal);
                Console.WriteLine(text0);
                Console.WriteLine(text1);
                WaitForShutdown(true, PlatformManager.SystemSettings.CrashPowerOffTimeout, true);
                FailureDepth++;
            }
            else
            {
                PlatformManager.Shutdown(ShutdownType.HardShutdown);
            }
        }
        internal static void Panic(string msg)
        {
            PlatformManager.SessionAge = 3;
            if (FailureDepth != 1)
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.BackgroundColor = ConsoleColor.Black;
                string text0 = "!!! panic !!! message: " + msg;
                string text1 = "current kernel version: " + PlatformManager.GetPlatformVersion();
                PlatformManager.Log(text0, "failuremanager", LogLevel.Fatal);
                PlatformManager.Log(text1, "failuremanager", LogLevel.Fatal);
                Console.WriteLine(text0);
                Console.WriteLine(text1);
                WaitForShutdown(true, PlatformManager.SystemSettings.CrashPowerOffTimeout, true);
                FailureDepth++;
            }
            else
            {
                if (FailureDepth <= 1) ACPIManager.ForceShutdownPC();
                while (true) { }
            }
        }
    }
}
