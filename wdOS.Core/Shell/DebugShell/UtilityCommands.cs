using Cosmos.Core;
using Cosmos.System.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using wdOS.Core.Foundation;

namespace wdOS.Core.Shell.DebugShell
{
    public static class UtilityCommands
    {
        public static void AddCommands()
        {
            TShellManager.AllCommands.Add(new ClearCommand());
            TShellManager.AllCommands.Add(new CShellCommand());
            TShellManager.AllCommands.Add(new EchoCommand());
            TShellManager.AllCommands.Add(new HelpCommand());
            TShellManager.AllCommands.Add(new LogCatCommand());
            TShellManager.AllCommands.Add(new RepeatCommand());
            TShellManager.AllCommands.Add(new RestartCommand());
            TShellManager.AllCommands.Add(new ShutdownCommand());
            TShellManager.AllCommands.Add(new SUtilsCommand());
            TShellManager.AllCommands.Add(new WelcomeCommand());
        }
        public class SUtilsCommand : ConsoleCommand
        {
            public override string Name => "sutils";
            public override string Description => "manages system information";
            public override int Execute(string[] args)
            {
                if (args.Length > 0)
                {
                    switch (args[0])
                    {
                        case "sysinfo":
                            Console.WriteLine($"cpu vendor     - \"{CPU.GetCPUVendorName()}\"");
                            Console.WriteLine($"cpu model name - \"{CPU.GetCPUBrandString()}\"");
                            Console.WriteLine($"system uptime  - {CPU.GetCPUUptime}");
                            Console.WriteLine($"system vendor  - {"not implemented"}");
                            Console.WriteLine($"wdOS version   - {Kernel.GetKernelVersion()}");
                            Console.WriteLine($"cpu bit width  - {(Kernel.Is64BitCPU ? 64 : 32)}");
                            break;
                        case "lspci":
                            List<IHelpEntry> entries = new();
                            int index = 0;
                            foreach (var device in Cosmos.HAL.PCI.Devices)
                            {
                                entries.Add(new GeneralHelpEntry($"device #{index}", $"deviceID: {device.DeviceID}, vendorID: {device.VendorID}, slot: {device.slot}"));
                                index++;
                            }
                            IHelpEntry.ShowHelpMenu(entries);
                            break;
                        case "memory":
                            if (args.Contains("sweep"))
                            {
                                Console.WriteLine($"sweeped {Kernel.SweepTrash()} unused objects");
                                break;
                            }
                            uint total = Kernel.GetTotalRAM();
                            uint used = Kernel.GetUsedRAM();
                            double usedpercent = Math.Round((used + 0.0) / (total + 0.0) * 100);
                            Console.WriteLine($"total memory: {total} bytes");
                            Console.WriteLine($"used memory : {used} bytes ({usedpercent}%)");
                            Console.WriteLine($"free memory : {total - used} bytes ({100.0 - usedpercent}%)");
                            break;
                        case "services":
                            if (args.Contains("enable"))
                            {
                                Kernel.EnableServices(true);
                                break;
                            }
                            else if (args.Contains("disable"))
                            {
                                Kernel.EnableServices(false);
                                break;
                            }
                            else
                            {
                                Console.WriteLine("currently running services:");
                                List<IHelpEntry> entries1 = new();
                                foreach (var service in SystemDatabase.SSDService)
                                    entries1.Add(new GeneralHelpEntry(service.ShellName, service.ShellDesc));
                                IHelpEntry.ShowHelpMenu(entries1);
                            }
                            break;
                        case "resetfont":
                            if (args.Length < 2)
                            {
                                Console.WriteLine("sutils: font number is not specified");
                                break;
                            }
                            int number = int.Parse(args[1]);
                            if (number < 0 || number >= SystemDatabase.SystemSettings.TerminalFonts.Count)
                            {
                                Console.WriteLine("sutils: specified font number is not valid");
                                break;
                            }
                            var font2 = SystemDatabase.SystemSettings.TerminalFonts[number];
                            VGAScreen.SetFont(font2.CreateVGAFont(), font2.Height);
                            SystemDatabase.SystemSettings.SystemTerminalFont = number;
                            break;
                        case "help":
                            List<IHelpEntry> entries2 = new()
                            {
                                new GeneralHelpEntry("sysinfo", "shows your system info"),
                                new GeneralHelpEntry("lspci", "shows your PCI devices"),
                                new GeneralHelpEntry("services", "enables or disables services"),
                                new GeneralHelpEntry("resetafont", "changes currently set vga font"),
                                new GeneralHelpEntry("memory", "controls your system memory"),
                                new GeneralHelpEntry("help", "shows your system utilities")
                            };
                            IHelpEntry.ShowHelpMenu(entries2);
                            break;
                        default:
                            Console.WriteLine("sutils: specified action is not supported");
                            return 1;
                    }
                }
                return 0;
            }
        }
        public class HelpCommand : ConsoleCommand
        {
            public override string Name => "help";
            public override string Description => "shows this help list";
            public override int Execute(string[] args)
            {
                if (args.Length < 1)
                {
                    List<IHelpEntry> entries = new();
                    foreach (var cmd in TShellManager.AllCommands) entries.Add(cmd);
                    IHelpEntry.ShowHelpMenu(entries);
                }
                else if (args.Length > 1) 
                { 
                    Console.WriteLine("help: too much arguments"); 
                    return 1; 
                }
                else
                {
                    var cmd = TShellManager.FindCommandByName(args[0]);
                    Console.WriteLine($"{cmd.Name} - {cmd.Description}");
                }
                return 0;
            }
        }
        public class EchoCommand : ConsoleCommand
        {
            public override string Name => "echo";
            public override string Description => "echoes any arguments";
            public override int Execute(string[] args)
            {
                string print = Utilities.ConcatArray(args);
                if (string.IsNullOrEmpty(print.Trim()))
                {
                    Console.WriteLine($"current prompt state: {TShellManager.ShowPrompt}");
                    return 0;
                }
                else if (args[0] == "--disable-prompt")
                {
                    TShellManager.ShowPrompt = false;
                    return 0;
                }
                else if (args[0] == "--enable-prompt")
                {
                    TShellManager.ShowPrompt = true;
                    return 0;
                }
                Console.WriteLine(print);
                return 0;
            }
        }
        public class ClearCommand : ConsoleCommand
        {
            public override string Name => "clear";
            public override string Description => "clears current console";
            public override int Execute(string[] args)
            {
                if (args.Length > 0) { Console.WriteLine("clear: too much arguments"); return 1; }

                Console.Clear(); 
                return 0; 
            }
        }
        public class RestartCommand : ConsoleCommand
        {
            public override string Name => "restart";
            public override string Description => "restarts local computer";
            public override int Execute(string[] args)
            {
                if (args.Length > 0) { Console.WriteLine("restart: too much arguments"); return 1; }

                Kernel.ShutdownPC(true);
                return 0;
            }
        }
        public class ShutdownCommand : ConsoleCommand
        {
            public override string Name => "shutdown";
            public override string Description => "shutdowns local computer";
            public override int Execute(string[] args)
            {
                if (args.Length > 0) { Console.WriteLine("shutdown: too much arguments"); return 1; }

                Kernel.ShutdownPC(false); 
                return 0;
            }
        }
        public class WelcomeCommand : ConsoleCommand
        {
            public override string Name => "welcome";
            public override string Description => "shows logon welcome text";
            public override int Execute(string[] args)
            {
                if (args.Length > 0) { Console.WriteLine("welcome: too much arguments"); return 1; }

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"Running wdOS TShell, version: {Kernel.GetKernelVersion()}");
                Console.WriteLine($"Current user: {SystemDatabase.CurrentUser.UserName}. Start by typing \"help\" command");
                return 0;
            }
        }
        public class LogCatCommand : ConsoleCommand
        {
            public override string Name => "logcat";
            public override string Description => "shows entire system log";
            public override int Execute(string[] args)
            {
                if (args.Length > 0) { Console.WriteLine("logcat: too much arguments"); return 1; }
                Console.WriteLine(SystemDatabase.SystemLog.ToString());
                return 0;
            }
        }
        public class CShellCommand : ConsoleCommand
        {
            public override string Name => "cshell";
            public override string Description => "opens CShell GUI shell";
            public override int Execute(string[] args)
            {
                if (args.Length > 0) { Console.WriteLine("cshell: too much arguments"); return 1; }
                if (SystemInteraction.Mice.Count > 0)
                {
                    CShell.CShellManager shell = new();
                    shell.ShellBeforeRun();
                    while (shell.IsRunning) shell.ShellRun();
                    return 0;
                }
                Console.WriteLine("cshell: mouse not detected");
                return 1;
            }
        }
        public class RepeatCommand : ConsoleCommand
        {
            public override string Name => "repeat";
            public override string Description => "repeats certain command multiple times";
            public override int Execute(string[] args)
            {
                if (args.Length < 2) { Console.WriteLine("repeat: too few arguments"); return 1; }
                if (args.Length > 2) { Console.WriteLine("repeat: too much arguments"); return 1; }

                int repeats = int.Parse(args[0]);
                var cmd = Utilities.ConcatArray(args.Skip(1).ToArray());

                for (int i = 0; i < repeats; i++)
                {
                    if (!TShellManager.ExecuteCmd(cmd))
                    {
                        Console.WriteLine($"repeat: command '{args[1]}' doesn't exist!"); return 1;
                    }
                }
                return 0;
            }
        }
    }
}
