using Cosmos.Core;
using Cosmos.Core.Memory;
using Cosmos.HAL;
using Cosmos.System.FileSystem.VFS;
using Cosmos.System.Graphics.Fonts;
using Cosmos.System.Network.IPv4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wdOS.Platform
{
    internal static partial class PlatformManager
    {
        internal static Process KernelProcess;
        internal static Process CurrentProcess;
        internal static List<Process> AllProcesses;
        internal static List<KeyboardBase> AttachedKeyboards;
        internal static List<MouseBase> AttachedMice;
        internal static List<KernelModule> LoadedModules = new();
        internal static StringBuilder SystemLog = new();
        internal static bool LoadUsersFromDisk = false;
        internal static bool VerboseMode = false;
        internal static int SessionAge = 0;
        internal static int FailureDepth = 0;
        internal static Dictionary<uint, string> ErrorTexts = new()
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
        private static int nextpid = 0;
        private static bool initialized = false;
        internal static void Log(string message, string component, LogLevel level = LogLevel.Info)
        {
            if (SystemSettings.EnableLogging)
            {
                string data = "[" + component + "][" + GetLogLevelAsString(level) +
                    "][" + GetTimeAsString() + "] " + message;
                if (VerboseMode) Console.WriteLine(data);
                Bootstrapper.KernelDebugger.Send(data);
                SystemLog.Append(data + '\n');
            }
        }
        internal static StringBuilder GetSystemLog() => SystemLog;
        internal static string GetLogLevelAsString(LogLevel level) => level switch
        {
            LogLevel.Info => "info ",
            LogLevel.Warning => "warn ",
            LogLevel.Error => "error",
            LogLevel.Fatal => "fatal",
            _ => "unknw",
        };
        internal static int GetCPUBitWidth()
        {
            int ecx = 0, edx = 0, unused = 0;
            CPU.ReadCPUID(1, ref unused, ref unused, ref ecx, ref edx);
            return (edx & 1 << 30) != 0 ? 64 : 32;
        }
        internal static string GetPlatformVersion() =>
            BuildConstants.VersionMajor + "." +
            BuildConstants.VersionMinor + "." +
            BuildConstants.VersionPatch;
        internal static int AllocPID() => nextpid++;
        internal static unsafe void Initialize()
        {
            if (!initialized)
            {
                PlatformManager.Log("setting up system folders...", "platformmanager");
                for (int i = 0; i < FileSystemManager.SystemFolders.Length; i++)
                    FileSystemManager.CreateDirectory("0:\\" + FileSystemManager.SystemFolders[i]);

                KernelProcess = new Process
                {
                    BinaryPath = "<kernel>",
                    ConsoleArguments = "",
                    IsRunning = true
                };
                KernelProcess.Executor = KernelProcess;
                CurrentProcess = KernelProcess;

                PlatformManager.Log("set up basic platform!", "platformmanager");
                initialized = true;
            }
        }
        internal static void HandleNETException(Exception exc)
        {
            string text = "unhandled platform exception, type: " + exc.GetType().Name + ", msg: " + exc.Message;
            Console.WriteLine(text);
            Panic(text);
        }
        internal static void Panic(uint message)
        {
            PlatformManager.SessionAge = 3;
            if (FailureDepth != 1)
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.BackgroundColor = ConsoleColor.Black;
                string text0 = "!!! panic !!! " + ErrorTexts[message];
                string text1 = "current kernel version: " + PlatformManager.GetPlatformVersion();
                PlatformManager.Log(text0, "failuremanager", LogLevel.Fatal);
                PlatformManager.Log(text1, "failuremanager", LogLevel.Fatal);
                Console.WriteLine(text0);
                Console.WriteLine(text1);
                Bootstrapper.WaitForShutdown(true, PlatformManager.SystemSettings.CrashPowerOffTimeout, true);
                FailureDepth++;
            }
            else
            {
                PlatformManager.Shutdown(ShutdownType.HardShutdown);
            }
        }
        internal static void Panic(string msg)
        {
            PlatformManager.SessionAge = 3;
            if (FailureDepth != 1)
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.BackgroundColor = ConsoleColor.Black;
                string text0 = "!!! panic !!! message: " + msg;
                string text1 = "current kernel version: " + PlatformManager.GetPlatformVersion();
                PlatformManager.Log(text0, "failuremanager", LogLevel.Fatal);
                PlatformManager.Log(text1, "failuremanager", LogLevel.Fatal);
                Console.WriteLine(text0);
                Console.WriteLine(text1);
                Bootstrapper.WaitForShutdown(true, PlatformManager.SystemSettings.CrashPowerOffTimeout, true);
                FailureDepth++;
            }
            else
            {
                if (FailureDepth <= 1) HardwareManager.ForceShutdownPC();
                while (true) { }
            }
        }
        internal static string GetTimeAsString() =>
            (RTC.Hour < 10 ? "0" + RTC.Hour : RTC.Hour) + ":" +
            (RTC.Minute < 10 ? "0" + RTC.Minute : RTC.Minute) + ":" +
            (RTC.Second < 10 ? "0" + RTC.Second : RTC.Second);
        internal static string GetDateAsString() =>
            (RTC.DayOfTheMonth < 10 ? "0" + RTC.DayOfTheMonth : RTC.DayOfTheMonth) + "." +
            (RTC.Month < 10 ? "0" + RTC.Month : RTC.Month) + "." + RTC.Year;
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
            internal static bool EnableVerbose = false;
            internal static bool EnableServices = false;
            internal static bool EnablePeriodicGC = true;
            internal static bool RamdiskAsRoot = false;
            internal static Dictionary<int, PCScreenFont> TerminalFonts = new()
            {
                [0] = PCScreenFont.Default
            };
        }
        internal static class BuildConstants
        {
            internal const int VersionMajor = 0;
            internal const int VersionMinor = 10;
            internal const int VersionPatch = 0;
            internal const int CurrentType = TypePreBeta;
            internal const int TypePreAlpha = 0;
            internal const int TypeAlpha = 0;
            internal const int TypePreBeta = 1;
            internal const int TypeBeta = 2;
            internal const int TypePreRelease = 3;
            internal const int TypeRelease = 4;
        }
        internal static void Relogin()
        {
            if (UserManager.AvailableUsers.Count == 1)
            {
                var user = UserManager.AvailableUsers[0];
                if (user.UserLockType != 0)
                {
                retry:
                    Console.Write("login: " + user.UserName);
                    Console.Write("password: ");
                    string password = Console.ReadLine();
                    if (UserManager.Login(user.UserName, password) != UserManager.UserLoginResultLoggedInto)
                    {
                        Console.WriteLine("invalid credentials\n");
                        goto retry;
                    }
                }
                else
                {
                    if (UserManager.Login(user.UserName, "", true) != UserManager.UserLoginResultLoggedInto)
                    {
                        Console.WriteLine("corrupted user database\n");
                    }
                }
            }
            else
            {
            retry:
                Console.Write("login: ");
                string username = Console.ReadLine();
                Console.Write("password: ");
                string password = Console.ReadLine();
                if (UserManager.Login(username, password) != UserManager.UserLoginResultLoggedInto)
                {
                    Console.WriteLine("invalid credentials\n");
                    goto retry;
                }
            }
            Console.WriteLine("logged in as " + UserManager.CurrentUser.UserName);
        }
        internal static void Shutdown(ShutdownType type, bool halt = false, uint panic = 0)
        {
            Console.WriteLine('\n'); // double new line
            SessionAge = 2;

            switch (type)
            {
                case ShutdownType.SoftShutdown:
                    Console.WriteLine("shutting down...");

                    BroadcastManager.SaveBroadcasts();
                    ConfigurationManager.SaveSystemConfig();

                    AllProcesses.Clear();
                    Heap.Collect();

                    if (halt)
                        while (true) CPU.Halt();

                    Power.ACPIShutdown();

                    Console.WriteLine("shutdown failed! cpu halted");
                    goto case ShutdownType.Halt;
                case ShutdownType.SoftRestart:
                    Console.WriteLine("restarting...");

                    BroadcastManager.SaveBroadcasts();
                    ConfigurationManager.SaveSystemConfig();

                    AllProcesses.Clear();
                    Heap.Collect();

                    if (halt)
                        while (true) CPU.Halt();

                    Power.CPUReboot();

                    Console.WriteLine("restart failed! cpu halted");
                    goto case ShutdownType.Halt;
                case ShutdownType.HardShutdown:
                    if (halt)
                        while (true) CPU.Halt();

                    Power.ACPIShutdown();

                    Console.WriteLine("shutdown failed! cpu halted");
                    goto case ShutdownType.Halt;
                case ShutdownType.HardRestart:
                    if (halt)
                        while (true) CPU.Halt();

                    Power.CPUReboot();

                    Console.WriteLine("restart failed! cpu halted");
                    goto case ShutdownType.Halt;
                case ShutdownType.Panic:
                    Panic(panic);
                    goto case ShutdownType.Halt;
                case ShutdownType.Halt:
                default:
                    while (true) CPU.Halt();
            }
        }
    }
    public enum ShutdownType
    {
        SoftShutdown, SoftRestart, HardShutdown, HardRestart, Panic, Halt
    }
    internal enum LogLevel { Info, Warning, Error, Fatal }
}
