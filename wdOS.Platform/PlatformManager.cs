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
    internal static class PlatformManager
    {
        internal static Process KernelProcess;
        internal static Process CurrentProcess;
        internal static List<Process> AllProcesses;
        internal static List<KeyboardBase> AttachedKeyboards;
        internal static List<MouseBase> AttachedMice;
        internal static List<KernelModule> LoadedModules = new();
        internal static int SessionAge;
        internal static bool LoadUsersFromDisk = false;
        private static int nextpid = 0;
        private static bool initialized = false;
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
                PlatformLogger.Log("setting up system folders...", "platformmanager");
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

                PlatformLogger.Log("set up basic platform!", "platformmanager");
                initialized = true;
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
            internal const int VersionMinor = 8;
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
        internal static void Shutdown(int panic = -1, bool restart = false)
        {
            try
            {
                if (panic != -1) FailureManager.Panic((uint)panic);

                Console.WriteLine('\n'); // double new line

                SessionAge = 2;
                if (!restart) Console.WriteLine("shutting down...");
                else Console.WriteLine("restarting...");

                Heap.Collect();

                if (!restart) Power.CPUReboot();
                else Power.ACPIShutdown();

                Console.WriteLine("shutdown failed! cpu halted");

                CPU.Halt();
            }
            catch
            {
                FailureManager.Panic(6);
            }
        }
    }
}
