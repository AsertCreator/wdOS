using Cosmos.Core.Memory;
using Cosmos.Core;
using Cosmos.HAL;
using Cosmos.System.Graphics;
using System;
using System.Linq;
using Sys = Cosmos.System;
using Cosmos.Debug.Kernel;
using static wdOS.Platform.PlatformManager;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Collections.Generic;
using Cosmos.Core.Multiboot;

namespace wdOS.Platform
{
    public unsafe class Bootstrapper : Sys.Kernel
    {
        internal const string AssemblyName = nameof(Platform);
        internal static Bootstrapper Instance;
        internal static Debugger KernelDebugger;
        internal static string BootloaderName = "Unknown";
        internal static string[] CommandLineArgs = Array.Empty<string>();
        internal static string AlterShell = "debugsh";
        internal static bool EnableStatistics = false;
        private static bool isRunning = false;
        [SupportedOSPlatform("windows")]
        internal static void EarlyMain()
        {
            if (isRunning)
            {
                Log("attempted to run new kernel instance", "bootstrap", LogLevel.Warning);
                return;
            }

            isRunning = true;

            try
            {
                PlatformManager.SessionAge = 0;
                KernelDebugger = Instance.mDebugger;

                Initialize();

                Log("starting platform application...", "bootstrap");

                LateMain();

                Log("platform application have exited!", "bootstrap", LogLevel.Warning);
                PlatformManager.Shutdown(ShutdownType.SoftShutdown, false);
            }
            catch (Exception e)
            {
                FailureManager.HandleNETException(e);
            }
        }
        internal static void LateMain()
        {
            try
            {
                PlatformManager.SessionAge = 1;
                Cosmos.HAL.Global.EnableInterrupts();

                if (EnableStatistics) Console.WriteLine("wdOS Platform, version: " + PlatformManager.GetPlatformVersion());
                else Console.WriteLine("welcome to wdOS!");

                Console.WriteLine("current date: " + PlatformManager.GetDateAsString());
                Console.WriteLine("current time: " + PlatformManager.GetTimeAsString());

                if (EnableStatistics)
                {
                    Console.WriteLine("bootloader  : " + BootloaderName);
                }

                PlatformManager.Relogin();

                ShellManager app = new();
                app.Start();
            }
            catch (Exception e)
            {
                Log("platform application crash! message: " + e.Message, "bootstrap", LogLevel.Fatal);
                FailureManager.Panic(5);
            }
        }
        [SupportedOSPlatform("windows")]
        protected override void BeforeRun()
        {
            Instance = this;
            EarlyMain();
        }
        [SupportedOSPlatform("windows")]
        internal unsafe static void Initialize()
        {
            try
            {
                var font = SystemSettings.TerminalFonts[SystemSettings.SystemTerminalFont];
                Console.SetWindowSize(90, 30);
                VGAScreen.SetFont(font.CreateVGAFont(), font.Height);

                Log("wdOS Platform is booting, running on " + CPU.GetCPUBrandString(), "bootstrap");
                Log("current kernel version: " + GetPlatformVersion(), "bootstrap");
                Log("current memory amount: " + GetTotalRAM(), "bootstrap");
                Console.WriteLine("Starting up platform components...");

                SessionAge = 0;

                CheckMultibootTags();
                ParseCommandLineArgs();

                if (!SystemSettings.EnableLogging)
                {
                    Console.WriteLine("logging is disabled!");
                    KernelDebugger.Send("logging is disabled!");
                }

                try
                {
                    string logtext;

                    HardwareManager.Initialize();
                    logtext = "initalized HardwareManager: acpi available? " + HardwareManager.ACPIAvailable;
                    Log(logtext, "bootstrap");
                    Console.WriteLine(logtext);

                    FileSystemManager.Initialize();
                    logtext = "initalized FileSystemManager: mounted volumes: " + FileSystemManager.VFS.GetVolumes().Count;
                    Log(logtext, "bootstrap");
                    Console.WriteLine(logtext);

                    PlatformManager.Initialize();
                    logtext = "initalized PlatformManager: no data";
                    Log(logtext, "bootstrap");
                    Console.WriteLine(logtext);

                    FailureManager.Initialize();
                    logtext = "initalized FailureManager: no data";
                    Log(logtext, "bootstrap");
                    Console.WriteLine(logtext);

                    UserManager.Initialize();
                    logtext = "initalized UserManager: found/created users: " + UserManager.AvailableUsers.Count;
                    Log(logtext, "bootstrap");
                    Console.WriteLine(logtext);

                    ServiceManager.Initialize();
                    logtext = "initalized ServiceManager: started services: " + ServiceManager.Services.Count;
                    Log(logtext, "bootstrap");
                    Console.WriteLine(logtext);

                    BroadcastManager.Initialize();
                    logtext = "initalized BroadcastManager: no data";
                    Log(logtext, "bootstrap");
                    Console.WriteLine(logtext);

                    RuntimeManager.Initialize();
                    logtext = "initalized RuntimeManager: no data";
                    Log(logtext, "bootstrap");
                    Console.WriteLine(logtext);
                }
                catch { }

                if (FileSystemManager.VFS.GetDisks().Count == 0) FailureManager.Panic(2);

                PlatformManager.AttachedKeyboards = Cosmos.HAL.Global.GetKeyboardDevices().ToList();
                PlatformManager.AttachedMice = Cosmos.HAL.Global.GetMouseDevices().ToList();

                Log("initialized os!", "bootstrap");
            }
            catch (Exception exc)
            {
                string msg = "unable to perform initialization! type: " + exc.GetType().Name + ", message: " + exc.Message;
                Log(msg, "bootstrap", LogLevel.Fatal);
                Console.WriteLine(msg);
                PlatformManager.Shutdown(ShutdownType.Panic, panic: 5);
            }
        }
        internal static void ParseCommandLineArgs()
        {
            try
            {
                for (int i = 0; i < CommandLineArgs.Length; i++)
                {
                    if (!string.IsNullOrEmpty(CommandLineArgs[i].Trim()))
                    {
                        switch (CommandLineArgs[i])
                        {
                            case "-v" or "--verbose":
                                VerboseMode = true;
                                break;
                            case "--no-acpi":
                                HardwareManager.ForceDisableACPI = true;
                                break;
                            case "--no-logging":
                                PlatformManager.SystemSettings.EnableLogging = false;
                                break;
                            case "--alter-shell":
                                string type = CommandLineArgs[++i];
                                AlterShell = type;
                                break;
                            default:
                                Console.WriteLine("wdOS.Platform: unknown kernel argument: " + CommandLineArgs[i]);
                                FailureManager.Panic("unknown kernel argument");
                                break;
                        }
                    }
                }
            }
            catch
            {
                Console.WriteLine("wdOS.Platform: failed to parse console arguments");
                FailureManager.Panic("failed to parse console arguments");
            }
        }
        internal static void CheckMultibootTags()
        {
            var MbAddress = (IntPtr)Multiboot2.GetMBIAddress();

            Mb2Tag* tag;

            for (tag = (Mb2Tag*)(MbAddress + 8); tag->Type != 0; tag = (Mb2Tag*)((byte*)tag + (tag->Size + 7 & ~7)))
            {
                switch (tag->Type)
                {
                    case 1:
                        CommandLineArgs = Utilities.FromCString(&((Mb2StringTag*)tag)->FirstChar).Split(' ');
                        break;
                    case 2:
                        BootloaderName = Utilities.FromCString(&((Mb2StringTag*)tag)->FirstChar);
                        break;
                    case 3:
                        var mdt = (Mb2ModuleTag*)tag;
                        var mod = new KernelModule
                        {
                            ModuleStart = mdt->ModuleStart,
                            ModuleEnd = mdt->ModuleEnd,
                            ModuleAddress = (byte*)mdt->ModuleStart,
                            Name = Utilities.FromCString(&mdt->FirstChar)
                        };
                        PlatformManager.LoadedModules.Add(mod);
                        Log($"loaded module - size: {mod.ModuleEnd - mod.ModuleStart}, addr: 0x{mod.ModuleStart:X8}, name: {mod.Name}",
                            "bootstrapper", LogLevel.Info);
                        break;
                    default:
                        break;
                }
            }
        }
        internal static void WaitForShutdown(bool restart, int timeout, bool force)
        {
            if (!force)
            {
                Console.WriteLine((restart ? "restarting" : "shutting down") + " in " + timeout);
                Utilities.WaitFor((uint)(timeout * 1000));
                PlatformManager.Shutdown(ShutdownType.SoftShutdown, restart);
            }
            else
            {
                if (restart) Power.CPUReboot();
                if (!restart) Power.ACPIShutdown();
                while (true) CPU.Halt();
            }
        }
        internal static uint GetTotalRAM() => CPU.GetAmountOfRAM() * 1024 * 1024;
        internal static uint GetUsedRAM() => GCImplementation.GetUsedRAM();
        internal static double GetUsedRAMPercentage() => GetUsedRAM() / (double)GetTotalRAM() * 100;
        protected override void Run() { }
    }
    internal unsafe class KernelModule
    {
        internal string Name;
        internal uint ModuleStart;
        internal uint ModuleEnd;
        internal byte* ModuleAddress;
    }

