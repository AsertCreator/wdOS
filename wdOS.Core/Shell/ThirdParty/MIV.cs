using System;
using System.IO;

namespace wdOS.Core.Shell.ThirdParty
{
    // Originally written by Bartashevich (https://github.com/bartashevich)
    // Modified by CaveSponge (possibly, https://github.com/Arawn-Davies)
    // And finally modifed by AsertCreator (https://github.com/AsertCreator) to make it a bit more readble and compatible with wdOS
    internal static class MIV
    {
        public static string CurrentFile;
        public static void PrintStartScreen()
        {
            Console.Clear();
            Console.WriteLine("~");
            Console.WriteLine("~");
            Console.WriteLine("~");
            Console.WriteLine("~");
            Console.WriteLine("~");
            Console.WriteLine("~");
            Console.WriteLine("~");
            Console.WriteLine("~                             MIV - MInimalistic Vi");
            Console.WriteLine("~");
            Console.WriteLine("~                              version 1.2 MODIFIED");
            Console.WriteLine("~                             by Denis Bartashevich");
            Console.WriteLine("~                  Minor additions by CaveSponge and AsertCreator");
            Console.WriteLine("~                    MIV is open source and freely distributable");
            Console.WriteLine("~");
            Console.WriteLine("~                     type :help<Enter>          for information");
            Console.WriteLine("~                     type :q<Enter>             to exit");
            Console.WriteLine("~                     type :wq<Enter>            save to file and exit");
            Console.WriteLine("~                     press i                    to write");
            Console.WriteLine("~");
            Console.WriteLine("~");
            Console.WriteLine("~");
            Console.WriteLine("~");
            Console.WriteLine("~");
            Console.WriteLine("~");
            Console.Write("~");
        }
        public static string StringCopy(string value)
        {
            string newstring = string.Empty;
            for (int i = 0; i < value.Length - 1; i++) newstring += value[i];
            return newstring;
        }
        public static void PrintScreen(char[] chars, int pos, string infoBar, bool editMode)
        {
            int countNewLine = 0;
            int countChars = 0;
            WaitFor(10000000);
            Console.Clear();
            for (int i = 0; i < pos; i++)
            {
                if (chars[i] == '\n')
                {
                    Console.WriteLine("");
                    countNewLine++;
                    countChars = 0;
                }
                else
                {
                    Console.Write(chars[i]);
                    countChars++;
                    if (countChars % 80 == 79) countNewLine++;
                }
            }
            Console.Write("/");
            for (int i = 0; i < 23 - countNewLine; i++)
            {
                Console.WriteLine("");
                Console.Write("~");
            }

            //PRINT INSTRUCTION
            Console.WriteLine();
            for (int i = 0; i < 72; i++)
            {
                if (i < infoBar.Length) Console.Write(infoBar[i]);
                else Console.Write(" ");
            }
            if (editMode) Console.Write(countNewLine + 1 + "," + countChars);
        }
        public static string MIVMain(string start)
        {
            bool editMode = false;
            int pos = 0;
            char[] chars = new char[2000];
            string infoBar = string.Empty;

            if (start == null) PrintStartScreen();
            else
            {
                pos = start.Length;
                for (int i = 0; i < start.Length; i++) chars[i] = start[i];
                PrintScreen(chars, pos, infoBar, editMode);
            }
            ConsoleKeyInfo keyInfo;
            do
            {
                keyInfo = Console.ReadKey(true);
                if (IsForbiddenKey(keyInfo.Key)) continue;
                else if (!editMode && keyInfo.KeyChar == ':')
                {
                    infoBar = ":";
                    PrintScreen(chars, pos, infoBar, editMode);
                    do
                    {
                        keyInfo = Console.ReadKey(true);
                        if (keyInfo.Key == ConsoleKey.Enter)
                        {
                            if (infoBar == ":wq")
                            {
                                string returnstring = string.Empty;
                                for (int i = 0; i < pos; i++) returnstring += chars[i];
                                return returnstring;
                            }
                            else if (infoBar == ":q") return null;
                            else if (infoBar == ":help") { PrintStartScreen(); break; }
                            else
                            {
                                infoBar = "ERROR: No such command";
                                PrintScreen(chars, pos, infoBar, editMode);
                                break;
                            }
                        }
                        else if (keyInfo.Key == ConsoleKey.Backspace)
                        {
                            infoBar = StringCopy(infoBar);
                            PrintScreen(chars, pos, infoBar, editMode);
                        }
                        else if (keyInfo.KeyChar == 'q') infoBar += "q";
                        else if (keyInfo.KeyChar == ':') infoBar += ":";
                        else if (keyInfo.KeyChar == 'w') infoBar += "w";
                        else if (keyInfo.KeyChar == 'h') infoBar += "h";
                        else if (keyInfo.KeyChar == 'e') infoBar += "e";
                        else if (keyInfo.KeyChar == 'l') infoBar += "l";
                        else if (keyInfo.KeyChar == 'p') infoBar += "p";
                        else continue;
                        PrintScreen(chars, pos, infoBar, editMode);
                    } while (keyInfo.Key != ConsoleKey.Escape);
                }
                else if (keyInfo.Key == ConsoleKey.Escape)
                {
                    editMode = false;
                    infoBar = string.Empty;
                    PrintScreen(chars, pos, infoBar, editMode);
                    continue;
                }
                else if (keyInfo.Key == ConsoleKey.I && !editMode)
                {
                    editMode = true;
                    infoBar = "-- INSERT --";
                    PrintScreen(chars, pos, infoBar, editMode);
                    continue;
                }
                else if (keyInfo.Key == ConsoleKey.Enter && editMode && pos >= 0)
                {
                    chars[pos++] = '\n';
                    PrintScreen(chars, pos, infoBar, editMode);
                    continue;
                }
                else if (keyInfo.Key == ConsoleKey.Backspace && editMode && pos >= 0)
                {
                    if (pos > 0) pos--;
                    chars[pos] = '\0';
                    PrintScreen(chars, pos, infoBar, editMode);
                    continue;
                }

                if (editMode && pos >= 0)
                {
                    chars[pos++] = keyInfo.KeyChar;
                    PrintScreen(chars, pos, infoBar, editMode);
                }

            } while (true);
        }

