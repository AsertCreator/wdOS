using PrismAPI.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XSharp.Assembler.x86;

namespace wdOS.Platform.UI
{
	public sealed class UIWindow
	{
		public static ulong NextZIndex = 0;
		public string Text;
		public Point Location;
		public Point Size;
		public Color BackgroundColor = WindowManager.GrayColor;
		public Process Process = PlatformManager.KernelProcess;
		public ulong ZIndex = NextZIndex++;
		public UIWindowStyle Style;
		public List<UIControl> Controls = new();
		public void AddControl(UIControl control)
		{
			Controls.Add(control);
			control.Window = this;
		}
		public void Render()
		{
			CommonRenderer.RenderBox(Location.X, Location.Y, Size.X, Size.Y);
			WindowManager.CanvasObject.DrawString(Location.X + 3, Location.Y + 3, Text, WindowManager.SystemFont, Color.Black, false);

			for (int i = 0; i < Controls.Count; i++) Controls[i].Render();
		}
	}
	public enum UIWindowStyle
	{
		None, FullScreen, NoBorder, Transparent
	}
}
