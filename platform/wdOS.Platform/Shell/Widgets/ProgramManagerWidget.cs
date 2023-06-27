using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using wdOS.Platform.Core;
using wdOS.Platform.Shell.UI;

namespace wdOS.Platform.Shell.Widgets
{
	public class ProgramManagerWidget : WidgetBase
	{
		public const uint ID = 101;

		public override int InitialWidth => 400;
		public override int InitialHeight => 300;
		public override bool IsHidden => true;
		public override bool IsSystem => true;
		public override uint WidgetID => ID;

		public ProgramManagerWidget()
		{
			Name = "Program Manager";
			Description = Name;
			Version = PlatformManager.GetPlatformVersion();
		}

		public override void SetupUIWindow(UIWindow uw, object arg)
		{
			uw.Location = new Point(
				WindowManager.ScreenWidth / 2 - InitialWidth / 2,
				WindowManager.ScreenHeight / 2 - InitialWidth / 2);
			uw.AddControl(new UIButton()
			{
				Location = new Point(5, InitialWidth - 23 - 5),
				Size = new Point(InitialWidth - 10, 23),
				Text = "Open Calculator"
			});
		}
	}
}
