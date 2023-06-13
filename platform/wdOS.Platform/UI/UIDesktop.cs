using PrismAPI.Graphics;
using System.Collections.Generic;

namespace wdOS.Platform.UI
{
	public sealed class UIDesktop
	{
		public string Name = "Desktop";
		public int DesktopWidth = WindowManager.ScreenWidth;
		public int DesktopHeight = WindowManager.ScreenHeight;
		public List<UIWindow> Windows = new();
		public UIVersionText VersionText = new();
		public UIAppBar AppBar = new();
		public Color BackgroundColor = WindowManager.BackgroundColor;
		public void Render()
		{
			WindowManager.CanvasObject.Clear(BackgroundColor);

			// left for testing commonrenderer
			CommonRenderer.RenderButton("OK", 10, 10 + 28 * 0, 100, 23);
			CommonRenderer.RenderButton("Cancel", 10, 10 + 28 * 1, 100, 23);
			CommonRenderer.RenderButton("Apply", 10, 10 + 28 * 2, 100, 23);

			for (int i = 0; i < Windows.Count; i++)
				Windows[i].Render();

			AppBar.Render();
			VersionText.BottomMargin = AppBar.AppBarHeight - 5;
			VersionText.Render();
		}
	}
}
