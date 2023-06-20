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
                KeyBuffer = new(4);
                initialized = true;
            }

            if (KeyboardManager.KeyAvailable) KeyBuffer.Write(KeyboardManager.ReadKey());

            WindowManager.CanvasObject.Clear(BackgroundColor);
            for (int i = 0; i < Widgets.Count; i++)
                Widgets[i].Render();

            AppBar.Render(WindowManager.CanvasObject);

            CommonRenderer.RenderStatic(20, 20, WindowManager.Framecount.ToString(), WindowManager.CanvasObject, Color.White);
        }
    }
}
