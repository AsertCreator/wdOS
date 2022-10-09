using Cosmos.Core;
using Cosmos.Debug.Kernel;
using Cosmos.HAL;
using Cosmos.System.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using wdOS.Core.Shells;
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
        internal static int RestartTimeout = 5;
        internal static class BuildConstants
        {
            internal const int VersionMajor = 0;
            internal const int VersionMinor = 1;
            internal const int VersionPatch = 1;
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
            Debugger = mDebugger;
            var font = Sys.Graphics.Fonts.PCScreenFont.Default;
            VGAScreen.SetFont(font.CreateVGAFont(), font.Height);
            Log($"wdOS is booting, running on {CPU.GetCPUBrandString()}");
            Log($"Current kernel version: {GetVersion()}");
            DetectPointingDevices();
            try
            {
                Console.WriteLine("Starting components...");
                FileSystemManager.Initialize();
                if (FileSystemManager.VFS.Disks.Count == 0) Panic(2);
                Console.WriteLine($"Starting {ShellToRun.Name}...");
                ShellToRun.BeforeRun();
                while (true) ShellToRun.Run();
            }
            catch (Exception e) { Log($"Kernel crashed! Message: {e.Message}"); Panic(5); }
        }
        internal static void DetectPointingDevices()
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
            Log($"Kernel died with message: {PanicStrings[message]}");
            Log($"Kernel version: {GetVersion()}");
            Console.WriteLine($"!!! panic \"{PanicStrings[message]}\" !!!");
            WaitForShutdown(true, RestartTimeout);
        }
        internal static string GetStrTime() => $"{RTC.Hour}:{RTC.Minute}:{RTC.Second}";
        internal static string GetVersion() => $"{BuildConstants.VersionMajor}.{BuildConstants.VersionMinor}.{BuildConstants.VersionPatch}";
        internal static void Log(string text) => Debugger.Send($"[{AssemblyName}][{GetStrTime()}] {text}");
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
        internal static string CanonicalPath(bool doubleup, params string[] xpath)
        {
            if (doubleup) return Path.GetFullPath(Path.Combine(xpath).Replace("..", "..\\.."));
            return Path.GetFullPath(Path.Combine(xpath));
        }
        internal bool HasFlag(int value, int match) => (value & match) != 0;
    }
}
