using Cosmos.Core;
using Cosmos.HAL.Drivers;
using Cosmos.System.Graphics;
using Cosmos.System.Graphics.Fonts;
using System.Collections.Generic;
using System.Drawing;

namespace wdOS.Platform
{
    public static class GraphicsManager
    {
        public static Color WhiteColor = Color.White;
        public static Color BlackColor = Color.Black;
        public static Canvas SystemCanvas;
        public static Mode CurrentMode;
        public static List<Font> Fonts = new();
        public readonly static Rect ScreenSpanRect = new();
        private static bool initialized = false;
        public static void Initialize()
        {
            if (!initialized)
            {
                SystemCanvas = FullScreenCanvas.GetFullScreenCanvas();
                CurrentMode = (Mode)SystemCanvas.Mode;
                Clear(BlackColor);
                SetPixel(1, 1, WhiteColor);
                Fonts.Add(PCScreenFont.Default);
                initialized = true;
            }
        }
        public static void Disable() => SystemCanvas.Disable();
        public static void Swap() => SystemCanvas.Display();
        public static void Clear(Color c)
        {
            for (int y = 0; y < CurrentMode.Height; y++)
                for (int x = 0; x < CurrentMode.Width; x++)
                    SystemCanvas.DrawPoint(c, x, y);
        }
        public static void SetPixel(ushort x, ushort y, Color c)
        {
            SystemCanvas.DrawPoint(c, x, y);
        }
        public static void FillRectangle(Color c, Rect rect)
        {
            for (int y = rect.Top; y < CurrentMode.Height - rect.Bottom; y++)
                for (int x = rect.Left; x < CurrentMode.Width - rect.Right; x++)
                    SystemCanvas.DrawPoint(c, x, y);
        }
        public static void DrawText(Color c, string text, int ix, int iy, Font fnt)
        {
            var x = ix;
            var y = iy;
            for (int i = 0; i < text.Length; i++)
            {
                var ch = text[i];
                var p = fnt.Height * (byte)ch;
                if (ch == '\0') return;
                if (ch == '\n')
                {
                    y += fnt.Height + 2;
                    x = ix;
                }
                else
                {
                    for (int cy = 0; cy < fnt.Height; cy++)
                        for (int cx = 0; cx < fnt.Width; cx++)
                            if (fnt.ConvertByteToBitAddress(fnt.Data[p + cy], cx + 1))
                                SystemCanvas.DrawPoint(c, x + (fnt.Width - cx), y + cy);
                    x += fnt.Width;
                }
            }
        }
    }
    public struct Rect
    {
        public int Top;
        public int Bottom;
        public int Right;
        public int Left;
    }
    public struct Mode
    {
        public int Width;
        public int Height;
        public int ColorDepth;

        public static explicit operator Mode(Cosmos.System.Graphics.Mode m) => new()
        {
            Width = (int)m.Width,
            Height = (int)m.Height,
            ColorDepth = (int)m.ColorDepth
        };
    }
}
