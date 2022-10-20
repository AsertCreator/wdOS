using Cosmos.Core;
using Cosmos.Core.Memory;
using Cosmos.Debug.Kernel;
using Cosmos.HAL;
using Cosmos.System.Audio;
using Cosmos.System.Audio.IO;
using Cosmos.System.Graphics;
using IL2CPU.API.Attribs;
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
        internal static Debugger KernelDebugger;
        internal static Shell CurrentShell = new TShell();
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
            internal const int VersionPatch = 3;
        }
        internal static class SystemSettings
        {
            internal static int CrashRestartTimeout = 5;
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
        }
        protected override void BeforeRun()
        {
            try
            {
                KernelDebugger = mDebugger;
                EarlyInitialization();
                LateInitialization();
                Log("Starting user stage...");
                {
                    Console.WriteLine($"Starting {CurrentShell.Name}...");
                    CurrentShell.BeforeRun();
                    while (CurrentShell.IsRunning) CurrentShell.Run();
                }
                Log("Shell is exited!");
                ShutdownPC(false);
            }
            catch (Exception e) { Log($"Execution crash! Message: {e.Message}"); Panic(5); }
        }
        internal static void EarlyInitialization()
        {
            for (int i = 0; i < 20; i++) INTs.SetIntHandler((byte)i, ErrorHandler.HandleException);
            Log($"wdOS is booting, running on {CPU.GetCPUBrandString()}");
            Log($"Current kernel version: {KernelVersion}"); 
            Log("Done early initialization!");
        }
        internal static void LateInitialization()
        {
            Log("Starting late initialization...");
            AudioPlayer.Setup();
            Console.WriteLine("Starting components...");
            var font = Sys.Graphics.Fonts.PCScreenFont.Default;
            VGAScreen.SetFont(font.CreateVGAFont(), font.Height);
            Keyboards = Cosmos.HAL.Global.GetKeyboardDevices().ToList();
            Mice = Cosmos.HAL.Global.GetMouseDevices().ToList();
            if (Keyboards.Count == 0)
            {
                Console.WriteLine("Error! Your PC has no attahced keyboards. Without any keyboards you can't use system");
                Panic(3);
            }
            FileSystem.Initialize(); // initialize file system manager
            if (FileSystem.VFS.Disks.Count == 0) Panic(2); // if no disks are found, then panic
            Log("Done late initialization!");
        }
        internal static int SweepTrash()
        {
            Log("Performing garbage cleaning...");
            int result = Heap.Collect();
            Log($"Sweeped {result} objects! Sweet!");
            return result;
        }
        internal static void Log(string text) { string data = $"[{AssemblyName}][{StringTime}] {text}"; KernelDebugger.Send(data); _ = SystemLog.Append(data + '\n'); }
        internal static void Panic(uint message) => ErrorHandler.Panic(message);
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
        internal static void ShutdownPC(bool restart)
        {
            try
            {
                Log("Starting shutdown process...");
                // TODO: Shutdown something
                Log("Done shutdown process!");
                if (!restart) { Sys.Power.Shutdown(); }
                else { Sys.Power.Reboot(); }
            }
            catch { if (!restart) { Panic(6); } else { Panic(6); } }
        }
        protected override void Run() { }
    }
}
