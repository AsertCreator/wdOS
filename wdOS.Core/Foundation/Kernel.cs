using Cosmos.Core;
using Cosmos.Core.Memory;
using Cosmos.Debug.Kernel;
using Cosmos.HAL;
using Cosmos.System.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using wdOS.Core.Shell.DebugShell;
using Sys = Cosmos.System;
using wdOS.Core.Shell.Services;
using wdOS.Core.Shell;
using wdOS.Core.Foundation.Network;
using static wdOS.Core.Foundation.KernelLogger;

namespace wdOS.Core.Foundation
{
    public unsafe class Kernel : Sys.Kernel
    {
        public const string SystemDriveLabel = "wdOSDisk";
        public const string AssemblyName = nameof(Core);
        public static Kernel Instance;
        public static Debugger KernelDebugger;
        public static List<Application> CurrentApplications = new();
        public static PIT.PITTimer ServiceTimer;
        public static string GetKernelVersion() => 
            SystemDatabase.BuildConstants.VersionMajor + "." +
            SystemDatabase.BuildConstants.VersionMinor + "." + 
            SystemDatabase.BuildConstants.VersionPatch;
        public static bool Is64BitCPU
        { 
            get
            {
                int ecx = 0, edx = 0, unused = 0;
                CPU.ReadCPUID(1, ref unused, ref unused, ref ecx, ref edx);
                return (edx & (1 << 30)) != 0;
            } 
        }
        protected override void BeforeRun()
        {
            try
            {
                Instance = this;
                SystemInteraction.State = SystemState.BeforeLife;
                KernelDebugger = mDebugger;

                EarlyInitialization();
                LateInitialization();

                Log("Starting user stage...");
                {
                    try
                    {
                        SystemInteraction.State = SystemState.Running;
                        Cosmos.HAL.Global.EnableInterrupts();

                        Console.WriteLine($"wdOS, version: {GetKernelVersion()}");
                        Console.WriteLine($"Current date: {SystemDatabase.GetDateAsString()}");
                        Console.WriteLine($"Current time: {SystemDatabase.GetTimeAsString()}\n");
                        SystemInteraction.ShowLoginScreen();

                        Console.WriteLine($"Starting {SystemInteraction.CurrentShell.ShellName}...");
                        SystemInteraction.CurrentShell.ShellBeforeRun();
                        while (SystemInteraction.CurrentShell.IsRunning) SystemInteraction.CurrentShell.ShellRun();
                        SystemInteraction.CurrentShell.ShellAfterRun();
                    }
                    catch (Exception e)
                    {
                        Log("User stage crash! Message: " + e.Message, LogLevel.Fatal);
                        ErrorHandler.Panic(5);
                    }
                }
                Log("Shell have exited!", LogLevel.Warning);
                ShutdownPC(false);
            }
            catch (Exception e) { Log("System crash! Message: " + e.Message, LogLevel.Fatal); ErrorHandler.Panic(5); }
        }
        public unsafe static void EarlyInitialization()
        {
            try
            {
                ErrorHandler.Initialize();
                Log("wdOS is booting, running on " + CPU.GetCPUBrandString());
                Log("Current kernel version: " + GetKernelVersion());
                Log("Current memory amount: " + GetTotalRAM());
                Console.WriteLine("Starting early components...");
                SystemInteraction.State = SystemState.Starting;
                if (!SystemDatabase.SystemSettings.EnableLogging)
                {
                    Console.WriteLine("Logging is disabled, you wont be able to use logcat utility");
                    KernelDebugger.Send("Logging is disabled!");
                }

                FileSystem.Initialize();
                if (FileSystem.VFS.Disks.Count == 0) ErrorHandler.Panic(2);
                Log($"Enabled File System: created {FileSystem.VFS.GetVolumes().Count} volumes");

                SystemInteraction.SetupSystem();

                SystemInteraction.Keyboards = Cosmos.HAL.Global.GetKeyboardDevices().ToList();
                SystemInteraction.Mice = Cosmos.HAL.Global.GetMouseDevices().ToList();
                NetworkInitialization();

                Log("Done early initialization!");
            }
            catch (Exception exc)
            {
                string msg = $"Unable to perform early initialization! Type: {exc.GetType().Name}, Message: {exc.Message}";
                Log(msg, LogLevel.Fatal);
                Console.WriteLine(msg);
                SystemInteraction.Shutdown(5);
            }
        }
        public static void EnableServices(bool enable)
        {
            SystemDatabase.SystemSettings.EnableServices = enable;
            if (enable)
            {
                foreach (var service in SystemDatabase.SSDService) service.ShellBeforeRun();
                ServiceTimer = new((_) =>
                {
                    foreach (var service in SystemDatabase.SSDService) service.ShellRun();
                },
                (ulong)(1000000 * SystemDatabase.SystemSettings.ServicePeriod), true);
                Cosmos.HAL.Global.PIT.RegisterTimer(ServiceTimer);
                Log($"Enabled Services: uses SSD database, {SystemDatabase.SSDService.Count} services");
            }
            else
            {
                Cosmos.HAL.Global.PIT.UnregisterTimer(ServiceTimer.TimerID);
                foreach (var service in SystemDatabase.SSDService) service.ShellAfterRun();
                Log($"Disabled Services: {SystemDatabase.SSDService.Count} services");
            }
        }
        public static void LateInitialization()
        {
            try
            {
                Log("Starting late initialization...");
                Console.WriteLine("Starting late components...");

                var font = SystemDatabase.SystemSettings.TerminalFonts[SystemDatabase.SystemSettings.SystemTerminalFont];

                Console.SetWindowSize(90, 30);

                VGAScreen.SetFont(font.CreateVGAFont(), font.Height);
                SystemDatabase.SSDShells.Add(new TShellManager());
                SystemDatabase.SSDService.Add(new PeriodicGC());
                SystemInteraction.CurrentShell = SystemDatabase.SSDShells[0];
                Log($"Selected shell: {SystemInteraction.CurrentShell.ShellName} - {SystemInteraction.CurrentShell.ShellDesc}");
                Log($"Enabled ShellServiceDatabase: instantiated with {SystemDatabase.SSDShells.Count} shells and {SystemDatabase.SSDService.Count} services");

                SystemInteraction.InitializeInteraction();
                SystemDatabase.InitializeDatabase();

                Log("Done late initialization!");
            }
            catch (Exception exc)
            {
                string msg = $"Unable to perform late initialization! Type: {exc.GetType().Name}, Message: {exc.Message}";
                Log(msg, LogLevel.Fatal);
                Console.WriteLine(msg);
                SystemInteraction.Shutdown(5);
            }
        }
        public static void NetworkInitialization()
        {
            try
            {
                if (SystemDatabase.SystemSettings.EnableNetwork)
                {
                    Log("Starting network initialization...");

                    NetworkManager.MainClient = new(80);
                    NetworkManager.PingClient = new(80);
                    NetworkManager.LANClient = new();
                    int time = NetworkManager.LANClient.SendDiscoverPacket();
                    Log("Got an IP Adrress in " + time + " seconds");

                    Log("Done network initialization!");
                }
            }
            catch
            {
                Log("Unable to perform network initialization!", LogLevel.Error);
                Console.WriteLine("Unable to perform network initialization!");
            }
        }
        public static CPUVendor GetCPUVendor()
        {
            string rawvendor = CPU.GetCPUVendorName();
            if (rawvendor == "GenuineIntel") return CPUVendor.Intel;
            if (rawvendor == "AuthenticAMD") return CPUVendor.Amd;
            if (rawvendor == "AMDisbetter!") return CPUVendor.Amd;
            return CPUVendor.Unknown;
        }
        public static int SweepTrash()
        {
            int result = Heap.Collect();
            return result;
        }
        public static void WaitForShutdown(bool restart, int timeout)
        {
            Console.WriteLine((restart ? "Restarting" : "Shutting down") + " in " + timeout);
            Utilities.WaitFor((uint)(timeout * 1000));
            ShutdownPC(restart);
        }
        public static void ShutdownPC(bool restart)
        {
            try
            {
                Log("Starting shutdown process...");
                SystemInteraction.Shutdown(0, restart);
                Log("Couldn't perform shutdown process!", LogLevel.Fatal);
                
                ErrorHandler.Panic(6);
            }
            catch { ErrorHandler.Panic(6); }
        }
        public static uint GetTotalRAM() => CPU.GetAmountOfRAM() * 1024 * 1024;
        public static uint GetUsedRAM() => GCImplementation.GetUsedRAM();
        protected override void Run() { }
    }
    public enum CPUVendor { Intel, Amd, Unknown }
}
