using Cosmos.Core;
using Cosmos.Core.Memory;
using Cosmos.Debug.Kernel;
using Cosmos.HAL;
using Cosmos.System.Graphics;
using Cosmos.System.Graphics.Fonts;
using Cosmos.System.Network.IPv4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using wdOS.Core.Shell.CShell;
using wdOS.Core.Shell.TShell;
using Sys = Cosmos.System;
using wdOS.Core.Shell.Services;
using wdOS.Core.Shell;
using wdOS.Core.Foundation.Network;

namespace wdOS.Core.Foundation
{
    public class Kernel : Sys.Kernel
    {
        internal const string SystemDriveLabel = "wdOSDisk";
        internal const string AssemblyName = nameof(Core);
        internal static Kernel Instance;
        internal static Debugger KernelDebugger;
        internal static ShellBase CurrentShell;
        internal static List<KeyboardBase> Keyboards;
        internal static List<MouseBase> Mice;
        internal static StringBuilder SystemLog = new();
        internal static List<ShellBase> SSDShells = new();
        internal static List<ServiceBase> SSDService = new();
        internal static PIT.PITTimer ServiceTimer;
        internal static string ComputerName = "minimal environment";
        internal static string StringTime => RTC.Hour + ":" + RTC.Minute + ":" + RTC.Second;
        internal static string KernelVersion => BuildConstants.VersionMajor + "." + BuildConstants.VersionMinor + "." + BuildConstants.VersionPatch;
        internal static uint TotalRAM => CPU.GetAmountOfRAM() * 1024 * 1024;
        internal static uint UsedRAM => GCImplementation.GetUsedRAM();
        internal static bool Is64BitCPU
        { 
            get
            {
                int ecx = 0, edx = 0, unused = 0;
                CPU.ReadCPUID(1, ref unused, ref unused, ref ecx, ref edx);
                return (edx & (1 << 30)) != 0;
            } 
        }
        internal static class BuildConstants
        {
            internal const int VersionMajor = 0;
            internal const int VersionMinor = 5;
            internal const int VersionPatch = 0;
        }
        internal static class SystemSettings
        {
            internal static int CrashPowerOffTimeout = 5;
            internal static int SystemTerminalFont = 0;
            internal static int ServicePeriod = 1000;
            internal static Address CustomAddress = null;
            internal static Address RouterAddress = null;
            internal static bool EnableAudio = false;
            internal static bool EnableLogging = true;
            internal static bool EnableNetwork = false;
            internal static bool EnableServices = true;
            internal static bool EnableFileSystem = true;
            internal static bool EnablePeriodicGC = true;
            internal static bool EnableBinaryRuntime = true;
            internal static bool CDROMBoot = true;
            internal static Dictionary<int, PCScreenFont> TerminalFonts = new()
            {
                [0] = PCScreenFont.Default
            };
        }
        protected override void BeforeRun()
        {
            try
            {
                Instance = this;
                KernelDebugger = mDebugger;
                EarlyInitialization();
                LateInitialization();
                Log("Starting user stage...");
                {
                    try
                    {
                        Cosmos.HAL.Global.EnableInterrupts();
                        Console.WriteLine($"Starting {CurrentShell.Name}...");
                        CurrentShell.BeforeRun();
                        while (CurrentShell.IsRunning) CurrentShell.Run();
                    }
                    catch (Exception e)
                    {
                        Log("User stage crash! Message: " + e.Message); 
                        ErrorHandler.Panic(5);
                    }
                }
                Log("ShellBase is exited!");
                ShutdownPC(false);
            }
            catch (Exception e) { Log("System crash! Message: " + e.Message); ErrorHandler.Panic(5); }
        }
        internal unsafe static void EarlyInitialization()
        {
            try
            {
                ErrorHandler.Initialize();
                Log("wdOS is booting, running on " + CPU.GetCPUBrandString());
                Log("Current kernel version: " + KernelVersion);
                Log("Current memory amount: " + CPU.GetAmountOfRAM());
                Console.WriteLine("Starting early components...");
                if (!SystemSettings.EnableLogging)
                {
                    Console.WriteLine("Logging is disabled, you wont be able to use logcat utility");
                    KernelDebugger.Send("Logging is disabled!");
                }
                if (SystemSettings.EnableBinaryRuntime)
                {
                    Runtime.Setup();
                    Log($"Enabled Binary Runtime");
                }
                if (SystemSettings.EnableFileSystem)
                {
                    if (HeapSmall.SMT == null) GCImplementation.Init();
                    FileSystem.Initialize();
                    if (FileSystem.VFS.Disks.Count == 0) ErrorHandler.Panic(2);
                    Log($"Enabled File System: created {FileSystem.VFS.GetVolumes().Count} volumes");
                }
                Keyboards = Cosmos.HAL.Global.GetKeyboardDevices().ToList();
                Mice = Cosmos.HAL.Global.GetMouseDevices().ToList();
                if (Keyboards.Count == 0)
                {
                    Console.WriteLine("Your PC has no attached mice. Without any mice you can't graphical shell");
                }
                if (Keyboards.Count == 0)
                {
                    Console.WriteLine("Your PC has no attached keyboards. Without any keyboards you can't use system");
                    ErrorHandler.Panic(3);
                }
                NetworkInitialization();

                Log("Done early initialization!");
            }
            catch
            {
                Log("Unable to perform early initialization!");
                Console.WriteLine("Unable to perform early initialization!");
                SystemInteraction.Shutdown(5);
            }
        }
        internal static void EnableServices(bool enable)
        {
            SystemSettings.EnableServices = enable;
            if (enable)
            {
                foreach (var service in SSDService) service.BeforeRun();
                ServiceTimer = new((_) =>
                {
                    foreach (var service in SSDService) service.Run();
                },
                (ulong)(1000000 * SystemSettings.ServicePeriod), true);
                Cosmos.HAL.Global.PIT.RegisterTimer(ServiceTimer);
                Log($"Enabled Services: uses SSD database, {SSDService.Count} services");
            }
            else
            {
                Cosmos.HAL.Global.PIT.UnregisterTimer(ServiceTimer.TimerID);
                foreach (var service in SSDService) service.AfterRun();
                Log($"Disabled Services: {SSDService.Count} services");
            }
        }
        internal static void LateInitialization()
        {
            try
            {
                Log("Starting late initialization...");
                Console.WriteLine("Starting late components...");

                var font = SystemSettings.TerminalFonts[SystemSettings.SystemTerminalFont];
                VGAScreen.SetFont(font.CreateVGAFont(), font.Height);
                SSDShells.Add(new TShellManager());
                SSDShells.Add(new CShellManager());
                SSDService.Add(new PeriodicGC());
                CurrentShell = SSDShells[0];
                Log($"Enabled ShellServiceDatabase: instantiated with {SSDShells.Count} shells and {SSDService.Count} services");
                if (SystemSettings.EnableAudio)
                {
                    AudioPlayer.AudioInitialization();
                    AudioPlayer.PlaySound(AudioPlayer.RawBootChime);
                    Log($"Enabled Audio Player: boot chime length {AudioPlayer.RawBootChime.Length}");
                }

                Log("Done late initialization!");
            }
            catch
            {
                Log("Unable to perform late initialization!");
                Console.WriteLine("Unable to perform late initialization!");
                SystemInteraction.Shutdown(5);
            }
        }
        internal static void NetworkInitialization()
        {
            try
            {
                Log("Starting network initialization...");

                if (SystemSettings.EnableNetwork)
                {
                    NetworkManager.MainClient = new(80);
                    NetworkManager.PingClient = new(80);
                    NetworkManager.LANClient = new();
                    int time = NetworkManager.LANClient.SendDiscoverPacket();
                    Log("Got an IP Adrress in " + time + " seconds");
                }

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
            Log("Sweeped" + result + " objects! Sweet!");
            return result;
        }
        internal static void Log(string text)
        {
            if (SystemSettings.EnableLogging)
            {
                string data = "[" + AssemblyName + "][" + StringTime + "] " + text;
                KernelDebugger.Send(data);
                SystemLog.Append(data + '\n');
            }
        }
        internal static void WaitForShutdown(bool restart, int timeout)
        {
            Console.WriteLine((restart ? "Restarting" : "Shutting down") + " in " + timeout);
            Utilities.WaitFor((uint)(timeout * 1000));
            ShutdownPC(restart);
        }
        internal static void ShutdownPC(bool restart)
        {
            try
            {
                Log("Starting shutdown process...");
                SystemInteraction.Shutdown(0, restart);
                Log("Couldn't perform shutdown process! Crashing...");
                ErrorHandler.Panic(6);
            }
            catch { ErrorHandler.Panic(6); }
        }
        protected override void Run() { }
    }
}
