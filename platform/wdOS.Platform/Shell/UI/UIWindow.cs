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
    public sealed class UIWindow
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
            Size = ns;
            switch (WindowStyle)
            {
                default:
                case UIWindowStyle.None:
                    ClientAreaBuffer = new Canvas((ushort)(Size.X - 6), (ushort)(Size.Y - 9 - WindowManager.SystemFont.Size));
                    break;
                case UIWindowStyle.FullScreen:
                case UIWindowStyle.NoBorder:
                case UIWindowStyle.Transparent:
                    ClientAreaBuffer = new Canvas((ushort)Size.X, (ushort)Size.Y);
                    break;
            }
        }
        public unsafe void Close()
        {
            var widget = this;

			AssociatedDesktop.Widgets.Remove(this);
            WindowManager.DestroyWidget(ref widget);
        }
        public void Render()
        {
            if (!initialized)
            {
                Resize(Size);

                AssociatedDesktop = WindowManager.CurrentDesktop;

                if (WindowStyle == UIWindowStyle.FullScreen)
                {
                    Size.X = AssociatedDesktop.DesktopWidth;
                    Size.Y = AssociatedDesktop.DesktopHeight;
                    Location.X = 0;
                    Location.Y = 0;
                }
                if (WindowStyle == UIWindowStyle.Transparent) BackgroundColor = Color.Transparent;

                initialized = true;
            }
            switch (WindowStyle)
            {
                default:
                case UIWindowStyle.None:
                    CommonRenderer.RenderRaisedBox(Location.X, Location.Y, Size.X, Size.Y, WindowManager.CanvasObject);

                    ClientAreaBuffer.DrawFilledRectangle(0, 0, (ushort)Size.X, (ushort)Size.Y, 0, BackgroundColor);
					for (int i = 0; i < Controls.Count; i++) Controls[i].Render(ClientAreaBuffer);

					WindowManager.CanvasObject.DrawImage(Location.X + 3, Location.Y + 6 + WindowManager.SystemFont.Size, ClientAreaBuffer);
                    WindowManager.CanvasObject.DrawString(Location.X + 3, Location.Y + 3, WindowTitle, WindowManager.SystemFont, Color.Black, false);
                    break;
                case UIWindowStyle.FullScreen:
                case UIWindowStyle.Transparent:
                case UIWindowStyle.NoBorder:
                    ClientAreaBuffer.DrawFilledRectangle(0, 0, (ushort)Size.X, (ushort)Size.Y, 0, BackgroundColor);
					for (int i = 0; i < Controls.Count; i++) Controls[i].Render(ClientAreaBuffer);

					WindowManager.CanvasObject.DrawImage(Location.X, Location.Y, ClientAreaBuffer, true);
                    break;
            }
        }
    }
    public enum UIWindowStyle
    {
        None, FullScreen, NoBorder, Transparent
    }
}