        public static bool IsForbiddenKey(ConsoleKey key)
        {
            ConsoleKey[] forbiddenKeys = 
                { ConsoleKey.Print, ConsoleKey.PrintScreen, ConsoleKey.Pause, ConsoleKey.Home, 
                ConsoleKey.PageUp, ConsoleKey.PageDown, ConsoleKey.End, ConsoleKey.NumPad0, 
                ConsoleKey.NumPad1, ConsoleKey.NumPad2, ConsoleKey.NumPad3, ConsoleKey.NumPad4, 
                ConsoleKey.NumPad5, ConsoleKey.NumPad6, ConsoleKey.NumPad7, ConsoleKey.NumPad8, 
                ConsoleKey.NumPad9, ConsoleKey.Insert, ConsoleKey.F1, ConsoleKey.F2, ConsoleKey.F3, 
                ConsoleKey.F4, ConsoleKey.F5, ConsoleKey.F6, ConsoleKey.F7, ConsoleKey.F8, 
                ConsoleKey.F9, ConsoleKey.F10, ConsoleKey.F11, ConsoleKey.F12, ConsoleKey.Add, 
                ConsoleKey.Divide, ConsoleKey.Multiply, ConsoleKey.Subtract, ConsoleKey.LeftWindows, 
                ConsoleKey.RightWindows };
            for (int i = 0; i < forbiddenKeys.Length; i++)
            {
                if (key == forbiddenKeys[i]) return true;
            }
            return false;
        }

        public static void WaitFor(int time)
        {
            for (int i = 0; i < time; i++) ;
        }
        public static void StartMIV()
        {
            Console.Write("Enter file's filename to open/create: ");
            CurrentFile = Console.ReadLine();
            try
            {
                if (File.Exists(@"0:\" + CurrentFile))
                {
                    Console.WriteLine("Found file!");
                }
                else if (!File.Exists(@"0:\" + CurrentFile))
                {
                    Console.WriteLine("Creating file!");
                    File.Create(@"0:\" + CurrentFile);
                }
                Console.Clear();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            string text = string.Empty;
            text = MIVMain(File.ReadAllText(@"0:\" + CurrentFile));

            Console.Clear();

            if (string.IsNullOrEmpty(text.Trim()))
            {
                File.WriteAllText(@"0:\" + CurrentFile, text);
                Console.WriteLine($"File has been saved to \"{CurrentFile}\"");
            }
        }
    }
}