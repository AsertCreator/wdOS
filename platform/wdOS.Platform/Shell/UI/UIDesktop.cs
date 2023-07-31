using Cosmos.Core;
using Cosmos.System;
using PrismAPI.Graphics;
using System;
using System.Collections.Generic;
using wdOS.Platform.Core;
using wdOS.Platform.Shell.Widgets;

namespace wdOS.Platform.Shell.UI
{
    public sealed class UIDesktop
    {
        public string Name = "AssociatedDesktop";
		public Color BackgroundColor = WindowManager.BackgroundColor;
		public int DesktopWidth = WindowManager.ScreenWidth;
        public int DesktopHeight = WindowManager.ScreenHeight;
		public int DesktopID = nextdeskid++;
		public object DesktopAuxObject;
		public List<UIWindow> Windows = new();
		public CircularBuffer<KeyEvent> KeyBuffer = new(4);
		public User DesktopOwner;
        private bool initialized = false;
		private static int nextdeskid = 0;

		public void Render()
        {
            if (!initialized)
            {
                MouseManager.ScreenWidth = (uint)DesktopWidth;
				MouseManager.ScreenHeight = (uint)DesktopHeight;
                initialized = true;
            }

            if (KeyboardManager.KeyAvailable) KeyBuffer.Write(KeyboardManager.ReadKey());

            WindowManager.CanvasObject.Clear(BackgroundColor);
            for (int i = 0; i < Windows.Count; i++)
                Windows[i].Render();

            string text = "Frame #" + WindowManager.Framecount.ToString() + ", FPS: " + WindowManager.CanvasObject.GetFPS() +
				"\nMemory usage percentage: " + GCImplementation.GetUsedRAM() / (double)(CPU.GetAmountOfRAM() * 1048576) * 100.0 + "%";

			CommonRenderer.RenderStatic(4, 4, text, WindowManager.CanvasObject, Color.White);

            GCImplementation.Free(text);
        }
        public void OpenWidget(WidgetBase wb, object arg)
        {
            Windows.Add(wb.CreateWindow(arg));
        }
    }
}