    [StructLayout(LayoutKind.Explicit, Size = 8)]
    internal struct Mb2Tag
    {
        [FieldOffset(0)] internal uint Type;
        [FieldOffset(4)] internal uint Size;
    }
    [StructLayout(LayoutKind.Explicit)]
    internal struct Mb2StringTag
    {
        [FieldOffset(0)] internal uint Type;
        [FieldOffset(4)] internal uint Size;
        [FieldOffset(8)] internal char FirstChar;
    }
    [StructLayout(LayoutKind.Explicit)]
    internal struct Mb2ModuleTag
    {
        [FieldOffset(0)] internal uint Type;
        [FieldOffset(4)] internal uint Size;
        [FieldOffset(8)] internal uint ModuleStart;
        [FieldOffset(12)] internal uint ModuleEnd;
        [FieldOffset(16)] internal char FirstChar;
    }
    [StructLayout(LayoutKind.Explicit, Size = 8)]
    internal struct Mb2FramebufferTag
    {
        [FieldOffset(0)] internal uint Type;
        [FieldOffset(4)] internal uint Size;
        [FieldOffset(8)] internal ulong FramebufferAddress;
        [FieldOffset(16)] internal uint FramebufferPitch;
        [FieldOffset(20)] internal uint FramebufferWidth;
        [FieldOffset(24)] internal uint FramebufferHeight;
        [FieldOffset(28)] internal byte FramebufferDepth;
        [FieldOffset(29)] internal byte FramebufferType;
        [FieldOffset(30)] internal byte Reserved;
    }
}
