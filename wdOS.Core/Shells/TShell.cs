using System;
using System.Collections.Generic;
using System.Linq;
using wdOS.Core.Commands;

namespace wdOS.Core.Shells
{
    internal class TShell : Shell
    {
        internal static List<ConsoleCommand> AllCommands = new();
        internal static int LastErrorCode;
        internal static byte Volume;
        internal static string Path = "";
        internal static bool Running = true;
        internal override string Name => "TShell";
        internal override int MajorVersion => Kernel.BuildConstants.VersionMajor;
        internal override int MinorVersion => Kernel.BuildConstants.VersionMinor;
        internal override int PatchVersion => Kernel.BuildConstants.VersionPatch;
        internal void Init()
        {
            AllCommands.Add(new HelpCommand());
            AllCommands.Add(new EchoCommand());
            AllCommands.Add(new ClearCommand());
            AllCommands.Add(new ChangePathCommand());
            AllCommands.Add(new ListCommand());
            AllCommands.Add(new FormatCommand());
            AllCommands.Add(new ListPartCommand());
            AllCommands.Add(new ChangeVolumeCommand());
            AllCommands.Add(new ExecuteSWCommand());
            AllCommands.Add(new MkdirCommand());
            AllCommands.Add(new ShutdownCommand());
            AllCommands.Add(new RestartCommand());
            AllCommands.Add(new WelcomeCommand());
            AllCommands.Add(new FunCommand());
        }
        internal override void BeforeRun()
        {
            Init();
            new WelcomeCommand().Execute(new string[] { });
            Console.ForegroundColor = ConsoleColor.White;
            if (Kernel.VFS.Disks.Count == 0)
            {
                Console.ForegroundColor = ConsoleColor.Red; 
                Console.WriteLine("wdOS cannot run without any attached drives, you must have at least one"); 
                while (true) { }
            }
            if (Kernel.VFS.GetVolumes().Count == 0)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write("It looks like you have no volumes that we can detect, " +
                    "but we format format entire drive for that. Do you want to format drive? [y/n]: ");
                if (Console.ReadKey().Key == ConsoleKey.Y)
                { new FormatCommand().Execute(new string[] { }); }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red; 
                    Console.WriteLine("wdOS cannot run without disk volumes, you must have at least one"); 
                    while (true) { } 
                }
            }
            Running = true;
        }
        internal override void Run()
        {
            try
            {
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write($"{GetFullPath()} > ");
                Console.ForegroundColor = ConsoleColor.White;
                var words = Console.ReadLine().Split(' ');
                var cmdname = words[0];
                var cmdargs = ArrayUtils.SkipArray(words, 1);
                var cmd = FindCommandByName(cmdname);
                Console.ForegroundColor = ConsoleColor.Gray;
                if (cmd != null) { LastErrorCode = cmd.Execute(cmdargs); }
                else
                { Console.WriteLine("This command doesn't exist!"); LastErrorCode = 1; }
                Console.WriteLine();
            }
            catch (Exception e)
            {
                Console.WriteLine($"This comamnd crashed with message: {e.Message}");
                LastErrorCode = int.MinValue;
            }
        }
        internal static string GetFullPath() => $"{Volume}:\\{Path}";
        internal static void GoLowerPath(string nextdir)
        {
            string path = Path + '\\' + nextdir;
            if (path.Length < 255) { Path = PathUtils.CanonicalPath(true, path); }
        }
        internal static ConsoleCommand FindCommandByName(string name)
        {
            foreach (ConsoleCommand command in AllCommands)
            { if (command.Name == name) { return command; } }
            return null;
        }
    }
    internal abstract class ConsoleCommand
    {
        internal abstract string Name { get; }
        internal abstract string ShortDescription { get; }
        internal abstract int Execute(string[] args);
    }
}