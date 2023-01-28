using System;
using System.Collections.Generic;
using System.Linq;
using wdOS.Core.Foundation;

namespace wdOS.Core.Shell.TShell
{
    internal class TShellManager : ShellBase
    {
        internal static List<IHelpEntry> AllCommands = new()
        {
            new HelpCommand(), new EchoCommand(), new ClearCommand(),
            new ChangePathCommand(), new ListCommand(), new FormatCommand(),
            new ListPartCommand(), new LogCatCommand(), new MkdirCommand(),
            new ShutdownCommand(), new RestartCommand(), new WelcomeCommand(),
            new SUtilsCommand(), new MIVCommand(), new CatCommand(),
            new CShellCommand(), new RepeatCommand(), new FunCommand()
        };
        internal static CircularList<string> CommandHistory = new(128);
        internal static int LastErrorCode;
        internal static string Path = "0:\\";
        //internal static User CurrentUser = "root";
        internal static bool Running = true;
        internal static bool ShowPrompt = true;
        internal static bool ClearScreen = true;
        internal override string Name => "TShell";
        internal override int MajorVersion => Kernel.BuildConstants.VersionMajor;
        internal override int MinorVersion => Kernel.BuildConstants.VersionMinor;
        internal override int PatchVersion => Kernel.BuildConstants.VersionPatch;

        internal override void BeforeRun()
        {
            try
            {
                _ = ((ConsoleCommand)AllCommands[11]).Execute(Array.Empty<string>());
                Console.ForegroundColor = ConsoleColor.White;
                if (FileSystem.VFS.GetVolumes().Count == 0)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write("It looks like you have no volumes that we can detect, " +
                        "but we format format entire drive for that. Do you want to format drive? [y/n]: ");
                    if (Console.ReadKey().Key == ConsoleKey.Y)
                    { _ = new FormatCommand().Execute(Array.Empty<string>()); }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("wdOS cannot run without disk volumes, you must have at least one");
                        ErrorHandler.Panic(3);
                    }
                }
                Running = true;
            }
            catch
            {
                Console.WriteLine("Can't start TShell due to unexcepted error!");
                Kernel.WaitForShutdown(false, 5);
            }
        }
        internal override void Run()
        {
            PrintPrompt(null);
            try
            {
                var cmd = Console.ReadLine();
                var name = cmd.Split(' ')[0];
                if (!ExecuteCmd(cmd)) 
                { 
                    Console.WriteLine($"The specified ('{name}') command, application or script doesn't exist!"); 
                    LastErrorCode = 1; 
                    return; 
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Current command crashed! Exception: {e.GetType().Name}, message: {e.Message}");
                LastErrorCode = int.MinValue;
            }
        }
        internal static void PrintPrompt(string cmd)
        {
            if (ShowPrompt)
            {
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Blue;
                //Console.Write($"{CurrentUser.EntryName} ");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write($"{Path}");
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write(" $ ");
                if (cmd != null) Console.WriteLine(cmd);
            }
        }
        internal static bool ExecuteCmd(string cmdline, bool addtihistory = true)
        {
            if (!cmdline.StartsWith("::"))
            {
                string[] words = cmdline.Split(' ');
                var cmd = FindCommandByName(words[0]);
                try
                {
                    if (cmd != null)
                    {
                        cmd.CurrentCmdArgs = Utilities.SkipArray(words, 1);
                        Console.ForegroundColor = ConsoleColor.Gray;
                        LastErrorCode = cmd.Execute(cmd.CurrentCmdArgs);
                        if (addtihistory) { CommandHistory.Add(cmdline); }
                        return true;
                    }
                    else
                    {
                        var path = Path + words[0];
                        if (!FileSystem.FileExists(path)) return false;
                        if (path.EndsWith(".tss"))
                        {
                            LastErrorCode = ExecuteScriptFile(path);
                            if (addtihistory) { CommandHistory.Add(cmdline); }
                            return true;
                        }
                        else if (path.EndsWith(".tse"))
                        {
                            LastErrorCode = ExecuteBinaryFile(path);
                            if (addtihistory) { CommandHistory.Add(cmdline); }
                            return true;
                        }
                        else return false;
                    }
                }
                catch (NotImplementedException)
                {
                    Console.WriteLine($"Current command (\"{words[0]}\") is not implemented!");
                    LastErrorCode = int.MinValue;
                }
                catch { throw; }
            }
            return true;
        }
        internal static int ExecuteScriptFile(string path)
        {
            try
            {
                string[] lines = FileSystem.ReadStringFile(path).Split('\n');
                foreach (var line in lines)
                {
                    ExecuteCmd(line, false);
                }
                return LastErrorCode;
            }
            catch { return int.MinValue / 2; }
        }
        internal static int ExecuteBinaryFile(string path)
        {
            Console.WriteLine("Binary files are not implemented");
            return 0;
        }
        internal static ConsoleCommand FindCommandByName(string name)
        {
            foreach (ConsoleCommand command in AllCommands)
            { if (command.Name == name) { return command; } }
            return null;
        }
    }
    internal abstract class ConsoleCommand : IHelpEntry
    {
        internal string[] CurrentCmdArgs;
        internal abstract string Name { get; }
        internal abstract string Description { get; }
        string IHelpEntry.EntryName => Name;
        string IHelpEntry.EntryDescription => Description;
        internal abstract int Execute(string[] args);
    }
}