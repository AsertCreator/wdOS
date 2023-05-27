using Cosmos.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using wdOS.Pillow;

namespace wdOS.Platform
{
    internal static class DebugShell
    {
        internal static List<ConsoleCommand> AllCommands;
        internal static string Path = "/";
        internal static bool Running = true;
        internal static bool ShowPrompt = true;
        internal static bool ClearScreen = true;
        internal unsafe static int RunDebugShell()
        {
            try
            {
                AllCommands = new()
                {
                    new()
                    {
                        Name = "module",
                        Description = "shows contents of a module",
                        Execute = args =>
                        {
                            if (args.Length != 1) { Console.WriteLine("module: invalid count of arguments"); return 1; }
                            var module = PlatformManager.LoadedModules[int.Parse(args[0])];
                            Console.WriteLine(Utilities.FromCString((char*)module.ModuleAddress));
                            return 0;
                        }
                    },
                    new()
                    {
                        Name = "help",
                        Description = "shows help menu",
                        Execute = args =>
                        {
                            if (args.Length != 0) { Console.WriteLine("help: invalid count of arguments"); return 1; }
                            for (int i = 0; i < AllCommands.Count; i++)
                            {
                                var cmd = AllCommands[i];
                                Console.WriteLine(cmd.Name + " - " + cmd.Description);
                            }
                            return 0;
                        }
                    },
                    new()
                    {
                        Name = "send",
                        Description = "sends broadcast",
                        Execute = args =>
                        {
                            if (args.Length != 0) { Console.WriteLine("send: invalid count of arguments"); return 1; }
                            Console.Write("subject: ");
                            var subject = Console.ReadLine();
                            Console.Write("message: ");
                            var message = Console.ReadLine();
                            Console.Write("to: ");
                            var sendto = UserManager.FindByName(Console.ReadLine());
                            BroadcastManager.SendBroadcast(sendto, subject, message);
                            Console.WriteLine("sent!");
                            return 0;
                        }
                    },
                    new()
                    {
                        Name = "brlist",
                        Description = "lists received broadcasts",
                        Execute = args =>
                        {
                            if (args.Length != 0) { Console.WriteLine("brlist: invalid count of arguments"); return 1; }
                            var broadcasts = BroadcastManager.GetAvailableBroadcasts();
                            for (int i = 0; i < broadcasts.Length; i++)
                            {
                                var broadcast = broadcasts[i];
                                Console.WriteLine("from    : " + broadcast.Sender.UserName);
                                Console.WriteLine("to      : " + broadcast.Sendee.UserName);
                                Console.WriteLine("subject : " + broadcast.Subject);
                                Console.WriteLine("message : " + broadcast.Message);
                                Console.WriteLine("sent    : " + broadcast.SendTime);
                                Console.WriteLine();
                            }
                            return 0;
                        }
                    },
                    new()
                    {
                        Name = "testpillow",
                        Description = "test pillow vm",
                        Execute = args =>
                        {
                            EEExecutable exec = new();
                            exec.AllStringLiterals.Add("Hello World!");
                            exec.AllFunctions.Add(EEAssembler.AssemblePillowIL(
                                ".maxlocal 1\n" +
                                "pushobj\n" +
                                "pushint.b 0\n" +
                                "setlocal\n" +
                                "pushint.b 0\n" +
                                "getlocal\n" +
                                "pushint.b 0\n" +
                                "pushint.b 5\n" +
                                "setfield\n" +
                                "pushstr 0\n" +
                                "pushint.i 5345446\n" +
                                "setfield\n" +
                                "ret"));
                            exec.Entrypoint = exec.AllFunctions[0];
                            ExecutionEngine.AllFunctions.Add(exec.Entrypoint);

                            var res = exec.Execute("");
                            Console.WriteLine(res.ReturnedValue.ToString());

                            return 0;
                        }
                    },
                    new()
                    {
                        Name = "testgraphics",
                        Description = "test graphics",
                        Execute = args =>
                        {
                            GraphicsManager.Initialize();
                            Cosmos.System.MouseManager.ScreenWidth = (uint)GraphicsManager.CurrentMode.Width;
                            Cosmos.System.MouseManager.ScreenHeight = (uint)GraphicsManager.CurrentMode.Height;
                            bool running = true;

                            while (running)
                            {
                                var mouse = new Rect() 
                                { 
                                    Top = (int)Cosmos.System.MouseManager.Y,
                                    Left = (int)Cosmos.System.MouseManager.X,
                                    Right = (int)(GraphicsManager.CurrentMode.Width - Cosmos.System.MouseManager.X),
                                    Bottom = (int)(GraphicsManager.CurrentMode.Height - Cosmos.System.MouseManager.Y),
                                };
                                GraphicsManager.FillRectangle(System.Drawing.Color.Beige, GraphicsManager.ScreenSpanRect);
                                GraphicsManager.FillRectangle(System.Drawing.Color.Black, mouse);
                                GCImplementation.Free(mouse);
                                
                                GraphicsManager.Swap();

                                if (Cosmos.System.KeyboardManager.TryReadKey(out Cosmos.System.KeyEvent ev))
                                {
                                    switch (ev.Key)
                                    {
                                        case Cosmos.System.ConsoleKeyEx.Q:
                                            if ((ev.Modifiers & ConsoleModifiers.Control) != 0 &&
                                                (ev.Modifiers & ConsoleModifiers.Alt) != 0)
                                                running = false;
                                            break;
                                    }
                                }
                            }

                            GraphicsManager.Disable();

                            return 0;
                        }
                    }
                };

                ShowWelcomeMessage();
                Console.ForegroundColor = ConsoleColor.White;
                if (FileSystemManager.VFS.GetVolumes().Count == 0)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("debugsh: couldn't detect any volumes!");
                }
                Running = true;

                while (Running)
                {
                    PrintPrompt(null);

                    var cmd = Console.ReadLine();
                    if (!ExecuteCmd(cmd))
                    {
                        Console.WriteLine("debugsh: specified command, application or script doesn't exist");
                        continue;
                    }
                }
                return 0;
            }
            catch
            {
                Console.WriteLine("debugsh: unknown error");
                return 1;
            }
        }
        internal static void ShowWelcomeMessage()
        {
            var color = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("wdOS Platform Debug Shell, version " + PlatformManager.GetPlatformVersion());
            Console.ForegroundColor = color;
        }
        internal static void PrintPrompt(string cmd)
        {
            if (ShowPrompt)
            {
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.Write(UserManager.CurrentUser.UserName + " ");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write(Path);
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write(" $ ");
                if (cmd != null) Console.WriteLine(cmd);
            }
        }
        internal static bool ExecuteCmd(string cmdline)
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
                        Console.ForegroundColor = ConsoleColor.Gray;
                        cmd.Execute(args);
                        return true;
                    }
                    else
                    {
                        var path = Path + words[0];
                        if (!FileSystemManager.FileExists(path)) return false;
                        if (path.EndsWith(".tss"))
                        {
                            ExecuteScriptFile(path);
                            return true;
                        }
                        else if (path.EndsWith(".tse"))
                        {
                            ExecuteBinaryFile(path, Utilities.ConcatArray(args));
                            return true;
                        }
                        else return false;
                    }
                }
                catch (NotImplementedException)
                {
                    Console.WriteLine("debugsh: command (\"" + words[0] + "\") is not implemented");
                }
                catch
                {
                    Console.WriteLine("debugsh: current command crashed");
                }
            }
            return true;
        }
        internal static void ExecuteScriptFile(string path)
        {
            string[] lines = FileSystemManager.ReadStringFile(path).Split('\n');
            for (int i = 0; i < lines.Length; i++)
                ExecuteCmd(lines[i]);
        }
        internal static int ExecuteBinaryFile(string path, string args)
        {
            ProcessRuntime.Execute(path, args);
            return 0;
        }
        internal static ConsoleCommand FindCommandByName(string name)
        {
            for (int i = 0; i < AllCommands.Count; i++)
            {
                var command = AllCommands[i];
                if (command.Name == name) return command;
            }
            return null;
        }
    }
    internal sealed class ConsoleCommand
    {
        internal string Name;
        internal string Description;
        internal Func<string[], int> Execute;
    }
}