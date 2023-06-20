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

            cnv.DrawLine(x, y, x + width + 1, y, Color.White);
            cnv.DrawLine(x, y + 1, x, y + height + 1, Color.White);
            cnv.DrawLine(x + width, y + height, x + width, y, Color.Black);
            cnv.DrawLine(x + width - 1, y + height, x, y + height, Color.Black);

            cnv.DrawLine(x + 1, y + 1, x + width, y + 1, WindowManager.NearWhiteColor);
            cnv.DrawLine(x + 1, y + 2, x + 1, y + height, WindowManager.NearWhiteColor);
            cnv.DrawLine(x + width - 1, y + height - 1, x + 1, y + height - 1, Color.LightGray);
            cnv.DrawLine(x + width - 1, y + height - 1, x + width - 1, y + 1, Color.LightGray);
        }
        public static void RenderSunkenBox(int x, int y, int width, int height, Canvas cnv, Color c = default)
        {
            // todo: sunken box

            if (c == default) c = WindowManager.GrayColor;

            cnv.DrawFilledRectangle(x, y, (ushort)width, (ushort)height, 0, c);

            cnv.DrawLine(x, y, x + width + 1, y, Color.White);
            cnv.DrawLine(x, y + 1, x, y + height + 1, Color.White);
            cnv.DrawLine(x + width, y + height, x + width, y, Color.Black);
            cnv.DrawLine(x + width - 1, y + height, x, y + height, Color.Black);

            cnv.DrawLine(x + 1, y + 1, x + width, y + 1, WindowManager.NearWhiteColor);
            cnv.DrawLine(x + 1, y + 2, x + 1, y + height, WindowManager.NearWhiteColor);
            cnv.DrawLine(x + width - 1, y + height - 1, x + 1, y + height - 1, Color.LightGray);
            cnv.DrawLine(x + width - 1, y + height - 1, x + width - 1, y + 1, Color.LightGray);
        }
        public static bool RenderButton(string text, int x, int y, int width, int height, Canvas cnv, bool center = true, ConsoleKeyEx ex = default)
        {
            RenderRaisedBox(x, y, width, height, cnv);
            if (center)
                cnv.DrawString(x + width / 2, y + height / 2 - WindowManager.SystemFont.Size / 2, text, WindowManager.SystemFont, Color.Black, true);
            else
                cnv.DrawString(x + 3, y + height / 2 - WindowManager.SystemFont.Size / 2, text, WindowManager.SystemFont, Color.Black, false);

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
