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
        internal static Shell ShellToRun = new CShell();
        internal static List<Application> Applications = new();
        internal static List<KeyboardBase> Keyboards = new();
        internal static List<MouseBase> Mice = new();
        internal static int RestartTimeout = 5;
        internal class BuildConstants
        {
            internal const int VersionMajor = 0;
            internal const int VersionMinor = 1;
            internal const int VersionPatch = 1;
            internal const bool UseMultiThreading = false;
            internal const bool UseLEFAppParsing = false;
            internal const bool UsePlainAppParsing = true;
        }
        protected override void BeforeRun()
        {
            Debugger = mDebugger;
            Log($"wdOS is booting, running on {CPU.GetCPUBrandString()}");
            Log($"Current kernel version: {GetVersion()}");
            DetectPointingDevices();
            try
            {
                Console.WriteLine("Starting components...");
                VFSManager.RegisterVFS(VFS, false);
                Console.WriteLine($"Starting {ShellToRun.Name}...");
                ShellToRun.BeforeRun();
                while (true) ShellToRun.Run();
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
            WaitFor(timeout * 1000);
            ShutdownPC(restart);
        }
        internal static void WaitFor(int timeout)
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
            catch { if (!restart) { Panic("shutdown failed!"); } else { Panic("restart failed!"); } }
        }
    }
}
