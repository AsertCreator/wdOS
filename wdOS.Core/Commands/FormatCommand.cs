using Cosmos.System.FileSystem.VFS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using wdOS.Core.Shells;

namespace wdOS.Core.Commands
{
    internal class FormatCommand : ConsoleCommand
    {
        internal override string Name => "format";
        internal override string ShortDescription => "format specific drive entirely";
        internal override int Execute(string[] args)
        {
            Console.Clear();
            Console.WriteLine("!!! BEFORE YOU CONTINUE !!!");
            Console.WriteLine("You are going to entirely erase installed drive and format as FAT32");
            Console.WriteLine("You must to have drive backup, because this util can corrupt drive!");
            Console.WriteLine("Also this utility will turn connected drive to system drive");
            Console.WriteLine("\nIf you have read this discalimer, press any key to format...");
            Console.ReadKey(true);
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Clear();
            Console.WriteLine("!!! LAST WARNING !!!");
            Console.WriteLine("THIS UTILITY WILL ERASE ENTIRE DRIVE!");
            Console.WriteLine("Do you really want to format this drive? [y/n]");
            Console.ForegroundColor = ConsoleColor.White;
            if (Console.ReadKey().Key == ConsoleKey.Y)
            {
                Console.Clear();
                Console.WriteLine("Formatting drive as \"FAT32\"...");
                var disk = VFSManager.GetDisks()[0];
                disk.CreatePartition((int)(disk.Host.BlockSize * disk.Host.BlockCount / 1024 / 1024));
                disk.FormatPartition(0, "FAT32");
                disk.MountPartition(0);
                VFSManager.SetFileSystemLabel("0:", Kernel.SystemDriveLabel);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Done! Partition automatically mounted");
                return 0;
            }
            else
            {
                Console.WriteLine("Operation canceled by user request");
                return 1;
            }
        }
    }
}
