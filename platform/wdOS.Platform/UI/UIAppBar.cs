using PrismAPI.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wdOS.Platform.UI
{
	public sealed class UIAppBar
	{
		public int AppBarWidth;
		public int AppBarHeight;
		private bool initialized = false;
		public void Render()
		{
            if (!initialized)
            {
				AppBarHeight = 35;
				AppBarWidth = WindowManager.CanvasObject.Width + 10;
				initialized = true;
            }
			CommonRenderer.RenderBox(-5, WindowManager.CanvasObject.Height - AppBarHeight + 5, AppBarWidth, AppBarHeight);
		}
	}
}
