﻿using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Cosmos.HAL;
using Cosmos.Core;
using Cosmos.System.Graphics;
using Cosmos.Core.Multiboot;
using Cosmos.Debug.Kernel;
using static wdOS.Platform.Core.PlatformManager;
using wdOS.Platform.Core;

namespace wdOS.Platform
{
    public unsafe class Bootstrapper
    {
        public const string AssemblyName = nameof(Platform);
        public static Debugger KernelDebugger;
        public static string BootloaderName = "Unknown";
        public static string[] CommandLineArgs = Array.Empty<string>();
        public static string AlterShell = "debugsh";
        public static bool EnableStatistics = false;
        private static bool isRunning = false;
        [SupportedOSPlatform("windows")]
        public static void Main(Debugger debugger)
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
                KernelDebugger = debugger;

                Initialize();

                Log("starting platform application...", "bootstrap");

                LateInitialization();
            }
            catch
            {
                Console.WriteLine("critical error");
                WaitForShutdown(true, 5, false);
            }
        }
        public static void LateInitialization()
        {
            try
            {
                SystemSettings.LogIntoConsole = false;

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

                PlatformManager.AskForLogin();
            }
            catch (Exception e)
            {
                Log("late initialization failure! message: " + e.Message, "bootstrap", LogLevel.Fatal);
                PlatformManager.Panic("late initialization failure");
            }
        }
        [SupportedOSPlatform("windows")]
        public unsafe static void Initialize()
        {
            try
            {
                var font = SystemSettings.TerminalFonts[SystemSettings.SystemTerminalFont];
                Console.SetWindowSize(90, 30);
                VGAScreen.SetFont(font.CreateVGAFont(), font.Height);

				SessionAge = 0;

				if (!SystemSettings.EnableLogging)
				{
					Console.WriteLine("logging is disabled!");
					KernelDebugger.Send("logging is disabled!");
				}

				Log("wdOS Platform is booting, running on " + CPU.GetCPUBrandString(), "bootstrap");
                Log("current kernel version: " + GetPlatformVersion(), "bootstrap");
                Log("current memory amount: " + GetTotalRAM(), "bootstrap");

                CheckMultibootTags();
                ParseCommandLineArgs();

                try
                {
                    HardwareManager.Initialize();
                    Log("initalized HardwareManager: acpi available? " + HardwareManager.ACPIAvailable, "bootstrap");

                    FileSystemManager.Initialize();
                    Log("initalized FileSystemManager: mounted volumes: " + FileSystemManager.VFS.GetVolumes().Count, "bootstrap");

                    PlatformManager.Initialize();
                    Log("initalized PlatformManager: no data", "bootstrap");

                    UserManager.Initialize();
                    Log("initalized UserManager: found/created users: " + UserManager.GetUserCount(true), "bootstrap");

                    ServiceManager.Initialize();
                    Log("initalized ServiceManager: started services: " + ServiceManager.Services.Count, "bootstrap");

                    BroadcastManager.Initialize();
                    Log("initalized BroadcastManager: no data", "bootstrap");

                    RuntimeManager.Initialize();
                    Log("initalized RuntimeManager: no data", "bootstrap");
                }
                catch { }

                if (FileSystemManager.VFS.GetDisks().Count == 0) PlatformManager.Panic("no disks attached");

                Log("initialized os!", "bootstrap");
            }
            catch (Exception exc)
            {
                string msg = "unable to perform initialization! type: " + exc.GetType().Name + ", message: " + exc.Message;
                Log(msg, "bootstrap", LogLevel.Fatal);
                Console.WriteLine(msg); 
                PlatformManager.Panic("initialization failed");
			}
        }
        public static void ParseCommandLineArgs()
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
                                SystemSettings.VerboseMode = true;
                                break;
                            case "-no-acpi":
                                HardwareManager.ForceDisableACPI = true;
                                break;
                            case "-no-logging":
                                SystemSettings.EnableLogging = false;
                                break;
                            case "-shell":
                                string type = CommandLineArgs[++i];
                                AlterShell = type;
                                break;
                            default:
                                Console.WriteLine("wdOS.Platform: unknown kernel argument: " + CommandLineArgs[i]);
                                PlatformManager.Panic("unknown kernel argument");
                                break;
                        }
                    }
                }
            }
            catch
            {
                Console.WriteLine("wdOS.Platform: failed to parse console arguments");
                PlatformManager.Panic("failed to parse console arguments");
            }
        }
        public static void CheckMultibootTags()
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
        public static void WaitForShutdown(bool restart, int timeout, bool force)
        {
            if (!force)
            {
                Console.WriteLine((restart ? "restarting" : "shutting down") + " in " + timeout);
                Utilities.WaitFor((uint)(timeout * 1000));
                PlatformManager.ShutdownSystem(ShutdownType.SoftShutdown, restart);
            }
            else
            {
                if (restart) Power.CPUReboot();
                if (!restart) Power.ACPIShutdown();
                while (true) CPU.Halt();
            }
        }
        public static uint GetTotalRAM() => CPU.GetAmountOfRAM() * 1024 * 1024;
        public static uint GetUsedRAM() => GCImplementation.GetUsedRAM();
        public static double GetUsedRAMPercentage() => GetUsedRAM() / (double)GetTotalRAM() * 100;
    }
    public unsafe class KernelModule
    {
        public string Name;
        public uint ModuleStart;
        public uint ModuleEnd;
        public byte* ModuleAddress;
    }

    [StructLayout(LayoutKind.Explicit, Size = 8)]
    public struct Mb2Tag
    {
        [FieldOffset(0)] public uint Type;
        [FieldOffset(4)] public uint Size;
    }
    [StructLayout(LayoutKind.Explicit)]
    public struct Mb2StringTag
    {
        [FieldOffset(0)] public uint Type;
        [FieldOffset(4)] public uint Size;
        [FieldOffset(8)] public char FirstChar;
    }
    [StructLayout(LayoutKind.Explicit)]
    public struct Mb2ModuleTag
    {
        [FieldOffset(0)] public uint Type;
        [FieldOffset(4)] public uint Size;
        [FieldOffset(8)] public uint ModuleStart;
        [FieldOffset(12)] public uint ModuleEnd;
        [FieldOffset(16)] public char FirstChar;
    }
    [StructLayout(LayoutKind.Explicit, Size = 8)]
    public struct Mb2FramebufferTag
    {
        [FieldOffset(0)] public uint Type;
        [FieldOffset(4)] public uint Size;
        [FieldOffset(8)] public ulong FramebufferAddress;
        [FieldOffset(16)] public uint FramebufferPitch;
        [FieldOffset(20)] public uint FramebufferWidth;
        [FieldOffset(24)] public uint FramebufferHeight;
        [FieldOffset(28)] public byte FramebufferDepth;
        [FieldOffset(29)] public byte FramebufferType;
        [FieldOffset(30)] public byte Reserved;
    }
}
