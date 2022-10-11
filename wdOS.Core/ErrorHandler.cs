using Cosmos.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static wdOS.Core.Kernel;

namespace wdOS.Core
{
    internal static class ErrorHandler
    {
        internal static void Panic(uint message)
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.BackgroundColor = ConsoleColor.Black;
            Log($"!!! panic !!! {SystemSettings.PanicStrings[message]}");
            Log($"Current kernel version: {KernelVersion}");
            Console.WriteLine($"!!! panic !!! {SystemSettings.PanicStrings[message]}");
            Console.WriteLine($"Current kernel version: {KernelVersion}");
            WaitForShutdown(true, SystemSettings.CrashRestartTimeout);
        }
        internal static void HandleException(ref INTs.IRQContext context)
        {
            bool cancontinue = false;
            string meaning = "program you just started tried to incorrectly interact with your system!\nUnfortunately, its impossible to evaluate what problem is.";
            string reason = $"cpu exception {context.Interrupt}";
            switch (context.Interrupt)
            {
                case 0x00: cancontinue = true; reason = "division by zero"; meaning = "program you just started tried to divide by zero!"; break;
                case 0x04: cancontinue = false; reason = "stack overlow"; meaning = "program you just started used too much memory!"; break;
                case 0x06: cancontinue = true; reason = "invalid opcode"; meaning = "program you just started is not compatible with your CPU!"; break;
                case 0x0D: cancontinue = true; reason = "general protection fault"; meaning = "program you just started tried to access system memory!"; break;
                case 0x10: cancontinue = true; reason = "x87 fpu exception"; meaning = "program you just started tried to incorrectly calculate numbers!"; break;
                case 0x13: cancontinue = false; reason = "SSE fpu exception!"; meaning = "program you just started tried to incorrectly calculate numbers!"; break;
            }
            Console.ForegroundColor = ConsoleColor.Red;
            Log($"!!! error !!! {reason}!");
            Log($"at address: {context.EIP}, esp: {context.ESP}, ebp: {context.EBP}");
            Log($"Meaning: {meaning}");
            Console.WriteLine($"!!! error !!! {reason}!");
            Console.WriteLine($"at address: {context.EIP}, esp: {context.ESP}, ebp: {context.EBP}");
            Console.WriteLine($"Meaning: {meaning}");
            if (cancontinue)
            {
                Console.Write("Do you want to continue? [y/n]: ");
                if (Console.ReadKey().Key == ConsoleKey.Y) Console.WriteLine();
                else ShutdownPC(false);
                Console.ForegroundColor = ConsoleColor.White;
            }
            else
            {
                Console.Write("You cannot continue using system!");
                WaitForShutdown(true, SystemSettings.CrashRestartTimeout);
            }
        }
    }
}
