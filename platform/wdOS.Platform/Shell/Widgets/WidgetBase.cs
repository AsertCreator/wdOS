using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using wdOS.Platform.Shell.UI;

namespace wdOS.Platform.Shell.Widgets
{
	public abstract class WidgetBase
	{
		public static List<WidgetBase> RegisteredWidgets = new();
		public string Name;
		public string Description;
		public string Version;
		public UIWidget AttachedUIWidget;
		public int InitialWidth;
		public int InitialHeight;
	}
}
