using Cosmos.Core;
using Cosmos.System;
using PrismAPI.Graphics;
using System;
using System.Collections.Generic;
using wdOS.Platform.Core;

namespace wdOS.Platform.Shell.UI
{
    public sealed class UIDesktop
    {
        public string Name = "Desktop";
        public int DesktopWidth = WindowManager.ScreenWidth;
        public int DesktopHeight = WindowManager.ScreenHeight;
        public List<UIWidget> Widgets = new();
        public UIAppBar AppBar = new();
        public Color BackgroundColor = WindowManager.BackgroundColor;
        public CircularBuffer<KeyEvent> KeyBuffer;
        public User Owner;
        private bool initialized = false;
        public void Render()
        {
            if (!initialized)
            {
                MouseManager.ScreenWidth = (uint)DesktopWidth;
				MouseManager.ScreenHeight = (uint)DesktopHeight;
				KeyBuffer = new(4);
                initialized = true;
            }

            if (KeyboardManager.KeyAvailable) KeyBuffer.Write(KeyboardManager.ReadKey());

            WindowManager.CanvasObject.Clear(BackgroundColor);
            for (int i = 0; i < Widgets.Count; i++)
                Widgets[i].Render();

            AppBar.Render(WindowManager.CanvasObject);

            string text = "Frame #" + WindowManager.Framecount.ToString() + 
                "\nMemory usage percentage: " + GCImplementation.GetUsedRAM() / (double)(CPU.GetAmountOfRAM() * 1048576) * 100.0 + "%";

			CommonRenderer.RenderStatic(20, 20, text, WindowManager.CanvasObject, Color.White);

            GCImplementation.Free(text);
        }
    }
}
