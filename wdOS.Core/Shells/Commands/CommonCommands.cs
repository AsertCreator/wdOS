using Cosmos.Core;
using Cosmos.System.FileSystem.VFS;
using System;
using System.Collections.Generic;
using System.IO;

namespace wdOS.Core.Shells.Commands
{
    internal class ChangePathCommand : ConsoleCommand
    {
        public override string Name => "cd";
        public override string Description => "changes directory";
        internal override int Execute(string[] args)
        {
            var path = Kernel.CanonicalPath(true, TShell.GetFullPath(), Utilities.ConnectArgs(args));
            Console.WriteLine(path);
            if (Utilities.ConnectArgs(args).Contains(':'))
            {
                //TShell.Path = 
                Console.WriteLine(path.Substring(3));
                //TShell.Volume = 
                Console.WriteLine(Convert.ToByte(args[0][0]));
                return 0;
            }
            if (!VFSManager.FileExists(path) && VFSManager.DirectoryExists(path))
                //TShell.Path =
                Console.WriteLine(path);
            return 0;
        }
    }
    internal class ChangeVolumeCommand : ConsoleCommand
    {
        public override string Name => "chgvol";
        public override string Description => "changes current volume";
        internal override int Execute(string[] args)
        {
            if (args.Length == 1)
            { TShell.Volume = Convert.ToByte(args[0][0]); return 0; }
            else
            { Console.WriteLine("Too many or too few arguments"); return 1; }
        }
    }
    internal class MkdirCommand : ConsoleCommand
    {
        public override string Name => "mkdir";
        public override string Description => "creates a directory in cd";
        internal override int Execute(string[] args)  { FileSystemManager.CreateDirectory(Path.Combine(TShell.GetFullPath(), Utilities.ConnectArgs(args))); return 0; }
    }
    internal class TouchCommand : ConsoleCommand
    {
        public override string Name => "touch";
        public override string Description => "creates a file in cd"/*and changes access time"*/;
        internal override int Execute(string[] args) { FileSystemManager.WriteStringFile(Path.Combine(TShell.GetFullPath(), Utilities.ConnectArgs(args)), ""); return 0; }
    }
    internal class FunCommand : ConsoleCommand
    {
        internal static string Alphabet = "qwertyuiopasdfghjklzxcvbnm";
        public override string Name
        {
            get
            {
                string name = "";
                var random = new Random();
                for (int i = 0; i < 5; i++)
                { name += Alphabet[random.Next(Alphabet.Length)]; }
                return name;
            }
        }
        public override string Description => "i bet you can't execute this cmd";
        internal override int Execute(string[] args) { Console.WriteLine("WHAT?? You did execute this program? It may be very hard"); return 0; }
    }
    internal class HelpCommand : ConsoleCommand
    {
        public override string Name => "help";
        public override string Description => "shows this help list";
        internal override int Execute(string[] args) { IHelpEntry.ShowHelpMenu(TShell.AllCommands); return 0; }
    }
    internal class EchoCommand : ConsoleCommand
    {
        public override string Name => "echo";
        public override string Description => "echoes any arguments";
        internal override int Execute(string[] args) { Console.WriteLine(Utilities.ConnectArgs(args)); return 0; }
    }
    internal class ClearCommand : ConsoleCommand
    {
        public override string Name => "clear";
        public override string Description => "clears console";
        internal override int Execute(string[] args) { Console.Clear(); return 0; }
    }
    internal class ListPartCommand : ConsoleCommand
    {
        public override string Name => "lspart";
        public override string Description => "lists all partitions";
        internal override int Execute(string[] args)
        {
            int index = 0;
            var disks = VFSManager.GetDisks();
            foreach (var drive in disks)
            {
                Console.WriteLine($"drive #{index} - {drive.Size} bytes");
                foreach (var part in drive.Partitions)
                {
                    Console.WriteLine($"   partition {part.RootPath} - {part.Host.BlockCount * part.Host.BlockSize} bytes");
                }
                index++;
            }
            Console.WriteLine($"Total number of drives: {disks.Count}");
            return 0;
        }
    }
    internal class ListCommand : ConsoleCommand
    {
        public override string Name => "ls";
        public override string Description => "lists all etries in directory";
        internal override int Execute(string[] args)
        {
            var list = VFSManager.GetDirectoryListing(TShell.GetFullPath());
            int maxlength = 0;
            foreach (var entry in list)
            {
                if (entry.mName.Length > maxlength)
                { maxlength = entry.mName.Length; }
            }
            foreach (var entry in list)
            {
                int numberSpaces = maxlength - entry.mName.Length;
                Console.WriteLine(
                    $"{(entry.mEntryType == Cosmos.System.FileSystem.Listing.DirectoryEntryTypeEnum.File ? "     " : "<DIR>")} " +
                    $"{entry.mName}{new string(' ', numberSpaces + 1)}- {entry.mSize}");
            }
            Console.WriteLine($"Total number of entries: {list.Count}");
            return 0;
        }
    }
    internal class RestartCommand : ConsoleCommand
    {
        public override string Name => "restart";
        public override string Description => "restarts this computer";
        internal override int Execute(string[] args) { Kernel.ShutdownPC(true); return 0; }
    }
    internal class ShutdownCommand : ConsoleCommand
    {
        public override string Name => "shutdown";
        public override string Description => "shutdowns this computer";
        internal override int Execute(string[] args) { Kernel.ShutdownPC(false); return 0; }
    }
    internal class WelcomeCommand : ConsoleCommand
    {
        public override string Name => "welcome";
        public override string Description => "shows logon welcome text";
        internal override int Execute(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Running wdOS Console Shell, \"just in case\" thing");
            Console.WriteLine("Start by typing \"help\" command");
            return 0;
        }
    }
    internal class ExecuteSWCommand : ConsoleCommand
    {
        public override string Name => "execsw";
        public override string Description => "executes internal shareware(?)";
        internal override int Execute(string[] args)
        {
            Console.WriteLine("Not implemented yet!");
            return 0;
        }
    }
    internal class SystemUtilsCommand : ConsoleCommand
    {
        public override string Name => "systemutils";
        public override string Description => "tests and gathering info about system";
        internal override int Execute(string[] args)
        {
            Console.WriteLine("SystemUtils - definitely not bug free program");
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
                        List<IHelpEntry> entries = new();
                        int index = 0;
                        foreach (var device in Cosmos.HAL.PCI.Devices)
                        {
                            entries.Add(new GeneralHelpEntry($"Device #{index}", $"DeviceID: {device.DeviceID}, VendorID: {device.VendorID}, Slot: {device.slot}"));
                            index++;
                        }
                        IHelpEntry.ShowHelpMenu(entries);
                        break;
                    case "enablemenu":
                        break;
                    case "help":
                        List<IHelpEntry> entries1 = new()
                        {
                            new GeneralHelpEntry("cpuid", "shows your cpuid"),
                            new GeneralHelpEntry("cpuname", "shows your cpu name"),
                            new GeneralHelpEntry("cpufreq", "shows your cpu frequency"),
                            new GeneralHelpEntry("uptime", "shows your system uptime"),
                            new GeneralHelpEntry("lspci", "shows your PCI devices"),
                            new GeneralHelpEntry("enablemenu", "shows your system menu"),
                            new GeneralHelpEntry("help", "shows your system utilities")
                        };
                        IHelpEntry.ShowHelpMenu(entries1);
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
