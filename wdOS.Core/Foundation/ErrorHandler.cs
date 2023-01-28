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
            for (int i = 0; i < 20; i++) INTs.SetIntHandler((byte)i, HandleException);
        }
        internal static void Panic(uint message)
        {
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
        internal static void HandleException(ref INTs.IRQContext context)
        {
            // apparently this is never called

            // bool cancontinue = false;
            // string meaning = "program you just started tried to incorrectly interact with your system!\nunfortunately, its impossible to evaluate what problem is.";
            // string reason = $"cpu exception {context.Interrupt}";
            // switch (context.Interrupt)
            // {
            //     case 0x00: cancontinue = true; reason = "division by zero"; meaning = "program you just started tried to divide by zero!"; break;
            //     case 0x04: cancontinue = false; reason = "stack overlow"; meaning = "program you just started used too much memory!"; break;
            //     case 0x06: cancontinue = true; reason = "invalid opcode"; meaning = "program you just started is not compatible with your CPU!"; break;
            //     case 0x0D: cancontinue = true; reason = "general protection fault"; meaning = "program you just started tried to access system memory!"; break;
            //     case 0x10: cancontinue = true; reason = "x87 fpu exception"; meaning = "program you just started tried to incorrectly calculate numbers!"; break;
            //     case 0x13: cancontinue = false; reason = "SSE fpu exception!"; meaning = "program you just started tried to incorrectly calculate numbers!"; break;
            // }
            // Console.ForegroundColor = ConsoleColor.Red;
            // Log($"!!! error !!! {reason}!");
            // Log($"at address: {context.EIP}, esp: {context.ESP}, ebp: {context.EBP}");
            // Log($"Meaning: {meaning}");
            // Console.WriteLine($"!!! error !!! {reason}!");
            // Console.WriteLine($"at address: {context.EIP}, esp: {context.ESP}, ebp: {context.EBP}");
            // Console.WriteLine($"Meaning: {meaning}");
            // if (cancontinue)
            // {
            //     Console.Write("Do you want to continue? [y/n]: ");
            //     if (Console.ReadKey().Key == ConsoleKey.Y) Console.WriteLine();
            //     else ShutdownPC(false);
            //     Console.ForegroundColor = ConsoleColor.White;
            // }
            // else
            // {
            //     Console.Write("You cannot continue using system!");
            //     WaitForShutdown(true, SystemSettings.CrashPowerOffTimeout);
            // }
        }
    }
}
