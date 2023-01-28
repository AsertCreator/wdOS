using Cosmos.Core;
using Cosmos.HAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wdOS.Core.Foundation
{
    internal static class SystemInteraction
    {
        internal const byte BoxRightUp = 0xB7;
        internal const byte BoxLeftUp = 0xDA;
        internal const byte BoxRightDown = 0xD9;
        internal const byte BoxLeftDown = 0xC0;
        internal const byte BoxVerticalLine = 0xB3;
        internal const byte BoxHorizontalLine = 0xC4;
        internal static void Shutdown(int panic = -1, bool restart = false)
        {
            try
            {
                ShowMessageBox("Shutting down...");
                Kernel.EnableServices(false);
                Kernel.SSDShells.Clear();
                Kernel.SSDService.Clear();
                if (panic != -1) ErrorHandler.Panic((uint)panic);
                if (Kernel.CurrentShell != null) Kernel.CurrentShell.IsRunning = false;
                foreach (var app in Runtime.CurrentApplications)
                {
                    app.IsRunning = false;
                    app.IsRunningElevated = false;
                }
                Runtime.CurrentApplications.Clear();
                if (!Kernel.SystemSettings.CDROMBoot) SaveSystemSettingsAsFile();
                Kernel.SweepTrash();
                if (!restart) Power.ACPIReboot();
                else Power.ACPIShutdown();
            }
            catch
            {
                ErrorHandler.Panic(6);
            }
        }
        internal static void SaveSystemSettingsAsFile()
        {

        }
        internal static void ShowMessageBox(string text)
        {
            var console = Cosmos.System.Global.Console.mText;
            var height = 5;
            var width = text.Length + 4;
            console[1, 1] = BoxLeftUp;
            console[1, 1 + height] = BoxLeftDown;
            console[1 + width, 1] = BoxRightUp;
            console[1 + width, 1 + height] = BoxRightDown;
            for (int x = 2; x < width - 1; x++)
            {
                console[x, 1] = BoxHorizontalLine;
                console[x, 1 + height] = BoxHorizontalLine;
            }
            for (int y = 2; y < height - 1; y++)
            {
                console[1, y] = BoxVerticalLine;
                console[1 + width, y + height] = BoxVerticalLine;
            }
        }
        internal static string SaveSystemSettings()
        {
            StringBuilder sb = new();
            sb.AppendLine($"; wdOS System Settings, do not touch this!");
            sb.AppendLine($"system.computername::{Kernel.ComputerName}");
            sb.AppendLine($"system.enablefilesystem::{(Kernel.SystemSettings.EnableFileSystem ? 1 : 0)}");
            sb.AppendLine($"system.enableperiodicgc::{(Kernel.SystemSettings.EnablePeriodicGC ? 1 : 0)}");
            sb.AppendLine($"system.enablelogging::{(Kernel.SystemSettings.EnableLogging ? 1 : 0)}");
            sb.AppendLine($"system.enableaudio::{(Kernel.SystemSettings.EnableAudio ? 1 : 0)}");
            sb.AppendLine($"system.terminalfont::{Kernel.SystemSettings.SystemTerminalFont}");
            sb.AppendLine($"system.cdromboot::{Kernel.SystemSettings.CDROMBoot}");
            sb.AppendLine($"system.version::{Kernel.KernelVersion}");
            return sb.ToString();
        }
        internal static void LoadSystemSettings()
        {

        }
    }
}
