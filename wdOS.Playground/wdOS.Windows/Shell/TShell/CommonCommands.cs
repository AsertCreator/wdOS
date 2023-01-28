using System;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using wdOS.Core.Foundation;
using wdOS.Core.Shell.ThirdParty;

namespace wdOS.Core.Shell.TShell
{
    internal class ChangePathCommand : ConsoleCommand
    {
        internal override string Name => "cd";
        internal override string Description => "changes directory";
        internal override int Execute(string[] args)
        {
            string path = Utilities.ConnectArgs(args, '\\');
            if (args.Length == 0) return 0;
            if (TShellManager.Path.Split('\\').Length == 1 && path.StartsWith("..")) return 0;
            else if (path.Contains(':'))
            {
                TShellManager.Path = path[..2];
                Environment.CurrentDirectory = TShellManager.Path;
            }
            else if (path.StartsWith(".."))
            {
                if (TShellManager.Path.Length > 3)
                {
                    string[] var0 = TShellManager.Path.Split("\\").Reverse().ToArray();
                    TShellManager.Path = string.Join('\\', Utilities.SkipArray(var0, 2).Reverse().ToArray()) + "\\";
                    Environment.CurrentDirectory = TShellManager.Path;
                }
            }
            else
            {
                if (FileSystem.DirectoryExists(TShellManager.Path + path))
                {
                    TShellManager.Path = TShellManager.Path + path + '\\';
                    Environment.CurrentDirectory = TShellManager.Path;
                }
                else Console.WriteLine(path + " does not exist!");
            }
            return 0;
        }
    }
    internal class MkdirCommand : ConsoleCommand
    {
        internal override string Name => "mkdir";
        internal override string Description => "creates a directory in cd";
        internal override int Execute(string[] args) { FileSystem.CreateDirectory(Path.Combine(TShellManager.Path, Utilities.ConnectArgs(args))); return 0; }
    }
    internal class TouchCommand : ConsoleCommand
    {
        internal override string Name => "touch";
        internal override string Description => "creates a file in cd"/*and changes access time"*/;
        internal override int Execute(string[] args) { FileSystem.WriteStringFile(Path.Combine(TShellManager.Path, Utilities.ConnectArgs(args)), ""); return 0; }
    }
    internal class FunCommand : ConsoleCommand
    {
        internal override string Name
        {
            get
            {
                string name = "";
                var random = new Random();
                for (int i = 0; i < 5; i++)
                { name += Utilities.EnglishAlphabet[random.Next(Utilities.EnglishAlphabet.Length)]; }
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
        internal override int Execute(string[] args) { IHelpEntry.ShowHelpMenu(TShellManager.AllCommands); return 0; }
    }
    internal class EchoCommand : ConsoleCommand
    {
        internal override string Name => "echo";
        internal override string Description => "echoes any arguments";
        internal override int Execute(string[] args) 
        {
            string print = Utilities.ConnectArgs(args);
            if (string.IsNullOrEmpty(print.Trim()))
            {
                Console.WriteLine($"Current prompt state: {TShellManager.ShowPrompt}");
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
            Console.WriteLine(); 
            return 0; 
        }
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
            Console.WriteLine("You can't use lspart on Windows");
            return 0;
        }
    }
    internal class ListCommand : ConsoleCommand
    {
        internal override string Name => "ls";
        internal override string Description => "lists all etries in directory";
        internal override int Execute(string[] args)
        {
            var count = 0;
            var list = Directory.GetFiles(TShellManager.Path);
            foreach (var file in list)
            {
                Console.WriteLine($"      {file} - {new FileInfo(Path.Combine(Environment.CurrentDirectory, file))}");
            }
            count += list.Count();
            list = Directory.GetDirectories(TShellManager.Path);
            foreach (var file in list)
            {
                Console.WriteLine($"<DIR> {file}");
            }
            count += list.Count();
            Console.WriteLine($"Total number of entries: {count}");
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
    internal class MIVCommand : ConsoleCommand
    {
        internal override string Name => "miv";
        internal override string Description => "minimalistic text editor";
        internal override int Execute(string[] args)
        {
            Console.WriteLine("MIV is not my program, its written by bartashevich and modified by me");
            Console.WriteLine("Opening in 4 seconds");
            Utilities.WaitFor(4000);
            MIV.StartMIV();
            return 0;
        }
    }
    internal class CatCommand : ConsoleCommand
    {
        internal override string Name => "cat";
        internal override string Description => "concats contents of the file";
        internal override int Execute(string[] args)
        {
            string path = TShellManager.Path + Utilities.ConnectArgs(args, '\\');
            if (!FileSystem.FileExists(path)) { Console.WriteLine("This file does not exist!"); return 1; }
            Console.WriteLine(FileSystem.ReadStringFile(path));
            return 0;
        }
    }
    internal class CShellCommand : ConsoleCommand
    {
        internal override string Name => "cshell";
        internal override string Description => "opens CShell GUI shell";
        internal override int Execute(string[] args)
        {
            Console.WriteLine("You can't use CShell, because it is not ported to Windows!");
            return 1;
        }
    }
    internal class RepeatCommand : ConsoleCommand
    {
        internal override string Name => "repeat";
        internal override string Description => "repeats certain command multiple times";
        internal override int Execute(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("You must eneter number of repeats and command!");
                return 1;
            }
            int repeats = int.Parse(args[0]);
            var cmd = Utilities.ConnectArgs(Utilities.SkipArray(args, 1));
            for (int i = 0; i < repeats; i++)
            {
                if (!TShellManager.ExecuteCmd(cmd))
                {
                    Console.WriteLine($"Command '{args[1]}' doesn't exist!"); return 1;
                }
            }
            return 0;
        }
    }
}
