using Cosmos.Core;
using Cosmos.System.FileSystem.VFS;
using System;
using System.Collections.Generic;
using System.IO;

namespace wdOS.Core.Shells.TShell
{
    internal class ChangePathCommand : ConsoleCommand
    {
        internal override string Name => "cd";
        internal override string Description => "changes directory";
        internal override int Execute(string[] args)
        {
            var path = Utilities.CanonicalPath(true, TShell.GetFullPath(), Utilities.ConnectArgs(args));
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
        internal override string Name => "chgvol";
        internal override string Description => "changes current volume";
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
        internal override string Name => "mkdir";
        internal override string Description => "creates a directory in cd";
        internal override int Execute(string[] args) { FileSystem.CreateDirectory(Path.Combine(TShell.GetFullPath(), Utilities.ConnectArgs(args))); return 0; }
    }
    internal class TouchCommand : ConsoleCommand
    {
        internal override string Name => "touch";
        internal override string Description => "creates a file in cd"/*and changes access time"*/;
        internal override int Execute(string[] args) { FileSystem.WriteStringFile(Path.Combine(TShell.GetFullPath(), Utilities.ConnectArgs(args)), ""); return 0; }
    }
    internal class FunCommand : ConsoleCommand
    {
        internal static string Alphabet = "qwertyuiopasdfghjklzxcvbnm";
        internal override string Name
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
        internal override string Description => "i bet you can't execute this cmd";
        internal override int Execute(string[] args) { Console.WriteLine("WHAT?? You did execute this program? It may be very hard"); return 0; }
    }
    internal class HelpCommand : ConsoleCommand
    {
        internal override string Name => "help";
        internal override string Description => "shows this help list";
        internal override int Execute(string[] args) { ShowHelpMenu(TShell.AllCommands); return 0; }
    }
    internal class EchoCommand : ConsoleCommand
    {
        internal override string Name => "echo";
        internal override string Description => "echoes any arguments";
        internal override int Execute(string[] args) { Console.WriteLine(Utilities.ConnectArgs(args)); return 0; }
    }
    internal class ClearCommand : ConsoleCommand
    {
        internal override string Name => "clear";
        internal override string Description => "clears console";
        internal override int Execute(string[] args) { Console.Clear(); return 0; }
    }
    internal class ListPartCommand : ConsoleCommand
    {
        internal override string Name => "lspart";
        internal override string Description => "lists all partitions";
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
        internal override string Name => "ls";
        internal override string Description => "lists all etries in directory";
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
        internal override string Name => "restart";
        internal override string Description => "restarts this computer";
        internal override int Execute(string[] args) { Kernel.ShutdownPC(true); return 0; }
    }
    internal class ShutdownCommand : ConsoleCommand
    {
        internal override string Name => "shutdown";
        internal override string Description => "shutdowns this computer";
        internal override int Execute(string[] args) { Kernel.ShutdownPC(false); return 0; }
    }
    internal class WelcomeCommand : ConsoleCommand
    {
        internal override string Name => "welcome";
        internal override string Description => "shows logon welcome text";
        internal override int Execute(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"Running wdOS TShell, version: {Kernel.KernelVersion}");
            Console.WriteLine("Start by typing \"help\" command");
            return 0;
        }
    }
    internal class LogCatCommand : ConsoleCommand
    {
        internal override string Name => "logcat";
        internal override string Description => "shows entire system log";
        internal override int Execute(string[] args)
        {
            Console.WriteLine("LogCat - definitely not bug free program, hold on...");
            Console.WriteLine(Kernel.SystemLog.ToString());
            Console.WriteLine("Have a great day!");
            return 0;
        }
    }
}
