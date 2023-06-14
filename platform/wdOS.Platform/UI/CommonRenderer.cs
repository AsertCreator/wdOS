using Cosmos.System;
using PrismAPI.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wdOS.Platform.UI
{
	public static class CommonRenderer
	{
		public static void RenderStatic(int x, int y, string text, Color c)
		{
			WindowManager.CanvasObject.DrawString(x, y, text, WindowManager.SystemFont, c, false);
		}
		public static void RenderBox(int x, int y, int width, int height, Color c = default)
		{
			if (c == default) c = WindowManager.GrayColor;

			WindowManager.CanvasObject.DrawFilledRectangle(x, y, (ushort)width, (ushort)height, 0, c);

			WindowManager.CanvasObject.DrawLine(x, y, x + width + 1, y, Color.White);
			WindowManager.CanvasObject.DrawLine(x, y + 1, x, y + height + 1, Color.White);
			WindowManager.CanvasObject.DrawLine(x + width, y + height, x + width, y, Color.Black);
			WindowManager.CanvasObject.DrawLine(x + width - 1, y + height, x, y + height, Color.Black);

			WindowManager.CanvasObject.DrawLine(x + 1, y + 1, x + width, y + 1, WindowManager.NearWhiteColor);
			WindowManager.CanvasObject.DrawLine(x + 1, y + 2, x + 1, y + height, WindowManager.NearWhiteColor);
			WindowManager.CanvasObject.DrawLine(x + width - 1, y + height - 1, x + 1, y + height - 1, Color.LightGray);
			WindowManager.CanvasObject.DrawLine(x + width - 1, y + height - 1, x + width - 1, y + 1, Color.LightGray);
		}
		public static bool RenderButton(string text, int x, int y, int width, int height, bool center = true, ConsoleKeyEx ex = default)
		{
			RenderBox(x, y, width, height);
			if (center)
				WindowManager.CanvasObject.DrawString(x + width / 2, y + height / 2 - WindowManager.SystemFont.Size / 2, text, WindowManager.SystemFont, Color.Black, true);
			else
				WindowManager.CanvasObject.DrawString(x + 3, y + height / 2 - WindowManager.SystemFont.Size / 2, text, WindowManager.SystemFont, Color.Black, false);

			if (ex != default)
			{
				var desk = WindowManager.DesktopList[WindowManager.CurrentDesktopIndex];
				var ke = desk.KeyBuffer.Peek();

				if (ke.Key == ex) return true;
			}
			return false;
		}
	}
}
