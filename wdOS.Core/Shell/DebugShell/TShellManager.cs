using System;
using System.Collections.Generic;
using System.Linq;
using wdOS.Core.Foundation;
using static wdOS.Core.Foundation.SystemDatabase;

namespace wdOS.Core.Shell.DebugShell
{
    public class TShellManager : ShellBase
    {
        public static List<ConsoleCommand> AllCommands = new();
        public static int LastErrorCode;
        public static string Path = "0:\\";
        public static bool Running = true;
        public static bool ShowPrompt = true;
        public static bool ClearScreen = true;
        public override string ShellName => "TShell";
        public override string ShellDesc => "basically optimized shell";
        public override int ShellMajorVersion => BuildConstants.VersionMajor;
        public override int ShellMinorVersion => BuildConstants.VersionMinor;
        public override int ShellPatchVersion => BuildConstants.VersionPatch;
        public override void ShellBeforeRun()
        {
            try
            {
                FileSystemCommands.AddComamnds();
                UtilityCommands.AddCommands();
                UserCommands.AddCommands();
                WWWCommands.AddCommands();

                new UtilityCommands.WelcomeCommand().Execute(Array.Empty<string>());
                Console.ForegroundColor = ConsoleColor.White;
                if (FileSystem.VFS.GetVolumes().Count == 0)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("tshell: couldn't detect any volumes!");
                }
                Running = true;
                IsRunning = true;
            }
            catch
            {
                Console.WriteLine("Can't start TShell due to unexcepted error!");
                Kernel.WaitForShutdown(false, 5);
            }
        }
        public override void ShellRun()
        {
            PrintPrompt(null);

            var cmd = Console.ReadLine();
            if (!ExecuteCmd(cmd))
            {
                Console.WriteLine($"tshell: specified command, application or script doesn't exist");
                LastErrorCode = 1;
                return;
            }
        }
        public static void PrintPrompt(string cmd)
        {
            if (ShowPrompt)
            {
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.Write($"{CurrentUser.UserName} ");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write($"{Path}");
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write(" $ ");
                if (cmd != null) Console.WriteLine(cmd);
            }
        }
        public static bool ExecuteCmd(string cmdline)
        {
            if (!cmdline.StartsWith("::") && !string.IsNullOrWhiteSpace(cmdline))
            {
                string[] words = cmdline.Split(' ');
                var cmd = FindCommandByName(words[0]);
                string[] args = words.Skip(1).ToArray();
                try
                {
                    if (cmd != null)
                    {
                        cmd.CurrentCmdArgs = args;
                        Console.ForegroundColor = ConsoleColor.Gray;
                        LastErrorCode = cmd.Execute(cmd.CurrentCmdArgs);
                        //if (addtihistory) { CommandHistory.Add(cmdline); }
                        return true;
                    }
                    else
                    {
                        var path = Path + words[0];
                        if (!FileSystem.FileExists(path)) return false;
                        if (path.EndsWith(".tss"))
                        {
                            LastErrorCode = ExecuteScriptFile(path);
                            //if (addtihistory) { CommandHistory.Add(cmdline); }
                            return true;
                        }
                        else if (path.EndsWith(".tse"))
                        {
                            LastErrorCode = ExecuteBinaryFile(path);
                            //if (addtihistory) { CommandHistory.Add(cmdline); }
                            return true;
                        }
                        else return false;
                    }
                }
                catch (NotImplementedException)
                {
                    Console.WriteLine($"tshell: command (\"{words[0]}\") is not implemented");
                    LastErrorCode = int.MinValue;
                }
                catch
                {
                    Console.WriteLine("tshell: current command crashed");
                    LastErrorCode = int.MinValue;
                }
            }
            return true;
        }
        public static int ExecuteScriptFile(string path)
        {
            try
            {
                string[] lines = FileSystem.ReadStringFile(path).Split('\n');
                foreach (var line in lines) ExecuteCmd(line);
                return LastErrorCode;
            }
            catch { return int.MinValue / 2; }
        }
        public static int ExecuteBinaryFile(string path)
        {
            Console.WriteLine($"dynlink: \"{path}\" executable is not supported");
            return 0;
        }
        public static ConsoleCommand FindCommandByName(string name)
        {
            foreach (IHelpEntry command in AllCommands)
            { if (command.EntryName == name) { return (ConsoleCommand)command; } }
            return null;
        }
        public override void ShellAfterRun() { }
    }
    public abstract class ConsoleCommand : IHelpEntry
    {
        public string[] CurrentCmdArgs;
        public abstract string Name { get; }
        public abstract string Description { get; }
        string IHelpEntry.EntryName => Name;
        string IHelpEntry.EntryDescription => Description;
        public abstract int Execute(string[] args);
    }
}