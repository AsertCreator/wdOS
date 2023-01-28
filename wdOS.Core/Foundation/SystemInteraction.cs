using Cosmos.Core;
using Cosmos.HAL;
using Cosmos.System.FileSystem.VFS;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using wdOS.Core.Foundation.Threading;

namespace wdOS.Core.Foundation
{
    internal static class SystemInteraction
    {
        internal static SystemState State;
        internal static void Shutdown(int panic = -1, bool restart = false)
        {
            try
            {
                State = SystemState.ShuttingDown;
                if (!restart)
                    ShowScreenMessage("Shutting down...");
                else
                    ShowScreenMessage("Restarting...");
                Thread.IsInitialized = false;

                ShowScreenMessage("Disabling services...", true);
                {
                    Kernel.EnableServices(false);
                    Kernel.SSDShells.Clear();
                    Kernel.SSDService.Clear();
                }

                ShowScreenMessage("Disabling apps and shells...", true);
                {
                    if (panic != -1) ErrorHandler.Panic((uint)panic);
                    if (Kernel.CurrentShell != null) Kernel.CurrentShell.IsRunning = false;
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
        internal static void ShowScreenMessage(string msg, bool verbose = false)
        {
            if (verbose == true && Kernel.SystemSettings.EnableVerbose == false) return;
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.White;
            Console.Clear();
            Console.CursorLeft = 2;
            Console.CursorTop = 1;
            Console.WriteLine(msg);
        }
        internal static void SaveSystemSettings()
        {
            try
            {
                if (!Kernel.SystemSettings.CDROMBoot)
                {
                    StringBuilder sb = new();
                    sb.AppendLine($"; wdOS System Settings, do not touch this!");
                    sb.AppendLine($"system.computername::{Kernel.ComputerName}");
                    sb.AppendLine($"system.enablefilesystem::{(Kernel.SystemSettings.EnableFileSystem ? 1 : 0)}");
                    sb.AppendLine($"system.enableperiodicgc::{(Kernel.SystemSettings.EnablePeriodicGC ? 1 : 0)}");
                    sb.AppendLine($"system.enablelogging::{(Kernel.SystemSettings.EnableLogging ? 1 : 0)}");
                    sb.AppendLine($"system.enableaudio::{(Kernel.SystemSettings.EnableAudio ? 1 : 0)}");
                    sb.AppendLine($"system.terminalfont::{Kernel.SystemSettings.SystemTerminalFont}");
                    sb.AppendLine($"system.verbosemode::{Kernel.SystemSettings.EnableVerbose}");
                    if (!FileSystem.DirectoryExists(FileSystem.SystemDir)) FileSystem.CreateDirectory(FileSystem.SystemDir);
                    FileSystem.WriteStringFile(FileSystem.SystemSettingsFile, sb.ToString());
                }
            }
            catch
            {
                Console.WriteLine("Couldn't save system settings. Your settings will not be saved");
            }
        }
    }
    internal enum SystemState
    {
        BeforeLife, Starting, Running, ShuttingDown, AfterLife
    }
}
