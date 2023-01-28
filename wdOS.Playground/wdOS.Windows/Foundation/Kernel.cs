using System.Text;
using wdOS.Core.Shell.TShell;
using wdOS.Core.Shell;
using System.Collections.Generic;
using System;

namespace wdOS.Core.Foundation
{
    public unsafe class Kernel
    {
        internal const string SystemDriveLabel = "wdOSDisk";
        internal const string AssemblyName = nameof(Core);
        internal static Kernel Instance;
        internal static ShellBase CurrentShell;
        internal static List<ShellBase> SSDShells = new();
        internal static List<ServiceBase> SSDService = new();
        internal static List<Application> CurrentApplications = new();
        internal static StringBuilder SystemLog = new();
        internal static string ComputerName = "minimal environment";
        internal static string StringTime => DateTime.Now.Hour + ":" + DateTime.Now.Minute + ":" + DateTime.Now.Second;
        internal static string KernelVersion => BuildConstants.VersionMajor + "." + BuildConstants.VersionMinor + "." + BuildConstants.VersionPatch;
        internal static uint TotalRAM => 0;
        internal static uint UsedRAM => 0;
        internal static bool Is64BitCPU => Environment.Is64BitOperatingSystem;
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
            internal static bool EnableAudio = false;
            internal static bool EnableLogging = true;
            internal static bool EnableNetwork = false;
            internal static bool EnableVerbose = false;
            internal static bool EnableServices = false;
            internal static bool EnableFileSystem = true;
            internal static bool EnablePeriodicGC = true;
        }
        internal void BeforeRun()
        {
            try
            {
                Instance = this;
                SystemInteraction.State = SystemState.BeforeLife;
                EarlyInitialization();
                LateInitialization();
                Log("Starting user stage...");
                {
                    try
                    {
                        SystemInteraction.State = SystemState.Running;
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
                Log("Shell is exited!");
                ShutdownPC(false);
            }
            catch (Exception e) { Log("System crash! Message: " + e.Message); ErrorHandler.Panic(5); }
        }
        internal unsafe static void EarlyInitialization()
        {
            try
            {
                ErrorHandler.Initialize();
                Log("wdOS is booting, running on Windows Userspace");
                Log("Current kernel version: " + KernelVersion);
                Log("Current memory amount is unknown");
                Console.WriteLine("Starting early components...");
                SystemInteraction.State = SystemState.Starting;

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
                Log($"Enabled Services: uses SSD database, {SSDService.Count} services");
            }
            else
            {
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

                SSDShells.Add(new TShellManager());
                CurrentShell = SSDShells[0];
                Log($"Enabled ShellServiceDatabase: instantiated with {SSDShells.Count} shells and {SSDService.Count} services");

                Log("Done late initialization!");
            }
            catch
            {
                Log("Unable to perform late initialization!");
                Console.WriteLine("Unable to perform late initialization!");
                SystemInteraction.Shutdown(5);
            }
        }
        internal static int SweepTrash()
        {
            Log("Performing garbage cleaning...");
            GC.Collect();
            Log("Sweeped garbage! Sweet!");
            return 0;
        }
        internal static void Log(string text)
        {
            // no logging there
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
        internal void Run() { }
    }
}
