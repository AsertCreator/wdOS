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

namespace wdOS.Platform.Foundation
{
    public static class PlatformManager
    {
        public static StringBuilder SystemLog = new();
        public static SystemState CurrentSystemState;
        public static Process KernelProcess;
        public static List<KeyboardBase> AttachedKeyboards;
        public static List<MouseBase> AttachedMice;
        public static int NextPID = 0;
        public static bool LoadUsersFromDisk = false;
        private static bool initialized = false;
        public static string GetPlatformVersion() =>
            BuildConstants.VersionMajor + "." +
            BuildConstants.VersionMinor + "." +
            BuildConstants.VersionPatch;
        public static unsafe void Initialize()
        {
            if (!initialized)
            {
                PlatformLogger.Log("Setting up system folders...");
                for (int i = 0; i < FileSystemManager.SystemFolders.Length; i++)
                    FileSystemManager.CreateDirectory("0:\\" + FileSystemManager.SystemFolders[i]);

                KernelProcess = new Process();
                KernelProcess.BinaryPath = "<kernel>";
                KernelProcess.NetworkPath = "";
                KernelProcess.DataSectionAddress = (byte*)0;
                KernelProcess.CodeSectionAddress = (byte*)0;
                KernelProcess.ConsoleArguments = "";
                KernelProcess.IsRunning = true;
                KernelProcess.ParentProcess = KernelProcess;

                PlatformLogger.Log("Set up basic platform!");
                initialized = true;
            }
        }
        public static string[] GatherConsoleArguments()
        {
            return new string[] { }; // todo: this also
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
            public const int VersionMinor = 7;
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
            if (UserManager.AvailableUsers.Count == 1)
            {
                var user = UserManager.AvailableUsers[0];
                if (user.UserLockType != 0)
                {
                retry:
                    Console.Write($"login: {user.UserName}");
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
            Console.WriteLine($"logged in as {UserManager.CurrentUser.UserName}");
        }
        public static void Shutdown(int panic = -1, bool restart = false)
        {
            try
            {
                if (panic != -1) FailureManager.Panic((uint)panic);

                CurrentSystemState = SystemState.ShuttingDown;
                if (!restart)  Console.WriteLine("Shutting down...");
                else Console.WriteLine("Restarting...");

                KernelProcess.Destroy(true);

                Heap.Collect();

                if (!restart) Power.CPUReboot();
                else Power.ACPIShutdown();


            }
            catch
            {
                CurrentSystemState = SystemState.AfterLife;
                FailureManager.Panic(6);
            }
        }
        public enum SystemState
        {
            BeforeLife, Starting, Running, ShuttingDown, AfterLife
        }
    }
    public unsafe class Process
    {
        public const int SubsystemEmbedded = 0;
        public const int SubsystemNative = 1;
        public const int SubsystemScript = 2;
        public int PID = ++PlatformManager.NextPID;
        public string BinaryPath;
        public string NetworkPath;
        public string ConsoleArguments;
        public string CurrentDirectory;
        public object AdditionalObject;
        public int SubsystemType = SubsystemEmbedded;
        public byte* CodeSectionAddress;
        public byte* DataSectionAddress;
        public bool IsRunning = false;
        public List<Process> ChildProcesses = new();
        public Process ParentProcess;
        public void Destroy(bool killtree)
        {
            for (int i = 0; i < ChildProcesses.Count; i++)
            {
                var process = ChildProcesses[i];
                if (killtree) process.Destroy(true);
                else process.ParentProcess = ParentProcess;
            }
            if (BinaryPath != "<kernel>") IsRunning = false;
        }
    }
}
