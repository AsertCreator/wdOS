using Cosmos.Core;
using Cosmos.Core.Memory;
using Cosmos.System.FileSystem;
using PrismAPI.Graphics;
using PrismAPI.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using wdOS.Platform.Core;
using wdOS.Platform.Shell.Widgets;
using XSharp.Assembler.x86;

namespace wdOS.Platform.Shell.UI
{
    public abstract class UIWindow
    {
        public static ulong NextZIndex = 0;
        public string WindowTitle;
        public Point Location;
        public Point Size;
        public Color BackgroundColor = WindowManager.GrayColor;
        public UIDesktop AssociatedDesktop;
        public ulong ZIndex = NextZIndex++;
        public UIWindowStyle WindowStyle;
        public WidgetBase AssociatedWidget;
        public Process AssociatedProcess = PlatformManager.KernelProcess;
        public List<UIControl> Controls = new();
        public Canvas ClientAreaBuffer;
        private bool initialized = false;

        public void AddControl(UIControl control)
        {
            Controls.Add(control);
            control.Window = this;
        }
        public void Resize(Point ns)
		{
			if (WindowStyle == UIWindowStyle.FullScreen)
			{
				Size.X = AssociatedDesktop.DesktopWidth;
				Size.Y = AssociatedDesktop.DesktopHeight;
				Location.X = 0;
				Location.Y = 0;
			}
			if (WindowStyle == UIWindowStyle.Transparent) BackgroundColor = Color.Transparent;

			Size = ns;
            ClientAreaBuffer = new Canvas((ushort)GetClientAreaSizeX(), (ushort)GetClientAreaSizeY());
		}
        public void Close()
        {
            var window = this;

			AssociatedDesktop.Windows.Remove(this);
            WindowManager.DestroyWindow(ref window);
        }
        public int GetClientAreaSizeX() => WindowStyle != UIWindowStyle.None ? Size.X : Size.X - 6;
		public int GetClientAreaSizeY() => WindowStyle != UIWindowStyle.None ? Size.Y : Size.Y - 9 - WindowManager.SystemFont.Size;
		public void Render()
        {
            if (!initialized)
			{
				OnCreate();

				Resize(Size);

                AssociatedDesktop = WindowManager.CurrentDesktop;

                initialized = true;
			}

			ClientAreaBuffer.Clear(BackgroundColor);
			for (int i = 0; i < Controls.Count; i++) Controls[i].Render(ClientAreaBuffer);

            OnRender(ClientAreaBuffer);

			switch (WindowStyle)
            {
                default:
                case UIWindowStyle.None:
                    CommonRenderer.RenderRaisedBox(Location.X, Location.Y, Size.X, Size.Y, WindowManager.CanvasObject);
					WindowManager.CanvasObject.DrawImage(Location.X + 3, Location.Y + 6 + WindowManager.SystemFont.Size, ClientAreaBuffer);
                    WindowManager.CanvasObject.DrawString(Location.X + 3, Location.Y + 3, WindowTitle, WindowManager.SystemFont, Color.Black, false);
                    break;
                case UIWindowStyle.FullScreen:
                case UIWindowStyle.Transparent:
                case UIWindowStyle.NoBorder:
					WindowManager.CanvasObject.DrawImage(Location.X, Location.Y, ClientAreaBuffer, true);
                    break;
            }
		}
		protected abstract void OnCreate();
		protected abstract void OnRender(Canvas cnv);
    }
    public enum UIWindowStyle
    {
        None, FullScreen, NoBorder, Transparent
    }
}
