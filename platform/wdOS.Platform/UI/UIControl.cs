using PrismAPI.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wdOS.Platform.UI
{
	public abstract class UIControl
	{
		public UIWindow Window;
		public string Text;
		public Point Location;
		public Point Size;

		public abstract void Render();
	}
}
