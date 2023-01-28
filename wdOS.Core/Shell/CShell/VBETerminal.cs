using Cosmos.HAL;
using Cosmos.System.Graphics;
using Cosmos.System.Graphics.Fonts;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace wdOS.Core.Shell.CShell
{
    internal class VBETerminal : TextScreenBase
    {
        public override byte this[int x, int y] { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override ushort Cols { get; set; }
        public override ushort Rows { get; set; }
        public Point CursorPosition;
        public Canvas ScreenCanvas;
        public Font ScreenFont;
        public ConsoleColor ConsoleForeground;
        public ConsoleColor ConsoleBackground;
        public Dictionary<ConsoleColor, Color> ColorPallete;
        public VBECharacter[] Buffer;
        public VBETerminal(ushort cols, ushort rows) 
        {
            Cols = cols;
            Rows = rows;
            Buffer = new VBECharacter[cols * rows];
            ScreenFont = Foundation.Kernel.SystemSettings.TerminalFonts[
                Foundation.Kernel.SystemSettings.SystemTerminalFont];
            ConsoleForeground = Console.ForegroundColor;
            ConsoleBackground = Console.BackgroundColor;
            ColorPallete = new()
            {
                [ConsoleColor.Black] = Color.Black,
                [ConsoleColor.DarkBlue] = Color.DarkBlue,
                [ConsoleColor.DarkGreen] = Color.DarkGreen,
                [ConsoleColor.DarkCyan] = Color.DarkCyan,
                [ConsoleColor.DarkRed] = Color.DarkRed,
                [ConsoleColor.DarkMagenta] = Color.DarkMagenta,
                [ConsoleColor.DarkYellow] = Color.Yellow,
                [ConsoleColor.Gray] = Color.Gray,
                [ConsoleColor.DarkGray] = Color.DarkGray,
                [ConsoleColor.Blue] = Color.Blue,
                [ConsoleColor.Green] = Color.Green,
                [ConsoleColor.Cyan] = Color.Cyan,
                [ConsoleColor.Red] = Color.Red,
                [ConsoleColor.Magenta] = Color.Magenta,
                [ConsoleColor.Yellow] = Color.LightYellow,
                [ConsoleColor.White] = Color.White
            };
            Clear();
            ScreenCanvas = FullScreenCanvas.GetFullScreenCanvas(
                new Mode(Cols * ScreenFont.Width, Rows * ScreenFont.Height, ColorDepth.ColorDepth32));
            Render();
        }
        public void Render()
        {
            int fontsizex = ScreenFont.Width;
            int fontsizey = ScreenFont.Height;
            for (int y = 0; y < Rows; y++)
            {
                for (int x = 0; x < Cols; x++)
                {
                    var chr = Buffer[y * Cols + x];
                    ScreenCanvas.DrawFilledRectangle(
                        ColorPallete[chr.back], x * fontsizex, y * fontsizey, fontsizex, fontsizey);
                    ScreenCanvas.DrawChar((char)(byte)chr.chr, ScreenFont, ColorPallete[chr.fore], x * fontsizex, y * fontsizey);
                }
            }
            ScreenCanvas.Display();
        }
        public override void Clear()
        {
            for (int y = 0; y < Rows; y++)
            {
                for (int x = 0; x < Cols; x++)
                {
                    int index = y * Cols + x;
                    Buffer[index].fore = ConsoleForeground;
                    Buffer[index].back = ConsoleBackground;
                    Buffer[index].chr = ' ';
                }
            }
        }
        public override byte GetColor() => 0;
        public override int GetCursorSize() => 1;
        public override bool GetCursorVisible() => true;
        public override void ScrollUp() { }
        public override void SetColors(ConsoleColor aForeground, ConsoleColor aBackground)
        {
            ConsoleForeground = aForeground;
            ConsoleBackground = aBackground;
        }
        public override void SetCursorPos(int x, int y)
        {
            CursorPosition.X = x;
            CursorPosition.Y = y;
        }
        public override void SetCursorSize(int value) { }
        public override void SetCursorVisible(bool value) { }
    }
    internal struct VBECharacter
    {
        public ushort chr;
        public ConsoleColor fore;
        public ConsoleColor back;
    }
}
