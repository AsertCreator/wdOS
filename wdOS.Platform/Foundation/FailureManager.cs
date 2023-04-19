using Cosmos.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static wdOS.Platform.Foundation.Bootstrapper;

namespace wdOS.Platform.Foundation
{
    // not working lol
    public static class FailureManager
    {
        public static Dictionary<uint, string> ErrorTexts;
        private static int FailureDepth = 0;
        private static bool initialized = false;
        public static void Initialize()
        {
            if (!initialized)
            {
                ErrorTexts = new()
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
                INTs.GeneralProtectionFault = delegate (ref INTs.IRQContext ctx)
                {
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Current application tried to execute forbidden instruction");
                    Console.WriteLine("This application will be terminated");
                    Panic(4);
                };

                PlatformLogger.Log("Set up basic error handling!");
                initialized = true;
            }
        }
        public static void HandleNETException(Exception exc)
        {
            // todo: this
        }
        public static void Panic(uint message)
        {
            PlatformManager.CurrentSystemState = PlatformManager.SystemState.AfterLife;
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.BackgroundColor = ConsoleColor.Black;
            string text0 = $"!!! panic !!! {ErrorTexts[message]}";
            string text1 = $"Current kernel version: {PlatformManager.GetPlatformVersion()}";
            PlatformLogger.Log(text0);
            PlatformLogger.Log(text1);
            Console.WriteLine(text0);
            Console.WriteLine(text1);
            WaitForShutdown(true, PlatformManager.SystemSettings.CrashPowerOffTimeout);
            FailureDepth++;
        }
        public static void Panic(string msg)
        {
            PlatformManager.CurrentSystemState = PlatformManager.SystemState.AfterLife;
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.BackgroundColor = ConsoleColor.Black;
            string text0 = $"!!! panic !!! message: {msg}";
            string text1 = $"Current kernel version: {PlatformManager.GetPlatformVersion()}";
            PlatformLogger.Log(text0);
            PlatformLogger.Log(text1);
            Console.WriteLine(text0);
            Console.WriteLine(text1);
            WaitForShutdown(true, PlatformManager.SystemSettings.CrashPowerOffTimeout);
            FailureDepth++;
        }
    }
}
