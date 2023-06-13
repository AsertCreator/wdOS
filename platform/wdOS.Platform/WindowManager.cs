using Cosmos.Core;
using Cosmos.HAL.Drivers;
using Cosmos.System.Graphics.Fonts;
using System.Collections.Generic;
using System.Drawing;
using PrismAPI.Graphics;

namespace wdOS.Platform
{
    public static class WindowManager
    {
        public static Canvas CanvasObject;
        public static ushort Width = 800;
		public static ushort Height = 600;
        public static Point MousePosition;
        public static ulong Framecount;
		private static bool initialized = false;
        public static void Initialize()
        {
            if (!initialized)
            {
                CanvasObject = new(Width, Height);
                CanvasObject.Clear();
                initialized = true;
            }
        }
        public static void RenderFrame()
        {
            
        }
        public static bool RenderButton(string text, int x, int y)
        {

        }
    }
}
