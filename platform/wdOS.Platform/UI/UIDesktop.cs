using Cosmos.System;
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
		public CircularBuffer<KeyEvent> KeyBuffer;
		private bool initialized = false;
		public void Render()
		{
			if (!initialized)
			{
				KeyBuffer = new(4);
				initialized = true;
			}

			if (KeyboardManager.KeyAvailable)
			{
				KeyBuffer.Write(KeyboardManager.ReadKey());
			}

			WindowManager.CanvasObject.Clear(BackgroundColor);

			for (int i = 0; i < Windows.Count; i++)
				Windows[i].Render();

			AppBar.Render(WindowManager.CanvasObject);
			VersionText.BottomMargin = AppBar.AppBarHeight - 5;
			VersionText.Render(WindowManager.CanvasObject);
		}
	}
}
