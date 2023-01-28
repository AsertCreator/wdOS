using System;

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
                 ShowScreenMessage("Shutting down...");

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

                ShowScreenMessage("Shutting down app...", true);

                Environment.Exit(1);
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
            Console.WriteLine("Couldn't save system settings, because system saving is not supported on Windows");
        }
    }
    internal enum SystemState
    {
        BeforeLife, Starting, Running, ShuttingDown, AfterLife
    }
}
