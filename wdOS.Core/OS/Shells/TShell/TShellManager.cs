using System;
using System.Collections.Generic;
using wdOS.Core.OS.Foundation;

namespace wdOS.Core.OS.Shells.TShell
{
    internal class TShellManager : Shell, IPackage
    {
        internal static List<IHelpEntry> AllCommands = new()
        {
            new HelpCommand(), new EchoCommand(), new ClearCommand(),
            new ChangePathCommand(), new ListCommand(), new FormatCommand(),
            new ListPartCommand(), new LogCatCommand(), new MkdirCommand(),
            new ShutdownCommand(), new RestartCommand(), new WelcomeCommand(),
            new SUtilsCommand(), new MIVCommand(), new CatCommand(),
            new CShellCommand(), new FunCommand()
        };
        internal static List<string> CommandHistory = new(128);
        internal static int LastErrorCode;
        internal static string Path = "0:\\";
        //internal static User CurrentUser = "root";
        internal static bool Running = true;
        internal override string Name => "TShell";
        internal override int MajorVersion => Kernel.BuildConstants.VersionMajor;
        internal override int MinorVersion => Kernel.BuildConstants.VersionMinor;
        internal override int PatchVersion => Kernel.BuildConstants.VersionPatch;
        string IPackage.Name => Name.ToLower();
        string IPackage.Description => "wdOS built-in terminal shell";
        string[] IPackage.Files => new string[] { "wdOS.Core.bin" };
        int IPackage.MajorVersion => MajorVersion;
        int IPackage.MinorVersion => MinorVersion;
        int IPackage.PatchVersion => PatchVersion;
        PackageDatabase.PackageType IPackage.Type => PackageDatabase.PackageType.SystemShell;

        internal override void BeforeRun()
        {
            try
            {
                _ = ((ConsoleCommand)AllCommands[11]).Execute(new string[] { });
                Console.ForegroundColor = ConsoleColor.White;
                if (FileSystem.VFS.GetVolumes().Count == 0)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write("It looks like you have no volumes that we can detect, " +
                        "but we format format entire drive for that. Do you want to format drive? [y/n]: ");
                    if (Console.ReadKey().Key == ConsoleKey.Y)
                    { _ = new FormatCommand().Execute(new string[] { }); }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("wdOS cannot run without disk volumes, you must have at least one");
                        Kernel.Panic(3);
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
            try
            {
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Blue;
                //Console.Write($"{CurrentUser.EntryName} ");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write($"{Path}");
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write(" $ ");
                {
                    var words = Console.ReadLine().Split(' ');
                    var cmd = FindCommandByName(words[0]);
                    cmd.CurrentCmdArgs = Utilities.SkipArray(words, 1);
                    Console.ForegroundColor = ConsoleColor.Gray;
                    if (cmd != null) { LastErrorCode = cmd.Execute(cmd.CurrentCmdArgs); }
                    else
                    { Console.WriteLine($"Command '{words[0]}' doesn't exist!"); LastErrorCode = 1; return; }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"This comamnd crashed with message: {e.Message}");
                LastErrorCode = int.MinValue;
            }
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