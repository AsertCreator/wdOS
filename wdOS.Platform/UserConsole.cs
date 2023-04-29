using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wdOS.Platform
{
    // only for user programs
    internal unsafe static class UserConsole
    {
        internal static uint GetCursorX() => (uint)Console.CursorLeft;
        internal static uint GetCursorY() => (uint)Console.CursorTop;
        internal static uint GetConsoleForeground() => (uint)Console.ForegroundColor;
        internal static uint GetConsoleBackground() => (uint)Console.BackgroundColor;
        internal static void SetConsoleForeground(uint val) => Console.ForegroundColor = (ConsoleColor)val;
        internal static void SetConsoleBackground(uint val) => Console.BackgroundColor = (ConsoleColor)val;
        internal static void SetCursorX(uint val) => Console.SetCursorPosition((int)val, Console.CursorTop);
        internal static void SetCursorY(uint val) => Console.SetCursorPosition(Console.CursorLeft, (int)val);
        internal static void Write(char* buffer)
        {
            for (int i = 0; buffer[i] != 0; i++)
                Console.WriteLine(buffer[i]);
        }
        internal static uint Read(char* buffer)
        {
            string str = Console.ReadLine();
            for (int i = 0; i < str.Length; i++)
                buffer[i] = str[i];
            buffer[str.Length] = '\0';
            return (uint)str.Length;
        }
    }
}
