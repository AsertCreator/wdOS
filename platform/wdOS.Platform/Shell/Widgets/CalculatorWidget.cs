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
		public const uint ID = 200;

		public override int InitialWidth => 400;
		public override int InitialHeight => 400;
		public override bool IsHidden => false;
		public override bool IsSystem => true;
		public override uint WidgetID => ID;

		public CalculatorWidget()
		{
			Name = "Calculator";
			Description = "Calculator Widget";
			Version = PlatformManager.GetPlatformVersion();
		}

		public override void SetupUIWindow(UIWindow uw, object arg)
		{
			
		}
	}
}
