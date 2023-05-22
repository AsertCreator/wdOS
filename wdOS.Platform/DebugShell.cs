﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using wdOS.Platform.GraphicalPlatform;

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
                        Name = "testmt",
                        Description = "test multithreading",
                        Execute = args =>
                        {
                            PlatformManager.GlobalGarbage = 0;

                            Cosmos.System.Thread th = new Cosmos.System.Thread(() =>
                            {
                                DateTime dt = DateTime.Now.AddSeconds(5);
                                while (dt > DateTime.Now) { }
                                PlatformManager.GlobalGarbage = 5;
                            });
                            th.Start();

                            DateTime dt = DateTime.Now.AddSeconds(7);
                            while (dt > DateTime.Now) { }

                            if (PlatformManager.GlobalGarbage == 5) Console.WriteLine("test succeded!");
                            else Console.WriteLine("test failed!");

                            th.Stop();

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