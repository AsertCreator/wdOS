using Cosmos.Core;
using Cosmos.HAL;
using Cosmos.System.Graphics;
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
                        Console.WriteLine($"CPU Vendor     = \"{CPU.GetCPUVendorName()}\"");
                        Console.WriteLine($"CPU Model Name = \"{CPU.GetCPUBrandString()}\"");
                        Console.WriteLine($"System uptime  = {CPU.GetCPUUptime}");
                        Console.WriteLine($"System vendor  = {"not implemented"}");
                        Console.WriteLine($"wdOS Version   = {Kernel.KernelVersion}");
                        Console.WriteLine($"64-bit CPU     = {Kernel.Is64BitCPU}");
                        break;
                    case "lspci":
                        List<IHelpEntry> entries = new();
                        int index = 0;
                        foreach (var device in Cosmos.HAL.PCI.Devices)
                        {
                            entries.Add(new GeneralHelpEntry($"Device #{index}", $"DeviceID: {device.DeviceID}, VendorID: {device.VendorID}, Slot: {device.slot}"));
                            index++;
                        }
                        IHelpEntry.ShowHelpMenu(entries);
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
                        var font = Kernel.SystemSettings.TerminalFonts[Kernel.SystemSettings.SystemTerminalFont];
                        var bytes = font.CreateVGAFont();
                        var length = bytes.Length;
                        var rng = new Random();
                        for (int i = 0; i < length; i++)
                        {
                            bytes[i] = (byte)(bytes[i] + rng.Next(-4, 4));
                        }
                        VGAScreen.SetFont(bytes, font.Height);
                        break;
                    case "resetfont":
                        if (args.Length < 2)
                        {
                            Console.WriteLine("You need to specify font number!");
                            break;
                        }
                        int number = int.Parse(args[1]);
                        if (number < 0 || number >= Kernel.SystemSettings.TerminalFonts.Count)
                        {
                            Console.WriteLine("You need to specify a valid font number!");
                            break;
                        }
                        var font2 = Kernel.SystemSettings.TerminalFonts[number];
                        VGAScreen.SetFont(font2.CreateVGAFont(), font2.Height);
                        Kernel.SystemSettings.SystemTerminalFont = number;
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
