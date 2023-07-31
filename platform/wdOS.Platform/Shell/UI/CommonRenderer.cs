using Cosmos.System;
using PrismAPI.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wdOS.Platform.Shell.UI
{
    public static class CommonRenderer
    {
        public static void RenderStatic(int x, int y, string text, Canvas cnv, Color c)
        {
            cnv.DrawString(x, y, text, WindowManager.SystemFont, c, false);
        }
        public static void RenderRaisedBox(int x, int y, int width, int height, Canvas cnv, Color c = default)
        {
            if (c == default) c = WindowManager.GrayColor;

            cnv.DrawFilledRectangle(x, y, (ushort)width, (ushort)height, 0, c);

			cnv.DrawLine(x, y, x + width - 1, y, Color.White);
			cnv.DrawLine(x, y + 1, x, y + height - 1, Color.White);
			cnv.DrawLine(x + width, y + height, x + width, y, Color.Black);
			cnv.DrawLine(x + width - 1, y + height, x, y + height, Color.Black);

			cnv.DrawLine(x + 1, y + 1, x + width - 1, y + 1, WindowManager.NearWhiteColor);
			cnv.DrawLine(x + 1, y + 2, x + 1, y + height - 1, WindowManager.NearWhiteColor);
			cnv.DrawLine(x + width - 1, y + height - 1, x + 1, y + height - 1, WindowManager.DarkColor);
			cnv.DrawLine(x + width - 1, y + height - 1, x + width - 1, y + 1, WindowManager.DarkColor);
		}
        public static void RenderSunkenBox(int x, int y, int width, int height, Canvas cnv, Color c = default)
        {
            if (c == default) c = WindowManager.GrayColor;

            cnv.DrawFilledRectangle(x, y, (ushort)width, (ushort)height, 0, c);

			cnv.DrawLine(x, y, x + width - 1, y, WindowManager.DarkColor);
			cnv.DrawLine(x, y + 1, x, y + height - 1, WindowManager.DarkColor);
			cnv.DrawLine(x + width, y + height, x + width, y, Color.White);
			cnv.DrawLine(x + width - 1, y + height, x, y + height, Color.White);

			cnv.DrawLine(x + 1, y + 1, x + width - 1, y + 1, Color.Black);
			cnv.DrawLine(x + 1, y + 2, x + 1, y + height - 1, Color.Black);
			cnv.DrawLine(x + width - 1, y + height - 1, x + 1, y + height - 1, WindowManager.NearWhiteColor);
			cnv.DrawLine(x + width - 1, y + height - 1, x + width - 1, y + 1, WindowManager.NearWhiteColor);
		}
        public static bool RenderButton(string text, int x, int y, int width, int height, Canvas cnv, UIDesktop desk, bool center = true, ConsoleKeyEx ex = default)
		{
			if (ex != default)
			{
				var ke = desk.KeyBuffer.Peek();

				if (ke.Key == ex) return true;
			}

            var pressed = Collide((int)MouseManager.X, (int)MouseManager.Y, x, y, width, height) && MouseManager.MouseState == MouseState.Left;
			
            if (pressed) RenderSunkenBox(x, y, width, height, cnv);
			else RenderRaisedBox(x, y, width, height, cnv);

			if (center) cnv.DrawString(x + width / 2, y + height / 2, text, WindowManager.SystemFont, Color.Black, center);
			else cnv.DrawString(x + 3, y + height / 2, text, WindowManager.SystemFont, Color.Black, center);

            return pressed;
        }
		public static void RenderTooltip(string tip, int x, int y, Canvas cnv)
		{
			int size = WindowManager.SystemFont.MeasureString(tip);

			cnv.DrawRectangle(x, y, (ushort)(size + 6), (ushort)(WindowManager.SystemFont.Size + 6), 0, Color.Black);
			cnv.DrawFilledRectangle(x + 1, y + 1, (ushort)(size + 4), (ushort)(WindowManager.SystemFont.Size + 4), 0, Color.LightYellow);
			cnv.DrawString(x + 3, y + 3, tip, WindowManager.SystemFont, Color.Black);
		}
        public static bool Collide(int mx, int my, int bx, int by, int bw, int bh) => 
            mx >= bx && mx <= bx + bw && my >= by && my <= by + bh;
    }
}
