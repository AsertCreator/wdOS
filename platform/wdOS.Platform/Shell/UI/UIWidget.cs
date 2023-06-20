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
using XSharp.Assembler.x86;

namespace wdOS.Platform.Shell.UI
{
    public sealed class UIWidget
    {
        public static ulong NextZIndex = 0;
        public string Text;
        public Point Location;
        public Point Size;
        public Color BackgroundColor = Color.White;
        public Process Process = PlatformManager.KernelProcess;
        public UIDesktop Desktop = null;
        public ulong ZIndex = NextZIndex++;
        public UIWindowStyle Style;
        public List<UIControl> Controls = new();
        public Canvas DrawBuffer;
        private bool initialized = false;
        ~UIWidget()
        {
            GCImplementation.Free(DrawBuffer);
        }
        public void AddControl(UIControl control)
        {
            Controls.Add(control);
            control.Window = this;
        }
        public void Resize(Point ns)
        {
            Size = ns;
            switch (Style)
            {
                default:
                case UIWindowStyle.None:
                    DrawBuffer = new Canvas((ushort)(Size.X - 6), (ushort)(Size.Y - 9 - WindowManager.SystemFont.Size));
                    break;
                case UIWindowStyle.FullScreen:
                    DrawBuffer = new Canvas((ushort)Size.X, (ushort)Size.Y);
                    break;
                case UIWindowStyle.NoBorder:
                    DrawBuffer = new Canvas((ushort)Size.X, (ushort)Size.Y);
                    break;
                case UIWindowStyle.Transparent:
                    DrawBuffer = new Canvas((ushort)Size.X, (ushort)Size.Y);
                    break;
            }
        }
        public unsafe void Close()
        {
            Desktop.Widgets.Remove(this);
            Heap.Free(DrawBuffer.Internal);
            GCImplementation.Free(DrawBuffer);
        }
        public void Render()
        {
            if (!initialized)
            {
                Resize(Size);

                Desktop = WindowManager.DesktopList[WindowManager.CurrentDesktopIndex];
                if (Style == UIWindowStyle.FullScreen)
                {
                    Size.X = Desktop.DesktopWidth;
                    Size.Y = Desktop.DesktopHeight;
                    Location.X = 0;
                    Location.Y = 0;
                }
                if (Style == UIWindowStyle.Transparent) BackgroundColor = Color.Transparent;

                initialized = true;
            }
            switch (Style)
            {
                default:
                case UIWindowStyle.None:
                    CommonRenderer.RenderRaisedBox(Location.X, Location.Y, Size.X, Size.Y, WindowManager.CanvasObject);
                    DrawBuffer.DrawFilledRectangle(0, 0, (ushort)Size.X, (ushort)Size.Y, 0, BackgroundColor);
					for (int i = 0; i < Controls.Count; i++) Controls[i].Render(DrawBuffer);
					WindowManager.CanvasObject.DrawImage(Location.X + 3, Location.Y + 6 + WindowManager.SystemFont.Size, DrawBuffer);
                    WindowManager.CanvasObject.DrawString(Location.X + 3, Location.Y + 3, Text, WindowManager.SystemFont, Color.Black, false);
                    break;
                case UIWindowStyle.FullScreen:
                case UIWindowStyle.Transparent:
                case UIWindowStyle.NoBorder:
                    DrawBuffer.DrawFilledRectangle(0, 0, (ushort)Size.X, (ushort)Size.Y, 0, BackgroundColor);
					for (int i = 0; i < Controls.Count; i++) Controls[i].Render(DrawBuffer);
					WindowManager.CanvasObject.DrawImage(Location.X, Location.Y, DrawBuffer, true);
                    break;
            }
        }
    }
    public enum UIWindowStyle
    {
        None, FullScreen, NoBorder, Transparent
    }
}
