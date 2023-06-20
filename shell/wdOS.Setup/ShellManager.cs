using Cosmos.Core;
using Cosmos.System.FileSystem.Listing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using wdOS.Platform.Core;

namespace wdOS.Setup.Platform
{
    internal sealed class ShellManager
    {
        internal void Start()
        {
            var volumes = FileSystemManager.VFS.GetVolumes();
            DirectoryEntry volume = null;
            bool waiting = false;

            // welcome phase
            {
                PrintScreenHeader("Welcome");
                PrintScreenFooter("To start setup, press Super button!");
                waiting = true;
                while (waiting)
                {
                    ConsoleKeyInfo info = Console.ReadKey(true);
                    if (info.Key == ConsoleKey.LeftWindows || info.Key == ConsoleKey.RightWindows)
                        waiting = false;
                }
            }
            // installation drive phase
            {
                PrintScreenHeader("Installation Drive");
                PrintScreenText("Thanks for choosing wdOS! To start installation, you need to configure your " +
                    "system. Please choose your system installation drive: ");
                for (int i = 0; i < volumes.Count; i++)
                {
                    var vol = volumes[i];
                    Console.WriteLine($"\t{i}) \"{FileSystemManager.VFS.GetFileSystemLabel($"{i}:")}\"");
                }
                PrintScreenFooter("Select drive by typing it's zero-based number...");
                waiting = true;
                while (waiting)
                {
                    ConsoleKeyInfo info = Console.ReadKey(true);
                    if (info.Key >= ConsoleKey.D0 && info.Key <= ConsoleKey.D9)
                    {
                        int number = info.Key - ConsoleKey.D0;
                        if (!FileSystemManager.VFS.IsValidDriveId($"{number}:"))
                        {
                            Console.WriteLine("You must specify valid drive!");
                            continue;
                        }
                        volume = volumes[number];
                        waiting = false;
                    }
                    else
                    {
                        Console.WriteLine("You must specify a drive, not a letter!");
                        continue;
                    }
                }
            }
            // installation confirmation phase
            {
                PrintScreenHeader("Confirmation");
                PrintScreenText("Now, you need to acknowledge this: this utility will fully erase everything " +
                    "that's currently stored in that drive. You need to confirm on doing that. " +
                    "Disargeement will abort the installation. Do you want to proceed? [y/n]");
                PrintScreenFooter("Select one of the options...");
                waiting = true;
                while (waiting)
                {
                    ConsoleKeyInfo info = Console.ReadKey(true);
                    if (info.Key == ConsoleKey.Y) waiting = false;
                    else
                    {
                        Console.BackgroundColor = ConsoleColor.Black;
                        Console.Clear();
                        Console.WriteLine("Installation aborted. Please reboot your system.");
                        while (true) { }
                    }
                }
            }
            // setup is not done yet!
            while (true) { }
        }
        internal void PrintScreenHeader(string title)
        {
            Console.BackgroundColor = ConsoleColor.DarkBlue;
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Clear();

            string fulltitle = $" wdOS Setup - {title} ";
            Console.WriteLine('\n' + fulltitle);
            Console.WriteLine(new string('=', fulltitle.Length));
        }
        internal void PrintScreenText(string text)
        {
            Console.BackgroundColor = ConsoleColor.DarkBlue;
            Console.ForegroundColor = ConsoleColor.Gray;

            Console.WriteLine("\n\t" + text);
        }
        internal void PrintScreenFooter(string title)
        {
            Console.BackgroundColor = ConsoleColor.Gray;
            Console.ForegroundColor = ConsoleColor.Black;

            Console.SetCursorPosition(0, 28);
            Console.Write("  " + title + new string(' ', 90 - title.Length - 2));
        }
    }
}
