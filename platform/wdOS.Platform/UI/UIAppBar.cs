using Cosmos.System;
using PrismAPI.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace wdOS.Platform.UI
{
	public sealed class UIAppBar
	{
		public int AppBarWidth;
		public int AppBarHeight;
		public Dictionary<string, Action> MenuActions = new();
		private bool initialized = false;
		private bool showmenu = false;
		public void Render(Canvas cnv)
		{
			var desk = WindowManager.DesktopList[WindowManager.CurrentDesktopIndex];
			var y = WindowManager.CanvasObject.Height - AppBarHeight + 5;

			if (!initialized)
            {
				AppBarHeight = 35;
				AppBarWidth = WindowManager.CanvasObject.Width + 10;
				MenuActions["Shutdown PC"] = () => PlatformManager.ShutdownSystem(ShutdownType.SoftShutdown);
				MenuActions["Restart PC"] = () => PlatformManager.ShutdownSystem(ShutdownType.SoftRestart);
				MenuActions["Open up Test Window"] = () => 
					desk.Windows.Add(new()
					{
						Location = new() { X = 120, Y = 120 },
						Size = new() { X = 300, Y = 300 },
						Text = "Hello World!"
					});
				MenuActions["Close"] = () => showmenu = false;
				initialized = true;
            }

			CommonRenderer.RenderBox(-5, y, AppBarWidth, AppBarHeight, cnv);
			if (CommonRenderer.RenderButton("Menu", 5, y + 3, 50, 23, cnv, false, ConsoleKeyEx.LWin)) showmenu = !showmenu;
			if (showmenu)
			{
				for (int i = 0; i < MenuActions.Count; i++)
				{
					var kvp = MenuActions.ElementAt(i);
					if (CommonRenderer.RenderButton(kvp.Key, 0, y - 23 * (MenuActions.Count - i), 200, 23, cnv, false))
						kvp.Value();
				}
			}

			for (int i = 0; i < desk.Windows.Count; i++)
			{
				var wnd = desk.Windows[i];
				CommonRenderer.RenderButton(wnd.Text, 55 + 155 * i, y + 3, 150, 23, cnv, false);
			}
		}
	}
}
