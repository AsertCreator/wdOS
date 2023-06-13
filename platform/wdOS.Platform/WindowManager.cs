using PrismAPI.Graphics;
using PrismAPI.Graphics.Fonts;
using PrismAPI.Hardware.GPU;
using System;
using System.Collections.Generic;
using wdOS.Platform.UI;
using static wdOS.Platform.PlatformManager.BuildConstants;

namespace wdOS.Platform
{
    public static class WindowManager
    {
        public static Font SystemFont;
        public static Display CanvasObject;
        public static ushort ScreenWidth = 800;
        public static ushort ScreenHeight = 600;
        public static Color BackgroundColor = Color.ClassicBlue;
        public static Color GrayColor = new Color(198, 198, 198);
        public static Color NearWhiteColor = new Color(222, 222, 222);
		public static List<UIDesktop> DesktopList = new List<UIDesktop>();
        public static int CurrentDesktopIndex = 0;
        public static ulong Framecount;
        private static bool initialized = false;
        public static void Initialize()
        {
            if (!initialized)
            {
                SystemFont = Font.Fallback;
                CanvasObject = Display.GetDisplay(ScreenWidth, ScreenHeight);
                DesktopList.Add(
                    new()
                    {
                        Windows = new()
                        { 
                            new()
                            {
                                Location = new() { X = 120, Y = 120 },
                                Size = new() { X = 300, Y = 300 },
                                Text = "Hello World!"
                            }
                        }
                    }
                );

                initialized = true;
            }
        }
        public static void Start()
        {
            while (true)
            {
                CanvasObject.Clear(BackgroundColor);

                var curdesk = DesktopList[CurrentDesktopIndex];
                curdesk.Render();

                CanvasObject.Update();
                Framecount++;
            }
        }
    }
    public struct Point
    {
        public int X;
        public int Y;
    }
}
