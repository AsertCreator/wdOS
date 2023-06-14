using Cosmos.Core;
using Cosmos.HAL;
using System;
using System.Collections.Generic;
using System.Linq;
using wdOS.Pillow;

namespace wdOS.Platform
{
    public static class DebugShellManager
    {
        public static List<ConsoleCommand> AllCommands;
        public static string Path = "/";
        public static bool Running = true;
        public static bool ShowPrompt = true;
        public static bool ClearScreen = true;
        public unsafe static int RunDebugShell()
        {
            try
			{
				WindowManager.Initialize();
				WindowManager.Start();

				return 0;
                // temporaily f u, debugshell
                AllCommands = new()
                {
                    new()
                    {
                        Name = "mod-cat",
                        Description = "shows contents of a module",
                        Execute = args =>
                        {
                            if (args.Length != 1) { Console.WriteLine("mod-cat: too much or too few arguments"); return 1; }
                            if (!int.TryParse(args[0], out int res)) { Console.WriteLine("mod-cat: not a module id"); return 1; }
                            if (res < 0 || res >= PlatformManager.LoadedModules.Count) { Console.WriteLine("mod-cat: invalid module id"); return 1; }

                            var module = PlatformManager.LoadedModules[int.Parse(args[res])];
                            Console.WriteLine(Utilities.FromCString((char*)module.ModuleAddress));
                            return 0;
                        }
                    },
                    new()
                    {
                        Name = "mod-list",
                        Description = "lists loaded kernel modules",
                        Execute = args =>
                        {
                            if (args.Length != 0) { Console.WriteLine("mod-list: too much arguments"); return 1; }
                            for (int i = 0; i < PlatformManager.LoadedModules.Count; i++)
                            {
                                var mod = PlatformManager.LoadedModules[i];
                                Console.WriteLine(mod.Name + ", start addr: 0x" + mod.ModuleStart.ToString("X8") + ", end addr: 0x" + mod.ModuleEnd + ", size: " + (mod.ModuleEnd - mod.ModuleStart));
			                }
                            return 0;
                        }
                    },
                    new()
                    {
                        Name = "cmd-list",
                        Description = "lists available commands",
                        Execute = args =>
                        {
                            if (args.Length != 0) { Console.WriteLine("cmd-list: too much arguments"); return 1; }
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
                        Name = "br-send",
                        Description = "sends broadcast",
                        Execute = args =>
                        {
                            if (args.Length != 0) { Console.WriteLine("br-send: too much arguments"); return 1; }
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
                        Name = "br-list",
                        Description = "lists received broadcasts",
                        Execute = args =>
                        {
                            if (args.Length != 0) { Console.WriteLine("br-list: too much arguments"); return 1; }
                            var broadcasts = BroadcastManager.GetAvailableBroadcasts();
                            for (int i = 0; i < broadcasts.Length; i++)
                            {
                                var broadcast = broadcasts[i];
                                Console.WriteLine("from    : " + broadcast.Sender.Username);
                                Console.WriteLine("to      : " + broadcast.Sendee.Username);
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
                        Name = "logcat",
                        Description = "prints out system log",
                        Execute = args =>
                        {
                            if (args.Length != 0) { Console.WriteLine("logcat: too much arguments"); return 1; }
                            Console.WriteLine(PlatformManager.SystemLog);
                            return 0;
                        }
                    },
                    new()
                    {
                        Name = "test-pillow",
                        Description = "test pillow vm",
                        Execute = args =>
                        {
                            if (args.Length != 0) { Console.WriteLine("test-pillow: too much arguments"); return 1; }

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
                        Name = "test-graphics",
                        Description = "test graphics",
                        Execute = args =>
                        {
                            if (args.Length != 0) { Console.WriteLine("test-graphics: too much arguments"); return 1; }

                            WindowManager.Initialize();
                            WindowManager.Start();

                            return 0;
                        }
                    },
                    new()
                    {
                        Name = "user-create",
                        Description = "creates user",
                        Execute = args =>
                        {
                            if (args.Length != 1) { Console.WriteLine("user-create: too much or too few arguments"); return 1; }
                            User user = new(args[0], "users", "", 0);
                            if (!UserManager.CreateUser(user))
                            {
                                Console.WriteLine("failed to create user");
                                return 1;
                            }

                            return 0;
                        }
                    },
                    new()
                    {
                        Name = "user-setpass",
                        Description = "sets user password",
                        Execute = args =>
                        {
                            if (args.Length != 2) { Console.WriteLine("user-setpass: too much or too few arguments"); return 1; }

                            User user = UserManager.FindByName(args[0]);
                            if (user == null) { Console.WriteLine("user-setpass: no such user"); return 1; }

                            user.SetUserLock(UserManager.UserLockTypePass, args[1]);

                            return 0;
                        }
                    },
                    new()
                    {
                        Name = "user-makeroot",
                        Description = "make certain user root",
                        Execute = args =>
                        {
                            if (args.Length != 1) { Console.WriteLine("user-makeroot: too much arguments"); return 1; }

                            User user = UserManager.FindByName(args[0]);
                            if (user == null) { Console.WriteLine("user-makeroot: no such user"); return 1; }

                            if (!user.IsRoot)
                            {
                                if (!UserManager.CurrentUser.IsRoot) { Console.WriteLine("user-makeroot: access denied"); return 1; }

                                user.MakeRoot();
                            }
                            else
                            {
                                Console.WriteLine("user-makeroot: user is already root");
                            }

                            return 0;
                        }
                    },
                    new()
                    {
                        Name = "user-makeregular",
                        Description = "make certain user regular",
                        Execute = args =>
                        {
                            if (args.Length != 1) { Console.WriteLine("user-makeregular: too much arguments"); return 1; }

                            User user = UserManager.FindByName(args[0]);
                            if (user == null) { Console.WriteLine("user-makeregular: no such user"); return 1; }
                            
                            if (user.IsRoot)
                            {
                                if (!UserManager.CurrentUser.IsRoot) { Console.WriteLine("user-makeregular: access denied"); return 1; }

                                if (!user.MakeRegular())
                                {
                                    Console.WriteLine("user-makeregular: failed to lower rights. reason may be that you are only " +
                                        "regular root in the system and you're trying to lower your rights. at least one regular root " +
                                        "shall persist in the system"); 
                                    return 1;
                                }
                            }
                            else
                            {
                                Console.WriteLine("user-makeregular: user is already regular");
                            }

                            return 0;
                        }
                    },
                    new()
                    {
                        Name = "user-remove",
                        Description = "removes user",
                        Execute = args =>
                        {
                            if (args.Length != 1) { Console.WriteLine("user-remove: too much or too few arguments"); return 1; }

                            if (!UserManager.RemoveUser(args[0]))
                            {
                                Console.WriteLine("user-remove: no such user"); 
                                return 1;
                            }

                            return 0;
                        }
                    },
                    new()
                    {
                        Name = "user-switch",
                        Description = "switcher current user",
                        Execute = args =>
                        {
                            if (args.Length != 1) { Console.WriteLine("user-switch: too much or too few arguments"); return 1; }

                            User user = UserManager.FindByName(args[0]);
                            if (user == null) { Console.WriteLine("user-switch: no such user"); return 1; }

                            if (user.UserLockType == 1)
                            {
                                Console.Write("password: ");
                                string pass = Console.ReadLine();
                                if (UserManager.Login(user.Username, pass, false) != UserManager.UserLoginResultLoggedInto)
                                {
                                    Console.WriteLine("failed to login, double check your password and username");
                                    return 1;
                                }
                            }
                            else
                            {
                                if (UserManager.Login(user.Username, "", false) != UserManager.UserLoginResultLoggedInto)
                                {
                                    Console.WriteLine("failed to login");
                                    return 1;
                                }
                            }

                            return 0;
                        }
                    },
                    new()
                    {
                        Name = "user-list",
                        Description = "lists available users",
                        Execute = args =>
                        {
                            if (args.Length != 0) { Console.WriteLine("user-list: too much arguments"); return 1; }

                            UserManager.EnumerateUsers(x => 
                            {
                                Console.WriteLine(x.Username + ", is root: " + x.IsRoot + ", is hidden: " + x.IsHidden + ", is replicatable: " + x.IsReplicated + ", is local: " + x.IsLocal);
                            }, 
                            true);

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
        public static void ShowWelcomeMessage()
        {
            var color = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("wdOS Platform Debug Shell, version " + PlatformManager.GetPlatformVersion());
            Console.ForegroundColor = color;
        }
        public static void PrintPrompt(string cmd)
        {
            if (ShowPrompt)
            {
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.Write(UserManager.CurrentUser.Username + " ");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write(Path);
                Console.ForegroundColor = ConsoleColor.Gray;
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
                        else return false;
                    }
                }
                catch (NotImplementedException)
                {
                    Console.WriteLine("debugsh: command (\"" + words[0] + "\") is not implemented");
                }
                catch (InsufficientPrivilegesException)
                {
                    Console.WriteLine("debugsh: command (\"" + words[0] + "\") requires more privileges than given");
                }
                catch
                {
                    Console.WriteLine("debugsh: current command crashed");
                }
            }
            return true;
        }
        public static void ExecuteScriptFile(string path)
        {
            string[] lines = FileSystemManager.ReadStringFile(path).Split('\n');
            for (int i = 0; i < lines.Length; i++)
                ExecuteCmd(lines[i]);
        }
        public static ConsoleCommand FindCommandByName(string name)
        {
            for (int i = 0; i < AllCommands.Count; i++)
            {
                var command = AllCommands[i];
                if (command.Name == name) return command;
            }
            return null;
        }
    }
    public sealed class ConsoleCommand
    {
        public string Name;
        public string Description;
        public Func<string[], int> Execute;
    }
}