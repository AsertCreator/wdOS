using Cosmos.Core;
using Cosmos.HAL;
using Cosmos.System.FileSystem.VFS;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using wdOS.Core.Shell;

namespace wdOS.Core.Foundation
{
    public static class SystemInteraction
    {
        public static SystemState State;
        public static DateTime LastLoginTime;
        public static TimeSpan NoLoginMaxTime;
        public static ShellBase CurrentShell;
        public static List<KeyboardBase> Keyboards;
        public static List<MouseBase> Mice;
        public static string[] SystemFolders = 
            { "PrivateSystem", "PrivateUsers", "Applications", "bin", "lib", "proc", "dev" };
        public static void InitializeInteraction()
        {
            LastLoginTime = DateTime.Now - new TimeSpan(10, 0, 0);
            NoLoginMaxTime = new TimeSpan(0, 10, 0);
        }
        public static void ShowLoginScreen()
        {
            if (SystemDatabase.AvailableUsers.Count == 1)
            {
                var user = SystemDatabase.AvailableUsers[0];
                if (user.UserLockType != 0)
                {
                    retry:
                    Console.Write($"login: {user.UserName}");
                    Console.Write("password: ");
                    string password = Console.ReadLine();
                    if (SystemDatabase.Login(user.UserName, password) != SystemDatabase.UserLoginResultLoggedInto)
                    {
                        Console.WriteLine("invalid credentials\n");
                        goto retry;
                    }
                }
                else
                {
                    if (SystemDatabase.Login(user.UserName, "", true) != SystemDatabase.UserLoginResultLoggedInto)
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
                if (SystemDatabase.Login(username, password) != SystemDatabase.UserLoginResultLoggedInto)
                {
                    Console.WriteLine("invalid credentials\n");
                    goto retry;
                }
            }
            Console.WriteLine($"logged in as {SystemDatabase.CurrentUser.UserName}");
        }
        public static void SetupSystem()
        {
            KernelLogger.Log("Setting up system folders...");
            for (int i = 0; i < SystemFolders.Length; i++)
                FileSystem.CreateDirectory("0:\\" + SystemFolders[i]);
            KernelLogger.Log("Set up system folders!");
        }
        public static void Shutdown(int panic = -1, bool restart = false)
        {
            try
            {
                if (panic != -1) ErrorHandler.Panic((uint)panic);

                State = SystemState.ShuttingDown;
                if (!restart)
                    ShowScreenMessage("Shutting down...");
                else
                    ShowScreenMessage("Restarting...");

                ShowScreenMessage("Disabling services...", true);
                {
                    Kernel.EnableServices(false);
                    SystemDatabase.SSDShells.Clear();
                    SystemDatabase.SSDService.Clear();
                }

                ShowScreenMessage("Disabling apps and shells...", true);
                {
                    if (CurrentShell != null) CurrentShell.IsRunning = false;
                    foreach (var app in Kernel.CurrentApplications)
                    {
                        app.IsRunning = false;
                        app.IsRunningElevated = false;
                    }
                    Kernel.CurrentApplications.Clear();
                }

                ShowScreenMessage("Saving system settings...", true);
                {
                    SaveSystemSettings();
                    Kernel.SweepTrash();
                    State = SystemState.AfterLife;
                }

                if (!restart)
                    ShowScreenMessage("Shutting down CPU...", true);
                else
                    ShowScreenMessage("Restarting CPU...", true);

                if (!restart) Power.ACPIReboot();
                else Power.ACPIShutdown();
            }
            catch
            {
                State = SystemState.AfterLife;
                ErrorHandler.Panic(6);
            }
        }
        public static void ShowScreenMessage(string msg, bool verbose = false)
        {
            if (verbose == true && SystemDatabase.SystemSettings.EnableVerbose == false) return;
            Console.WriteLine(msg);
        }
        public static void SaveSystemSettings()
        {
            try
            {
                // not implemented
            }
            catch
            {
                Console.WriteLine("Couldn't save system settings. Your settings will not be saved");
            }
        }
    }
    public enum SystemState
    {
        BeforeLife, Starting, Running, ShuttingDown, AfterLife
    }
}
