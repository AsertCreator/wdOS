using Cosmos.System.FileSystem.VFS;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using wdOS.Core.Foundation;
using wdOS.Core.Shell.ThirdParty;

namespace wdOS.Core.Shell.DebugShell
{
    public static class FileSystemCommands
    {
        public static void AddComamnds()
        {
            TShellManager.AllCommands.Add(new ChangePathCommand());
            TShellManager.AllCommands.Add(new MkdirCommand());
            TShellManager.AllCommands.Add(new TouchCommand());
            TShellManager.AllCommands.Add(new ListPartCommand());
            TShellManager.AllCommands.Add(new ListCommand());
            TShellManager.AllCommands.Add(new MIVCommand());
            TShellManager.AllCommands.Add(new CatCommand());
            TShellManager.AllCommands.Add(new EraseCommand());
            TShellManager.AllCommands.Add(new RmdirCommand());
            TShellManager.AllCommands.Add(new DeleteCommand());
            TShellManager.AllCommands.Add(new WriteCommand());
        }
        public class ChangePathCommand : ConsoleCommand
        {
            public override string Name => "cd";
            public override string Description => "changes directory";
            public override int Execute(string[] args)
            {
                string path = Utilities.ConnectArgs(args, '\\');
                if (args.Length == 0) return 0;
                if (TShellManager.Path.Split('\\').Length == 1 && path.StartsWith("..")) return 0;
                else if (path.Contains(':'))
                {
                    TShellManager.Path = path[..2];
                }
                else if (path.StartsWith(".."))
                {
                    if (TShellManager.Path.Length > 3)
                    {
                        string[] var0 = TShellManager.Path.Split("\\").Reverse().ToArray();
                        TShellManager.Path = string.Join('\\', var0.Skip(2).Reverse().ToArray()) + "\\";
                    }
                }
                else
                {
                    if (FileSystem.DirectoryExists(TShellManager.Path + path))
                        TShellManager.Path = TShellManager.Path + path + '\\';
                    else Console.WriteLine("cd: no such file or directory");
                }
                return 0;
            }
        }
        public class MkdirCommand : ConsoleCommand
        {
            public override string Name => "mkdir";
            public override string Description => "creates a directory in cd";
            public override int Execute(string[] args) 
            {
                var path = Path.Combine(TShellManager.Path, Utilities.ConcatArray(args));
                FileSystem.CreateDirectory(path);
                KernelLogger.Log($"Creating directory at {path}");
                return 0; 
            }
        }
        public class TouchCommand : ConsoleCommand
        {
            public override string Name => "touch";
            public override string Description => "creates a file in cd"/*and changes access time"*/;
            public override int Execute(string[] args) 
            { 
                FileSystem.WriteStringFile(Path.Combine(TShellManager.Path, Utilities.ConcatArray(args)), ""); 
                return 0; 
            }
        }
        public class ListPartCommand : ConsoleCommand
        {
            public override string Name => "lspart";
            public override string Description => "lists all partitions";
            public override int Execute(string[] args)
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
        public class ListCommand : ConsoleCommand
        {
            public override string Name => "ls";
            public override string Description => "lists all etries in directory";
            public override int Execute(string[] args)
            {
                var list = VFSManager.GetDirectoryListing(TShellManager.Path);
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
        public class MIVCommand : ConsoleCommand
        {
            public override string Name => "miv";
            public override string Description => "minimalistic text editor";
            public override int Execute(string[] args)
            {
                Console.WriteLine("MIV is not my program, its written by bartashevich and modified by me");
                Console.WriteLine("Opening in 2 seconds");
                Utilities.WaitFor(2000);
                MIV.StartMIV();
                return 0;
            }
        }
        public class CatCommand : ConsoleCommand
        {
            public override string Name => "cat";
            public override string Description => "concats contents of the file";
            public override int Execute(string[] args)
            {
                string path = TShellManager.Path + Utilities.ConnectArgs(args, '\\');
                if (!FileSystem.FileExists(path)) { Console.WriteLine("This file does not exist!"); return 1; }
                Console.WriteLine(FileSystem.ReadStringFile(path));
                return 0;
            }
        }
        public class EraseCommand : ConsoleCommand
        {
            public override string Name => "erase";
            public override string Description => "erases drive and prepares it for system";
            public override int Execute(string[] args)
            {
                if (args.Length != 1)
                {
                    Console.WriteLine("erase: invalid count of arguments");
                    return 1;
                }
                var drive = args[0];
                var volumes = VFSManager.GetVolumes();
                for(int i = 0; i < volumes.Count; i++)
                {
                    if (volumes[i].mFullPath == drive)
                    {
                        var list = VFSManager.GetDirectoryListing(volumes[i].mFullPath);
                        for(int e = 0; e < list.Count; e++)
                        {
                            if (list[e].mEntryType == Cosmos.System.FileSystem.Listing.DirectoryEntryTypeEnum.Directory)
                                VFSManager.DeleteDirectory(list[e].mFullPath, true);
                            else
                                VFSManager.DeleteFile(list[e].mFullPath);
                        }
                        SystemInteraction.SetupSystem();
                        return 0;
                    }
                }
                Console.WriteLine($"erase: invalid volume");
                return 1;
            }
        }
        public class RmdirCommand : ConsoleCommand
        {
            public override string Name => "rmdir";
            public override string Description => "removes directory";
            public override int Execute(string[] args)
            {
                if (args.Length != 1)
                {
                    Console.WriteLine("rmdir: invalid count of arguments");
                    return 1;
                }
                string abspath = TShellManager.Path + args[0] + '\\';
                Console.WriteLine(abspath);
                if (!FileSystem.FileExists(abspath))
                {
                    Console.WriteLine("rmdir: no such directory");
                    return 1;
                }
                FileSystem.DeleteDirectory(abspath);
                return 0;
            }
        }
        public class DeleteCommand : ConsoleCommand
        {
            public override string Name => "del";
            public override string Description => "deletes file";
            public override int Execute(string[] args)
            {
                if (args.Length != 1)
                {
                    Console.WriteLine("del: invalid count of arguments");
                    return 1;
                }
                string abspath = TShellManager.Path + args[0];
                Console.WriteLine(abspath);
                if (!FileSystem.FileExists(abspath))
                {
                    Console.WriteLine("del: no such file");
                    return 1;
                }
                FileSystem.DeleteFile(abspath);
                return 1;
            }
        }
        public class WriteCommand : ConsoleCommand
        {
            public override string Name => "write";
            public override string Description => "writes text to file";
            public override int Execute(string[] args)
            {
                if (args.Length < 2)
                {
                    Console.WriteLine("write: invalid count of arguments");
                    return 1;
                }
                string abspath = TShellManager.Path + args[0];
                Console.WriteLine(abspath);
                string content = Utilities.ConcatArray(args.Skip(1).ToArray());
                if (FileSystem.DirectoryExists(abspath))
                {
                    Console.WriteLine("write: specified path is directory");
                    return 1;
                }
                FileSystem.WriteStringFile(abspath, content);
                return 1;
            }
        }
    }
}
