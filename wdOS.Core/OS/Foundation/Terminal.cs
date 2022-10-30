using System;

namespace wdOS.Core.OS.Foundation
{
    internal static class Terminal
    {
        internal static WriteHandler TerminalWrite;
        internal static ReadHandler TerminalReadChar;
        internal static ReadArrowHandler TerminalReadArrow;
        internal static ReadLineHandler TerminalReadLine;
        internal static void WriteLine(string text) => Write(text + '\n');
        internal static void Write(string text) => TerminalWrite(text);
        internal static void Write(char text) => TerminalWrite($"{text}");
        internal static string ReadLine() => TerminalReadLine();
        internal static ConsoleKeyInfo ReadKey() => TerminalReadChar();
        internal static Direction ReadArrow() => TerminalReadArrow();
        internal static void SetConsoleHandlers()
        {
            TerminalWrite = x => Console.Write(x);
            TerminalReadChar = () => Console.ReadKey();
            TerminalReadLine = () =>
            {
                // todo: implement virtual terminal
                return "";
            };
        }
    }
    internal delegate void WriteHandler(string text);
    internal delegate ConsoleKeyInfo ReadHandler();
    internal delegate Direction ReadArrowHandler();
    internal delegate string ReadLineHandler();
    internal enum Direction { Up, Down, Right, Left }
}
