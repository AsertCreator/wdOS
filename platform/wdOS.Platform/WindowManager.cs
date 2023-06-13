using PrismAPI.Graphics;
using PrismAPI.Graphics.Fonts;
using PrismAPI.Hardware.GPU;
using PrismAPI.Hardware.GPU.VMWare;
using System;
using System.Collections.Generic;
using static wdOS.Platform.PlatformManager.BuildConstants;

namespace wdOS.Platform
{
    public static class WindowManager
    {
        public static Font SystemFont;
        public static Display CanvasObject;
        public static ushort Width = 800;
		public static ushort Height = 600;
        public static Point MousePosition = new();
        public static Color BackgroundColor = new Color(198, 198, 198);
        public static ulong Framecount;
		private static bool initialized = false;
        private static Random rng = new();
        public static void Initialize()
        {
            if (!initialized)
			{
				Cosmos.System.MouseManager.ScreenWidth = WindowManager.Width;
				Cosmos.System.MouseManager.ScreenHeight = WindowManager.Height;

				SystemFont = Font.Fallback;
				CanvasObject = Display.GetDisplay(Width, Height);
                initialized = true;
            }
        }
        public static void Start()
        {
            while (true)
			{
				MousePosition.X = (int)Cosmos.System.MouseManager.X;
				MousePosition.Y = (int)Cosmos.System.MouseManager.Y;

                CanvasObject.Clear(BackgroundColor);

				RenderButton("OK", 10, 10 + 28 * 0, 150, 23);
				RenderButton("Cancel", 10, 10 + 28 * 1, 150, 23);
				RenderButton("Apply", 10, 10 + 28 * 2, 150, 23);

				RenderBuildString();

				RenderCursor();

				CanvasObject.Update();
				Framecount++;
			}
		}
        public static void RenderBuildString()
        {
            ushort x;
            string text;
            int count = 2;

            text = "wdOS Platform, version: " + VersionMajor + "." + VersionMinor + "." + VersionPatch;
            x = SystemFont.MeasureString(text);
            CanvasObject.DrawString(Width - x - 5, Height - count * SystemFont.Size - 5, text, SystemFont, Color.White);
            count--;

			text = "Built on Cosmos Project. Uses PrismAPI. Rng: " + rng.Next();
			x = SystemFont.MeasureString(text);
			CanvasObject.DrawString(Width - x - 5, Height - count * SystemFont.Size - 5, text, SystemFont, Color.White);
			count--;

		}
        public static void RenderCursor()
        {
            CanvasObject.DrawFilledRectangle(MousePosition.X, MousePosition.Y, 5, 5, 0, Color.White);
			CanvasObject.DrawFilledRectangle(MousePosition.X + 1, MousePosition.Y + 1, 3, 3, 0, Color.Black);
		}
        public static bool RenderButton(string text, int x, int y, int width, int height)
        {
            CanvasObject.DrawLine(x, y, x + width + 1, y, Color.White);
			CanvasObject.DrawLine(x, y + 1, x, y + height + 1, Color.White);
			CanvasObject.DrawLine(x + width, y + height, x + width, y, Color.Black);
			CanvasObject.DrawLine(x + width - 1, y + height, x, y + height, Color.Black);

			CanvasObject.DrawLine(x + 1, y + 1, x + width, y + 1, new Color(222, 222, 222));
			CanvasObject.DrawLine(x + 1, y + 2, x + 1, y + height, new Color(222, 222, 222));
			CanvasObject.DrawLine(x + width - 1, y + height - 1, x + 1, y + height - 1, Color.LightGray);
			CanvasObject.DrawLine(x + width - 1, y + height - 1, x + width - 1, y + 1, Color.LightGray);

			CanvasObject.DrawString(x + width / 2, y + height / 2 - SystemFont.Size / 2, text, SystemFont, Color.Black, true);

			return false;
        }
	}
	public struct Point
    {
        public int X;
        public int Y;
    }
}
