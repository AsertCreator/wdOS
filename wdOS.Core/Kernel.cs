using Cosmos.Core;
using Cosmos.Debug.Kernel;
using Cosmos.HAL;
using Cosmos.System.FileSystem;
using Cosmos.System.FileSystem.VFS;
using System;
using System.Collections.Generic;
using System.Linq;
using wdOS.Core.Shells;
using wdOS.Core.Shells.UI;
using wdOS.Core.Threading;
using Sys = Cosmos.System;

namespace wdOS.Core
{
    public class Kernel : Sys.Kernel
    {
        internal const string SystemDriveLabel = "wdOSDisk";
        internal const string AssemblyName = nameof(Core);
        internal static Debugger Debugger;
        internal static CosmosVFS VFS = new();
        internal static Shell ShellToRun = new TShell();
        internal static List<Application> Applications = new();
        internal static List<KeyboardBase> Keyboards = new();
        internal static List<MouseBase> Mice = new();
        internal static int RestartTimeout = 5;
        internal const int VersionMajor = 1;
        internal const int VersionMinor = 0;
        internal const int VersionPatch = 0;
        internal const bool UseMultiThreading = false;
        internal const bool UseLEFAppParsing = false;
        internal const bool UsePlainAppParsing = true;
        protected override void BeforeRun()
        {
            Debugger = mDebugger;
            Log($"wdOS is booting, running on {CPU.GetCPUBrandString()}");
            Log($"Current kernel version: {GetVersion()}");
            DetectPointingDevices();
            try
            {
                Log("Checking MT compatibility");
                if (UseMultiThreading)
                {
                    ProcessorScheduler.StartMultiThreading();
                    Log("MT is compatible!");
                }
                if (UseLEFAppParsing)
                {
                    ProcessorScheduler.StartMultiThreading();
                    Log("MT is compatible!");
                }
                else Log("MT is not compatible");
                Console.WriteLine("Starting components...");
                VFSManager.RegisterVFS(VFS, false);
                if (UseMultiThreading)
                {
                    Log("Starting Shell thread...");
                    ProcessorScheduler.CreateThread(0, "Shell", () =>
                    {
                        ShellToRun.BeforeRun();
                        while (true)
                        {
                            ShellToRun.Run();
                        }
                    });
                    while (true) { Console.WriteLine("MT Error!"); }
                }
                else
                {
                    Log("Starting Shell...");
                    ShellToRun.BeforeRun();
                    while (true)
                    {
                        ShellToRun.Run();
                    }
                }
            }
            catch (Exception e) { Log($"Kernel crashed! Message: {e.Message}"); Panic(e.Message); }
        }
        internal static void DetectPointingDevices()
        {
            Keyboards = Cosmos.HAL.Global.GetKeyboardDevices().ToList();
            Mice = Cosmos.HAL.Global.GetMouseDevices().ToList();
            if (Keyboards.Count == 0)
            {
                Console.WriteLine("Error! Your PC has no attahced keyboards. Without any keyboards you " +
                    "can't use system");
                WaitForShutdown(false, RestartTimeout);
            }
        }
        internal static void WaitForShutdown(bool restart, int timeout)
        {
            Console.WriteLine($"{(restart ? "Restarting" : "Shutting down")} in {timeout}");
            WaitForSeconds(timeout);
            ShutdownPC(restart);
        }
        internal static void WaitForSeconds(int timeout)
        {
            DateTime target = DateTime.Now.AddSeconds(timeout);
            while (DateTime.Now < target) { }
        }
        internal static void WaitForMilliSeconds(int timeout)
        {
            DateTime target = DateTime.Now.AddMilliseconds(timeout);
            while (DateTime.Now < target) { }
        }
        internal static void Panic(string message)
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.BackgroundColor = ConsoleColor.Black;
            Log($"Kernel died with message: {message}");
            Log($"Kenrel version: {GetVersion()}");
            Console.WriteLine($"!!! panic \"{message}\", apps loaded: {/*LoadedApps.Count*/0} !!!");
            WaitForShutdown(true, RestartTimeout + 10);
        }
        internal static string GetStrTime() => $"{RTC.Hour}:{RTC.Minute}:{RTC.Second}";
        internal static string GetVersion() => $"{VersionMajor}.{VersionMinor}.{VersionPatch}";
        internal static void Log(string text) => Debugger.Send($"[{AssemblyName}][{GetStrTime()}] {text}");
        protected override void Run() { }
        internal static int ShutdownProcess()
        {
            try
            {
                Log("Starting shutdown process...");
                // TODO: shutdown somethings
                Log("Done shutdown process!");
                return 0;
            }
            catch { Log("Shutdown process failed!"); return -1; }
        }
        internal static void ShutdownPC(bool restart)
        {
            try
            {
                if (ShutdownProcess() == -1)
                { Panic("shutdown process failed!"); }
                if (!restart) { Sys.Power.Shutdown(); Sys.Power.QemuShutdown(); }
                else { Sys.Power.Reboot(); Sys.Power.QemuReboot(); }
            }
            catch { if (!restart) { Panic("shutdown failed!"); } else { Panic("restart failed!"); } }
        }
    }
}
