using System;
using System.Collections.Generic;
using System.Linq;
using wdOS.Core.Foundation;

namespace wdOS.Core.Shell.TShell
{
    internal class SUtilsCommand : ConsoleCommand
    {
        internal override string Name => "sutils";
        internal override string Description => "tests and gathers info about system";
        internal override int Execute(string[] args)
        {
            Console.WriteLine("SUtils - definitely not bug free program");
            if (args.Length > 0)
            {
                switch (args[0])
                {
                    case "sysinfo":
                        Console.WriteLine($"CPU Vendor     = \"unimplemented\"");
                        Console.WriteLine($"CPU Model Name = \"unimplemented\"");
                        Console.WriteLine($"System uptime  = {Environment.TickCount64}");
                        Console.WriteLine($"System vendor  = {"not implemented"}");
                        Console.WriteLine($"wdOS Version   = {Kernel.KernelVersion}");
                        Console.WriteLine($"64-bit CPU     = {Kernel.Is64BitCPU}");
                        break;
                    case "lspci":
                        Console.WriteLine("You cant use lspci on Windows");
                        break;
                    case "memory":
                        if (args.Contains("sweep"))
                        {
                            Console.WriteLine("Sweeping...");
                            Console.WriteLine($"Sweeped {Kernel.SweepTrash()} unused objects!");
                            break;
                        }
                        uint total = Kernel.TotalRAM;
                        uint used = Kernel.UsedRAM;
                        double usedpercent = Math.Round((used + 0.0) / (total + 0.0) * 100);
                        Console.WriteLine($"Total memory: {total} bytes");
                        Console.WriteLine($"Used memory : {used} bytes ({usedpercent}%");
                        Console.WriteLine($"Free memory : {total - used} bytes ({100.0 - usedpercent}%");
                        break;
                    case "services":
                        if (args.Contains("enable"))
                        {
                            Console.WriteLine("Enabling services...");
                            Kernel.EnableServices(true);
                            break;
                        }
                        else if (args.Contains("disable"))
                        {
                            Console.WriteLine("Disabling services...");
                            Kernel.EnableServices(false);
                            break;
                        }
                        else
                        {
                            Console.WriteLine("List of currently running services:");
                            List<IHelpEntry> entries1 = new();
                            foreach (var service in Kernel.SSDService)
                                entries1.Add(new GeneralHelpEntry(service.Name, service.Desc));
                            IHelpEntry.ShowHelpMenu(entries1);
                        }
                        break;
                    case "crash":
                        Console.WriteLine("Say goodbye to your current sseion :)");
                        ErrorHandler.Panic(6);
                        break;
                    case "scramblevga":
                        Console.WriteLine("You cant use scramblevga on Windows");
                        break;
                    case "resetfont":
                        Console.WriteLine("You cant use resetfont on Windows");
                        break;
                    case "help":
                        List<IHelpEntry> entries2 = new()
                        {
                            new GeneralHelpEntry("sysinfo", "shows your system info"),
                            new GeneralHelpEntry("lspci", "shows your PCI devices"),
                            new GeneralHelpEntry("services", "enables or disables services"),
                            new GeneralHelpEntry("crash", "crashes your system"),
                            new GeneralHelpEntry("scramblevga", "scrambles your current vga font"),
                            new GeneralHelpEntry("resetfont", "changes currently set vga font"),
                            new GeneralHelpEntry("memory", "controls your system memory"),
                            new GeneralHelpEntry("help", "shows your system utilities")
                        };
                        IHelpEntry.ShowHelpMenu(entries2);
                        break;
                    default:
                        Console.WriteLine("This action is not supported!");
                        return 1;
                }
            }
            return 0;
        }
    }
}
