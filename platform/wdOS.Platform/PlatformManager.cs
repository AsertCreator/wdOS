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
using wdOS.Pillow;

namespace wdOS.Platform
{
    public static partial class PlatformManager
    {
        public static Process KernelProcess;
        public static Process CurrentProcess;
        public static List<Process> AllProcesses;
        public static List<KeyboardBase> AttachedKeyboards;
        public static List<MouseBase> AttachedMice;
        public static List<KernelModule> LoadedModules = new();
        public static StringBuilder SystemLog = new();
        public static bool LoadUsersFromDisk = false;
        public static bool VerboseMode = false;
        public static bool LogIntoConsole = true;
        public static int SessionAge = 0;
        public static int FailureDepth = 0;
        public static Dictionary<uint, string> ErrorTexts = new()
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
        public static void Log(string message, string component, LogLevel level = LogLevel.Info)
        {
            if (SystemSettings.EnableLogging)
            {
                string data = "[" + component + "][" + GetLogLevelAsString(level) +
                    "][" + GetTimeAsString() + "] " + message;
                Bootstrapper.KernelDebugger.Send(data);
                SystemLog.Append(data + '\n');
                if (LogIntoConsole || VerboseMode) Console.WriteLine(data);
            }
        }
        public static StringBuilder GetSystemLog() => SystemLog;
        public static string GetLogLevelAsString(LogLevel level) => level switch
        {
            LogLevel.Info => "info ",
            LogLevel.Warning => "warn ",
            LogLevel.Error => "error",
            LogLevel.Fatal => "fatal",
            _ => "unknw",
        };
        public static int GetCPUBitWidth()
        {
            int ecx = 0, edx = 0, unused = 0;
            CPU.ReadCPUID(1, ref unused, ref unused, ref ecx, ref edx);
            return (edx & 1 << 30) != 0 ? 64 : 32;
        }
        public static string GetPlatformVersion() =>
            BuildConstants.VersionMajor + "." +
            BuildConstants.VersionMinor + "." +
            BuildConstants.VersionPatch;
        public static int AllocPID() => nextpid++;
        public static int Execute(string path, string cmd)
        {
            Process process = new();
            int result = int.MinValue;

            process.IsRunning = true;
            process.PID = AllocPID();

            PlatformManager.AllProcesses.Add(process);

            try
            {
                if (!FileSystemManager.FileExists(path)) return result;

                byte[] bytes = FileSystemManager.ReadBytesFile(path);
                EEExecutable executable = ExecutionEngine.Load(bytes);

                var funcres = executable.Execute(cmd);

                if (funcres.IsExceptionUnwinding)
                {
                    OnProcessCrash(funcres);
                    return result;
                }

                if (funcres.ReturnedValue.ObjectType == ExecutionEngine.ObjectTypeInteger)
                    result = (int)funcres.ReturnedValue.ObjectValue;
                else
                    result = 0;

                process.IsRunning = false;
            }
            catch
            {
                process.IsRunning = false;
                OnProcessCrash(null);
            }
            return result;
        }
        public static void OnProcessCrash(EEFunctionResult res)
        {
            // todo: process crash handler
        }
        public static unsafe void Initialize()
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
        public static void HandleNETException(Exception exc)
        {
            string text = "unhandled platform exception, type: " + exc.GetType().Name + ", msg: " + exc.Message;
            Console.WriteLine(text);
            Panic(text);
        }
        public static void Panic(uint message)
        {
            PlatformManager.SessionAge = 3;

            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.BackgroundColor = ConsoleColor.Black;

            string text0 = "!!! panic !!! " + ErrorTexts[message];
            string text1 = "current kernel version: " + PlatformManager.GetPlatformVersion();

            PlatformManager.Log(text0, "failuremanager", LogLevel.Fatal);
            PlatformManager.Log(text1, "failuremanager", LogLevel.Fatal);

            Console.WriteLine(text0);
            Console.WriteLine(text1);

            Bootstrapper.WaitForShutdown(true, PlatformManager.SystemSettings.CrashPowerOffTimeout, true);
            while (true) { }
        }
        public static void Panic(string msg)
        {
            PlatformManager.SessionAge = 3;

            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.BackgroundColor = ConsoleColor.Black;

            string text0 = "!!! panic !!! message: " + msg;
            string text1 = "current kernel version: " + PlatformManager.GetPlatformVersion();

            PlatformManager.Log(text0, "failuremanager", LogLevel.Fatal);
            PlatformManager.Log(text1, "failuremanager", LogLevel.Fatal);

            Console.WriteLine(text0);
            Console.WriteLine(text1);

            Bootstrapper.WaitForShutdown(true, PlatformManager.SystemSettings.CrashPowerOffTimeout, true);
            while (true) { }
        }
        public static string GetTimeAsString() =>
            (RTC.Hour < 10 ? "0" + RTC.Hour : RTC.Hour) + ":" +
            (RTC.Minute < 10 ? "0" + RTC.Minute : RTC.Minute) + ":" +
            (RTC.Second < 10 ? "0" + RTC.Second : RTC.Second);
        public static string GetDateAsString() =>
            (RTC.DayOfTheMonth < 10 ? "0" + RTC.DayOfTheMonth : RTC.DayOfTheMonth) + "." +
            (RTC.Month < 10 ? "0" + RTC.Month : RTC.Month) + "." + RTC.Year;
        public static class SystemSettings
        {
            public static int CrashPowerOffTimeout = 5;
            public static int SystemTerminalFont = 0;
            public static int ServicePeriod = 1000;
            public static Address CustomAddress = null;
            public static Address RouterAddress = null;
            public static bool EnableAudio = false;
            public static bool EnableLogging = true;
            public static bool EnableNetwork = false;
            public static bool EnableVerbose = false;
            public static bool EnableServices = false;
            public static bool EnablePeriodicGC = true;
            public static bool RamdiskAsRoot = false;
            public static Dictionary<int, PCScreenFont> TerminalFonts = new()
            {
                [0] = PCScreenFont.Default
            };
        }
        public static class BuildConstants
        {
            public const int VersionMajor = 0;
            public const int VersionMinor = 10;
            public const int VersionPatch = 0;
            public const int CurrentType = TypePreBeta;
            public const int TypePreAlpha = 0;
            public const int TypeAlpha = 0;
            public const int TypePreBeta = 1;
            public const int TypeBeta = 2;
            public const int TypePreRelease = 3;
            public const int TypeRelease = 4;
        }
        public static void Relogin()
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
            Console.WriteLine("logged in as " + UserManager.CurrentUser.Username);
        }
        public static void Shutdown(ShutdownType type, bool halt = false, uint panic = 0)
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
                    while (true) CPU.Halt();
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
                    while (true) CPU.Halt();
                case ShutdownType.HardShutdown:
                    if (halt)
                        while (true) CPU.Halt();

                    Power.ACPIShutdown();

                    Console.WriteLine("shutdown failed! cpu halted");
                    while (true) CPU.Halt();
                case ShutdownType.HardRestart:
                    if (halt)
                        while (true) CPU.Halt();

                    Power.CPUReboot();

                    Console.WriteLine("restart failed! cpu halted");
                    while (true) CPU.Halt();
                case ShutdownType.Panic:
                    Panic(panic);
                    while (true) CPU.Halt();
                case ShutdownType.Halt:
                    Console.WriteLine("cpu halted");
                    while (true) CPU.Halt();
            }
        }
    }
    public enum ShutdownType
    {
        SoftShutdown, SoftRestart, HardShutdown, HardRestart, Panic, Halt
    }
    public enum LogLevel { Info, Warning, Error, Fatal }
}
