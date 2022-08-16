using Cosmos.System.FileSystem.VFS;
using System;
using System.IO;
using wdOS.Core.Shells;

namespace wdOS.Core.Commands
{
    internal class ChangePathCommand : ConsoleCommand
    {
        internal override string Name => "cd";
        internal override string ShortDescription => "changes directory";
        internal override int Execute(string[] args)
        {
            var path = PathUtils.CanonicalPath(true, TShell.GetFullPath(), ArrayUtils.ConnectArgs(args));
            if (ArrayUtils.ConnectArgs(args).Contains(':')) 
            { 
                TShell.Path = "";
                TShell.Volume = Convert.ToByte(args[0][0]);
                return 0; 
            }
            if (!VFSManager.FileExists(path) && VFSManager.DirectoryExists(path))
                TShell.Path = path;
            return 0;
        }
    }
    internal class ChangeVolumeCommand : ConsoleCommand
    {
        internal override string Name => "chgvol";
        internal override string ShortDescription => "changes current volume";
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
        internal override string ShortDescription => "creates a directory in cd";
        internal override int Execute(string[] args)
        {
            FSUtils.CreateDirectory(Path.Combine(TShell.GetFullPath(), ArrayUtils.ConnectArgs(args)));
            return 0;
        }
    }
    internal class TouchCommand : ConsoleCommand
    {
        internal override string Name => "touch";
        internal override string ShortDescription => "creates a file in cd"/*and changes access time"*/;
        internal override int Execute(string[] args)
        {
            FSUtils.WriteStringFile(Path.Combine(TShell.GetFullPath(), ArrayUtils.ConnectArgs(args)), "");
            return 0;
        }
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
        internal override string ShortDescription => "i bet you can't execute this cmd";
        internal override int Execute(string[] args)
        {
            Console.WriteLine("WHAT?? You did execute this program? It may be very hard");
            return 0;
        }
    }
    internal class HelpCommand : ConsoleCommand
    {
        internal override string Name => "help";
        internal override string ShortDescription => "shows this help list";
        internal override int Execute(string[] args)
        {
            int maxlength = 0;
            foreach (var cmd in TShell.AllCommands)
            {
                if (cmd.Name.Length > maxlength)
                { maxlength = cmd.Name.Length; }
            }
            foreach (var cmd in TShell.AllCommands)
            {
                int numberSpaces = maxlength - cmd.Name.Length;
                Console.WriteLine($"{cmd.Name}{new string(' ', numberSpaces + 1)}- {cmd.ShortDescription}");
            }
            Console.WriteLine($"Total number of commands: {TShell.AllCommands.Count}");
            return 0;
        }
    }
    internal class EchoCommand : ConsoleCommand
    {
        internal override string Name => "echo";
        internal override string ShortDescription => "echoes any arguments";
        internal override int Execute(string[] args)
        {
            Console.WriteLine(ArrayUtils.ConnectArgs(args));
            return 0;
        }
    }
    internal class ClearCommand : ConsoleCommand
    {
        internal override string Name => "clear";
        internal override string ShortDescription => "clears console";
        internal override int Execute(string[] args)
        {
            Console.Clear();
            return 0;
        }
    }
    internal class ListPartCommand : ConsoleCommand
    {
        internal override string Name => "lspart";
        internal override string ShortDescription => "lists all partitions";
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
        internal override string ShortDescription => "lists all etries in directory";
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
        internal override string ShortDescription => "restarts this computer";
        internal override int Execute(string[] args)
        {
            Kernel.ShutdownPC(true);
            return 0;
        }
    }
    internal class ShutdownCommand : ConsoleCommand
    {
        internal override string Name => "shutdown";
        internal override string ShortDescription => "shutdowns this computer";
        internal override int Execute(string[] args)
        {
            Kernel.ShutdownPC(false);
            return 0;
        }
    }
    internal class WelcomeCommand : ConsoleCommand
    {
        internal override string Name => "welcome";
        internal override string ShortDescription => "shows logon welcome text";
        internal override int Execute(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Running wdOS Console Shell, \"just in case\" thing");
            Console.WriteLine("Start by typing \"help\" command");
            return 0;
        }
    }
}
