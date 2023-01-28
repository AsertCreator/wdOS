using Cosmos.System.FileSystem.VFS;
using System;
using System.Linq;
using wdOS.Core.Foundation;

namespace wdOS.Core.Shell.TShell
{
    internal class FormatCommand : ConsoleCommand
    {
        internal override string Name => "format";
        internal override string Description => "format specific drive entirely";
        internal static string[] SupportedFilesystems = new string[] { "FAt32" };
        internal override int Execute(string[] args)
        {
            string filesystem = args.Length == 0 ? "FAT32" : args[0];
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("!!! BEFORE YOU CONTINUE !!!");
            Console.WriteLine($"You are going to entirely erase installed drive and format as {filesystem}");
            Console.WriteLine("You must to have drive backup, because this util can corrupt drive!");
            Console.WriteLine("Also this utility will turn connected drive to system drive");
            Console.Write("\nIf you have read this discalimer, press any key to format... ");
            Console.ReadKey(true);
            if (SupportedFilesystems.Contains(filesystem))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Clear();
                Console.WriteLine("!!! LAST WARNING !!!");
                Console.WriteLine("THIS UTILITY WILL ERASE ENTIRE DRIVE!");
                Console.Write("Do you really want to format this drive? [y/n]");
                Console.ForegroundColor = ConsoleColor.White;
                if (Console.ReadKey().Key == ConsoleKey.Y)
                {
                    Console.WriteLine($"Formatting drive as \"{filesystem}\"...");
                    var disk = VFSManager.GetDisks()[0];
                    disk.CreatePartition((int)(disk.Host.BlockSize * disk.Host.BlockCount / 1024 / 1024));
                    disk.FormatPartition(0, filesystem);
                    disk.MountPartition(0);
                    VFSManager.SetFileSystemLabel("0:", Kernel.SystemDriveLabel);
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Done! Partition will be automatically mounted");
                    return 0;
                }
                else
                {
                    Console.WriteLine("Operation canceled by user request");
                    return 1;
                }
            }
            else
            {
                Console.WriteLine("Requested filesystem is not supported!");
                return -1;
            }
        }
    }
}
