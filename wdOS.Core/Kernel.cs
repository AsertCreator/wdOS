using Cosmos.Core;
using Cosmos.Core.Memory;
using Cosmos.Debug.Kernel;
using Cosmos.HAL;
using Cosmos.System.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using wdOS.Core.Shells;
using wdOS.Core.Shells.TShell;
using Sys = Cosmos.System;

namespace wdOS.Core
{
    public class Kernel : Sys.Kernel
    {
        internal const string SystemDriveLabel = "wdOSDisk";
        internal const string AssemblyName = nameof(Core);
        internal static Debugger Debugger;
        internal static Shell ShellToRun = new TShell();
        internal static List<Application> Applications = new();
        internal static List<KeyboardBase> Keyboards = new();
        internal static List<MouseBase> Mice = new();
        internal static StringBuilder SystemLog = new();
        internal static string StringTime => $"{RTC.Hour}:{RTC.Minute}:{RTC.Second}";
        internal static string KernelVersion => $"{BuildConstants.VersionMajor}.{BuildConstants.VersionMinor}.{BuildConstants.VersionPatch}";
        internal static uint TotalRAM => CPU.GetAmountOfRAM() * 1024 * 1024;
        internal static uint UsedRAM => GCImplementation.GetUsedRAM();
        internal static class BuildConstants
        {
            internal const int VersionMajor = 0;
            internal const int VersionMinor = 2;
            internal const int VersionPatch = 1;
        }
        internal static class SystemSettings
        {
            internal static int CrashRestartTimeout = 5;
        }
        internal static Dictionary<uint, string> PanicStrings = new()
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
        protected override void BeforeRun()
        {
            try
            {
                // early initialization
                {
                    Debugger = mDebugger;
                    for (int i = 0; i < 32; i++) INTs.SetIntHandler((byte)i, HandleException);
                    Log($"wdOS is booting, running on {CPU.GetCPUBrandString()}");
                    Log($"Current kernel version: {KernelVersion}");
                }
                Log("Done early initialization...");
                Log("Starting late initialization...");
                // late initialization
                {
                    Console.WriteLine("Starting components...");
                    /*1*/
                    var font = Sys.Graphics.Fonts.PCScreenFont.Default; // cosmetics, like new vga font
                    /*1*/
                    VGAScreen.SetFont(font.CreateVGAFont(), font.Height);
                    /* 2*/
                    DetectInputDevices(); // detect input devices, such as keyboard and mouse
                    /*  3*/
                    FileSystem.Initialize(); // initialize file system manager
                    if (FileSystem.VFS.Disks.Count == 0) Panic(2); // if no disks are found, then panic
                }
                Log("Done late initialization...");
            }
            catch (Exception e) { Log($"Initialization failed! Message: {e.Message}"); Panic(5); }
            try
            {
                Log("Starting user stage...");
                // shell startup
                {
                    Console.WriteLine($"Starting {ShellToRun.Name}...");
                    ShellToRun.BeforeRun();
                    while (ShellToRun.IsRunning) ShellToRun.Run();
                }
                Log("Shell is exited!");
                ShutdownPC(false);
            }
            catch (Exception e) { Log($"Shell crashed! Message: {e.Message}"); Panic(5); }
        }
        internal static void DetectInputDevices()
        {
            Keyboards = Cosmos.HAL.Global.GetKeyboardDevices().ToList();
            Mice = Cosmos.HAL.Global.GetMouseDevices().ToList();
            if (Keyboards.Count == 0)
            {
                Console.WriteLine("Error! Your PC has no attahced keyboards. Without any keyboards you can't use system");
                Panic(3);
            }
        }
        internal static void WaitForShutdown(bool restart, int timeout)
        {
            Console.WriteLine($"{(restart ? "Restarting" : "Shutting down")} in {timeout}");
            WaitFor(timeout * 1000);
            ShutdownPC(restart);
        }
        internal static void WaitFor(int timeout)
        {
            DateTime target = DateTime.Now.AddMilliseconds(timeout);
            while (DateTime.Now < target) { }
        }
        internal static void Panic(uint message)
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.BackgroundColor = ConsoleColor.Black;
            Log($"!!! panic !!! {PanicStrings[message]}");
            Log($"Current kernel version: {KernelVersion}");
            Console.WriteLine($"!!! panic !!! {PanicStrings[message]}");
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
        internal static int SweepTrash()
        {
            Log("Performing garbage cleaning...");
            int result = Heap.Collect();
            Log($"Sweeped {result} objects! Sweet!");
            return result;
        }
        internal static void Log(string text) { Debugger.Send($"[{AssemblyName}][{StringTime}] {text}"); _ = SystemLog.Append(text + '\n'); }
        protected override void Run() { }
        internal static void ShutdownPC(bool restart)
        {
            try
            {
                Log("Starting shutdown process...");
                // TODO: Shutdown something
                Log("Done shutdown process!");
                if (!restart) { Sys.Power.QemuShutdown(); Sys.Power.Shutdown(); }
                else { Sys.Power.QemuReboot(); Sys.Power.Reboot(); }
            }
            catch { if (!restart) { Panic(6); } else { Panic(6); } }
        }
    }
}
