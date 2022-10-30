using Cosmos.Core;
using Cosmos.Core.Memory;
using Cosmos.Debug.Kernel;
using Cosmos.HAL;
using Cosmos.System.Graphics;
using Cosmos.System.Graphics.Fonts;
using Cosmos.System.Network.IPv4;
using Cosmos.System.Network.IPv4.UDP.DHCP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using wdOS.Core.OS.Shells;
using wdOS.Core.OS.Shells.CShell;
using wdOS.Core.OS.Shells.TShell;
using Sys = Cosmos.System;

namespace wdOS.Core.OS.Foundation
{
    public class Kernel : Sys.Kernel, IPackage
    {
        internal const string SystemDriveLabel = "wdOSDisk";
        internal const string AssemblyName = nameof(Core);
        internal static Debugger KernelDebugger;
        internal static Shell CurrentShell;
        internal static List<KeyboardBase> Keyboards = new();
        internal static List<MouseBase> Mice = new();
        internal static StringBuilder SystemLog = new();
        internal static string StringTime => $"{RTC.Hour}:{RTC.Minute}:{RTC.Second}";
        internal static string KernelVersion => $"{BuildConstants.VersionMajor}.{BuildConstants.VersionMinor}.{BuildConstants.VersionPatch}";
        internal static uint TotalRAM => CPU.GetAmountOfRAM() * 1024 * 1024;
        internal static uint UsedRAM => GCImplementation.GetUsedRAM();
        string IPackage.Name => "wdOS";
        string IPackage.Description => "System core - controls OS";
        string[] IPackage.Files => new string[] { "wdOS.Core.bin" };
        int IPackage.MajorVersion => BuildConstants.VersionMajor;
        int IPackage.MinorVersion => BuildConstants.VersionMinor;
        int IPackage.PatchVersion => BuildConstants.VersionPatch;
        PackageDatabase.PackageType IPackage.Type => PackageDatabase.PackageType.SystemShell;
        internal static class BuildConstants
        {
            internal const int VersionMajor = 0;
            internal const int VersionMinor = 4;
            internal const int VersionPatch = 0;
            internal const string CosmosCosmosHash = "???????";
            internal const string CosmosXSharpHash = "???????";
            internal const string CosmosIL2CPUHash = "???????";
            internal const string CosmosCommonHash = "???????";
        }
        internal static class SystemSettings
        {
            internal static int CrashPowerOffTimeout = 5;
            internal static int SystemTerminalFont = 0;
            internal static Address CustomAddress = null;
            internal static Address RouterAddress = null;
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
            internal static Dictionary<int, PCScreenFont> TerminalFonts = new()
            {
                [0] = PCScreenFont.Default
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
        internal void EarlyInitialization()
        {
            try 
            {
                for (int i = 0; i < 20; i++) INTs.SetIntHandler((byte)i, ErrorHandler.HandleException);
                Log($"wdOS is booting, running on {CPU.GetCPUBrandString()}");
                Log($"Current kernel version: {KernelVersion}");
                Console.WriteLine("Starting early components...");

                FileSystem.Initialize();
                if (FileSystem.VFS.Disks.Count == 0) Panic(2);
                Keyboards = Cosmos.HAL.Global.GetKeyboardDevices().ToList();
                Mice = Cosmos.HAL.Global.GetMouseDevices().ToList();
                if (Keyboards.Count == 0)
                {
                    Console.WriteLine("Error! Your PC has no attahced keyboards. Without any keyboards you can't use system");
                    Panic(3);
                }
                NetworkInitialization();

                Log("Done early initialization!");
            }
            catch
            {
                Log("Unable to perform early initialization!");
                Console.WriteLine("Unable to perform early initialization!");
                Panic(5);
            }
        }
        internal void LateInitialization()
        {
            try
            {
                Log("Starting late initialization...");
                Console.WriteLine("Starting late components...");

                var font = PCScreenFont.Default;
                VGAScreen.SetFont(font.CreateVGAFont(), font.Height);
                PackageDatabase.Packages.Add(this);
                PackageDatabase.Packages.Add(new TShellManager());
                PackageDatabase.Packages.Add(new CShellManager());
                CurrentShell = (Shell)PackageDatabase.Packages[1];
                AudioPlayer.Setup();

                Log("Done late initialization!");
            }
            catch
            {
                Log("Unable to perform late initialization!");
                Console.WriteLine("Unable to perform late initialization!");
                Panic(5);
            }
        }
        internal static void NetworkInitialization()
        {
            try
            {
                Log("Starting network initialization...");

                using var dchp = new DHCPClient();
                int time = dchp.SendDiscoverPacket();

                Log("Done network initialization!");
            }
            catch
            {
                Log("Unable to perform network initialization!");
                Console.WriteLine("Unable to perform network initialization!");
            }
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
        internal static void WaitFor(long timeout)
        {
            DateTime target = DateTime.Now.AddMilliseconds(timeout);
            while (DateTime.Now < target) { }
        }
        internal static void ShutdownPC(bool restart)
        {
            try
            {
                Log("Starting shutdown process...");
                // todo: shutdown something
                Log("Done shutdown process!");
                if (!restart) { Sys.Power.Shutdown(); }
                else { Sys.Power.Reboot(); }
            }
            catch { if (!restart) { Panic(6); } else { Panic(6); } }
        }
        protected override void Run() { }
    }
    internal enum LogType { Info, Debug, Warning, Error }
}
