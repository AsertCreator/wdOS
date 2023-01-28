using Cosmos.Core;
using System;
using System.Collections.Generic;
using static wdOS.Core.Foundation.Kernel;

namespace wdOS.Core.Foundation
{
    // not working lol
    internal static class ErrorHandler
    {
        internal static Dictionary<uint, string> ErrorTexts;
        internal static void Initialize()
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
            INTs.GeneralProtectionFault = delegate(ref INTs.IRQContext ctx)
            {
                Console.BackgroundColor = ConsoleColor.Black;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Current application tried to execute forbidden instruction");
                Console.WriteLine("This application will be terminated");
                Panic(4);
            };
        }
        internal static void Panic(uint message)
        {
            SystemInteraction.State = SystemState.AfterLife;
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.BackgroundColor = ConsoleColor.Black;
            string text0 = $"!!! panic !!! {ErrorTexts[message]}";
            string text1 = $"Current kernel version: {KernelVersion}";
            Log(text0);
            Log(text1);
            Console.WriteLine(text0);
            Console.WriteLine(text1);
            WaitForShutdown(true, SystemSettings.CrashPowerOffTimeout);
        }
        internal static void Panic(string msg)
        {
            SystemInteraction.State = SystemState.AfterLife;
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.BackgroundColor = ConsoleColor.Black;
            string text0 = $"!!! panic !!! custom: {msg}";
            string text1 = $"Current kernel version: {KernelVersion}";
            Log(text0);
            Log(text1);
            Console.WriteLine(text0);
            Console.WriteLine(text1);
            WaitForShutdown(true, SystemSettings.CrashPowerOffTimeout);
        }
    }
}
