using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using wdOS.Platform.Core;
using wdOS.Platform.Shell.UI;

namespace wdOS.Platform.Shell.Widgets
{
	public class CalculatorWidget : WidgetBase
	{
		public CalculatorWidget() 
		{
			Name = "Calculator";
			Description = "Calculator Widget";
			Version = PlatformManager.GetPlatformVersion();
			InitialWidth = 400;
			InitialHeight = 400;
		}
	}
}
