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

namespace wdOS.Platform.Core
{
    public static partial class PlatformManager
    {
        public static Process KernelProcess;
        public static List<Process> AllProcesses;
        public static List<KernelModule> LoadedModules = new();
        public static StringBuilder SystemLog = new();
        public static int SessionAge = 0;
        private static int nextpid = 0;
        private static bool initialized = false;
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
        public static void Log(string message, string component, LogLevel level = LogLevel.Info)
        {
            if (SystemSettings.EnableLogging)
            {
                string data = "[" + component + "][" + level switch
                {
                    LogLevel.Info => "info ",
                    LogLevel.Warning => "warn ",
                    LogLevel.Error => "error",
                    LogLevel.Fatal => "fatal",
                    _ => "unknw",
                } + "][" + GetTimeAsString() + "] " + message;
                Bootstrapper.KernelDebugger.Send(data);
                SystemLog.Append(data + '\n');
                if (SystemSettings.LogIntoConsole || SystemSettings.VerboseMode) Console.WriteLine(data);
            }
        }
        public static int Execute(string path, string cmd)
        {
            Process process = new();
            int result = int.MinValue;

            process.IsRunning = true;
            process.PID = AllocPID();

            AllProcesses.Add(process);

            try
            {
                if (!FileSystemManager.FileExists(path)) return result;

                byte[] bytes = FileSystemManager.ReadBytesFile(path);
                EEExecutable executable = ExecutionEngine.Load(bytes);

                var funcres = executable.Execute(cmd);

                if (funcres.IsExceptionUnwinding)
                {
                    // todo: process crash handling
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
                // todo: process crash handling
            }
            return result;
        }
        public static unsafe void Initialize()
        {
            if (!initialized)
            {
                Log("setting up system folders...", "platformmanager");
                for (int i = 0; i < FileSystemManager.SystemFolders.Length; i++)
                    FileSystemManager.CreateDirectory("0:\\" + FileSystemManager.SystemFolders[i]);

                KernelProcess = new Process
                {
                    BinaryPath = "<kernel>",
                    ConsoleArguments = "",
                    IsRunning = true
                };
                KernelProcess.Executor = KernelProcess;

                Log("set up basic platform!", "platformmanager");
                initialized = true;
            }
        }
        public static void Panic(string msg)
        {
            SessionAge = 3;

            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.BackgroundColor = ConsoleColor.Black;

            string text0 = "!!! panic !!! message: " + msg;
            string text1 = "current kernel version: " + GetPlatformVersion();

            Log(text0, "failuremanager", LogLevel.Fatal);
            Log(text1, "failuremanager", LogLevel.Fatal);

            Console.WriteLine(text0);
            Console.WriteLine(text1);

            Bootstrapper.WaitForShutdown(true, SystemSettings.CrashPowerOffTimeout, true);
            while (true) { }
        }
        public static string GetTimeAsString() =>
            (RTC.Hour < 10 ? "0" + RTC.Hour : RTC.Hour) + ":" +
            (RTC.Minute < 10 ? "0" + RTC.Minute : RTC.Minute) + ":" +
            (RTC.Second < 10 ? "0" + RTC.Second : RTC.Second);
        public static string GetDateAsString() =>
            (RTC.DayOfTheMonth < 10 ? "0" + RTC.DayOfTheMonth : RTC.DayOfTheMonth) + "." +
            (RTC.Month < 10 ? "0" + RTC.Month : RTC.Month) + "." + RTC.Year;
        public static void AskForLogin()
        {
            var list = UserManager.FindNonSystemUsers();

            if (list.Length == 0)
            {
                Console.WriteLine("no users available");
                Bootstrapper.WaitForShutdown(true, 5, false);
            }
            else if (list.Length == 1)
            {
                var target = list[0];
                Console.WriteLine($"{target.UserName} is only loggable user in system, logging in...");

                if (target.UserLockType == 0)
                {
                    if (UserManager.Login(target.Username, "", true) != UserManager.UserLoginResultLoggedInto)
                    {
                        Console.WriteLine("user database is corrupted");
                        Bootstrapper.WaitForShutdown(true, 5, false);
                    }
                }
                else
                {
                retry:
                    Console.WriteLine("login: " + target.UserName);
                    Console.Write("password: ");
                    string password = Console.ReadLine();
                    if (UserManager.Login(target.UserName, password) != UserManager.UserLoginResultLoggedInto)
                    {
                        Console.WriteLine("invalid credentials\n");
                        goto retry;
                    }
                }
                Console.WriteLine("logged in as " + UserManager.CurrentUser.Username);
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
                Console.WriteLine("logged in as " + UserManager.CurrentUser.Username);
            }
        }
        public static void ShutdownSystem(ShutdownType type, bool halt = false, string panic = null)
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
                    Panic(panic ?? "<null string>");
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
