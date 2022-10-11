using Cosmos.Core;
using System;
using System.Collections.Generic;

namespace wdOS.Core.Shells.TShell
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
                    case "cpuid":
                        Console.WriteLine($"CPUID: {CPU.GetCPUVendorName()}");
                        break;
                    case "cpuname":
                        Console.WriteLine($"CPU Name: {CPU.GetCPUBrandString()}");
                        break;
                    case "cpufreq":
                        Console.WriteLine($"CPU Frequncy: {CPU.GetCPUCycleSpeed()}");
                        break;
                    case "uptime":
                        Console.WriteLine($"System uptime: {CPU.GetCPUUptime}");
                        break;
                    case "lspci":
                        List<HelpEntry> entries = new();
                        int index = 0;
                        foreach (var device in Cosmos.HAL.PCI.Devices)
                        {
                            entries.Add(new GeneralHelpEntry($"Device #{index}", $"DeviceID: {device.DeviceID}, VendorID: {device.VendorID}, Slot: {device.slot}"));
                            index++;
                        }
                        ShowHelpMenu(entries);
                        break;
                    case "sweep":
                        Console.WriteLine("Sweeping...");
                        Console.WriteLine($"Sweeped {Kernel.SweepTrash()} unused objects!");
                        break;
                    case "memory":
                        uint total = Kernel.TotalRAM;
                        uint used = Kernel.UsedRAM;
                        Console.WriteLine($"Total memory: {total} bytes");
                        Console.WriteLine($"Used memory : {used} bytes ({Math.Round((used + 0.0) / (total + 0.0) * 100)}%");
                        break;
                    case "showmenu":
                        break;
                    case "help":
                        List<HelpEntry> entries1 = new()
                        {
                            new GeneralHelpEntry("cpuid", "shows your cpuid"),
                            new GeneralHelpEntry("cpuname", "shows your cpu name"),
                            new GeneralHelpEntry("cpufreq", "shows your cpu frequency"),
                            new GeneralHelpEntry("uptime", "shows your system uptime"),
                            new GeneralHelpEntry("lspci", "shows your PCI devices"),
                            new GeneralHelpEntry("showmenu", "shows your system menu"),
                            new GeneralHelpEntry("sweep", "sweeps all unused memory"),
                            new GeneralHelpEntry("memory", "shows your system memory"),
                            new GeneralHelpEntry("help", "shows your system utilities")
                        };
                        ShowHelpMenu(entries1);
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
