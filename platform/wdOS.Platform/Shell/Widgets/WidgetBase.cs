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
		public string Name;
		public string Description;
		public string Version;
		public abstract bool IsHidden { get; }
		public abstract bool IsSystem { get; }
		public abstract uint WidgetID { get; }
		public abstract UIWindow CreateWindow(object arg);
	}}
