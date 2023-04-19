using Cosmos.Core.Memory;
using Cosmos.Core;
using Cosmos.HAL;
using Cosmos.System.Graphics;
using System;
using System.Linq;
using Sys = Cosmos.System;
using Cosmos.Debug.Kernel;
using static wdOS.Platform.Foundation.PlatformLogger;
using wdOS.Platform.Shell;

namespace wdOS.Platform.Foundation
{
    public unsafe class Bootstrapper : Sys.Kernel
    {
        public const string AssemblyName = nameof(Platform);
        public static Bootstrapper Instance;
        public static Debugger KernelDebugger;
        private static bool isRunning = false;
        public static bool Is64BitCPU
        {
            get
            {
                int ecx = 0, edx = 0, unused = 0;
                CPU.ReadCPUID(1, ref unused, ref unused, ref ecx, ref edx);
                return (edx & 1 << 30) != 0;
            }
        }
        public static void EarlyMain()
        {
            if (isRunning)
            {
                Log("attempted to run new kernel instance", LogLevel.Warning);
                return;
            }

            isRunning = true;

            try
            {
                PlatformManager.CurrentSystemState = PlatformManager.SystemState.BeforeLife;
                KernelDebugger = Instance.mDebugger;

                Initialize();

                Log("Starting user stage...");
                {
                    var lateargs = PlatformManager.GatherConsoleArguments();
                    LateMain(lateargs);
                }
                Log("Platform application have exited!", LogLevel.Warning);
                PlatformManager.Shutdown(-1, false);
            }
            catch (Exception e) { Log("System crash! Message: " + e.Message, LogLevel.Fatal); FailureManager.Panic(5); }
        }
        public static void LateMain(string[] args)
        {
            try
            {
                PlatformManager.CurrentSystemState = PlatformManager.SystemState.Running;
                Cosmos.HAL.Global.EnableInterrupts();

                Console.WriteLine($"wdOS Platform, version: {PlatformManager.GetPlatformVersion()}");
                Console.WriteLine($"Current date: {PlatformManager.GetDateAsString()}");
                Console.WriteLine($"Current time: {PlatformManager.GetTimeAsString()}\n");
                PlatformManager.Relogin();

                Console.WriteLine("Starting platform application...");

                PlatformApplication app = new();
                app.Start();
            }
            catch (Exception e)
            {
                Log("User stage crash! Message: " + e.Message, LogLevel.Fatal);
                FailureManager.Panic(5);
            }
        }
        protected override void BeforeRun()
        {
            Instance = this;
            EarlyMain();
        }
        public unsafe static void Initialize()
        {
            try
            {
                Log("wdOS Platform is booting, running on " + CPU.GetCPUBrandString());
                Log("Current kernel version: " + PlatformManager.GetPlatformVersion());
                Log("Current memory amount: " + GetTotalRAM());
                Console.WriteLine("Starting up platform components...");

                PlatformManager.CurrentSystemState = PlatformManager.SystemState.Starting;

                if (!PlatformManager.SystemSettings.EnableLogging)
                {
                    Console.WriteLine("Logging is disabled, you wont be able to use logcat utility");
                    KernelDebugger.Send("Logging is disabled!");
                }

                try
                {
                    FileSystemManager.Initialize();
                    PlatformManager.Initialize();
                    ServiceManager.Initialize();
                    FailureManager.Initialize();
                    UserManager.Initialize();
                }
                catch { }

                if (FileSystemManager.VFS.Disks.Count == 0) FailureManager.Panic(2);
                Log($"Enabled File System: created {FileSystemManager.VFS.GetVolumes().Count} volumes");
                Log($"Enabled Platform: no data");
                Log($"Enabled Services: created {ServiceManager.Services.Count} services");
                Log($"Enabled Failures: no data");
                Log($"Enabled Users: created {UserManager.AvailableUsers.Count} users");

                PlatformManager.AttachedKeyboards = Cosmos.HAL.Global.GetKeyboardDevices().ToList();
                PlatformManager.AttachedMice = Cosmos.HAL.Global.GetMouseDevices().ToList();

                var font = PlatformManager.SystemSettings.TerminalFonts[PlatformManager.SystemSettings.SystemTerminalFont];

                Console.SetWindowSize(90, 30);

                VGAScreen.SetFont(font.CreateVGAFont(), font.Height);

                Log("Initialized OS!");
            }
            catch (Exception exc)
            {
                string msg = $"Unable to perform initialization! Type: {exc.GetType().Name}, Message: {exc.Message}";
                Log(msg, LogLevel.Fatal);
                Console.WriteLine(msg);
                PlatformManager.Shutdown(5);
            }
        }
        public static void WaitForShutdown(bool restart, int timeout)
        {
            Console.WriteLine((restart ? "Restarting" : "Shutting down") + " in " + timeout);
            Utilities.WaitFor((uint)(timeout * 1000));
            PlatformManager.Shutdown(01, restart);
        }
        public static uint GetTotalRAM() => CPU.GetAmountOfRAM() * 1024 * 1024;
        public static uint GetUsedRAM() => GCImplementation.GetUsedRAM();
        public static double GetUsedRAMPercentage() => GetUsedRAM() / (double)GetTotalRAM() * 100;
        protected override void Run() { }
    }
    public enum CPUVendor { Intel, Amd, Unknown }
}
