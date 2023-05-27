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

namespace wdOS.Setup.Platform
{
    internal static class PlatformManager
    {
        internal static List<KeyboardBase> AttachedKeyboards;
        internal static List<MouseBase> AttachedMice;
        internal static List<KernelModule> LoadedModules = new();
        internal static int SessionAge;
        internal static bool LoadUsersFromDisk = false;
        internal static int GlobalGarbage = 0;
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
            internal const int VersionMinor = 9;
            internal const int VersionPatch = 0;
            internal const int CurrentType = TypePreBeta;
            internal const int TypePreAlpha = 0;
            internal const int TypeAlpha = 1;
            internal const int TypePreBeta = 2;
            internal const int TypeBeta = 3;
            internal const int TypePreRelease = 4;
            internal const int TypeRelease = 5;
        }
        internal static void Shutdown(ShutdownType type, bool halt = false, uint panic = 0)
        {
            Console.WriteLine('\n'); // double new line
            SessionAge = 2;

            switch (type)
            {
                case ShutdownType.SoftShutdown:
                    Console.WriteLine("shutting down...");

                    Heap.Collect();

                    if (halt)
                        while (true) CPU.Halt();

                    Power.ACPIShutdown();

                    Console.WriteLine("shutdown failed! cpu halted");
                    goto case ShutdownType.Halt;
                case ShutdownType.SoftRestart:
                    Console.WriteLine("restarting...");

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
                    FailureManager.Panic(panic);
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
}
