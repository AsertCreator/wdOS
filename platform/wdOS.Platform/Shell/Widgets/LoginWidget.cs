using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using wdOS.Platform.Core;
using wdOS.Platform.Shell.UI;

namespace wdOS.Platform.Shell.Widgets
{
	public class LoginWidget : WidgetBase
	{
		public const uint ID = 100;

		public override int InitialWidth => 400;
		public override int InitialHeight => 300;
		public override bool IsHidden => true;
		public override bool IsSystem => true;
		public override uint WidgetID => ID;

		public LoginWidget()
		{
			Name = "wdOS: Login";
			Description = Name;
			Version = PlatformManager.GetPlatformVersion();
		}

		public override void SetupUIWindow(UIWindow uw, object arg)
		{
			uw.Location = new Point(
				WindowManager.ScreenWidth / 2 - InitialWidth / 2,
				WindowManager.ScreenHeight / 2 - InitialWidth / 2);

			uw.AddControl(new UIStatic() { Location = new Point(5, 5), Text = "Welcome! Select your profile and login." });

			uw.AddControl(new UIButton()
			{
				Location = new Point(80 * 0 + 5, uw.GetClientAreaSizeY() - 23 - 5),
				Size = new Point(75, 23),
				Text = "Login"
			});
			uw.AddControl(new UIButton()
			{
				Location = new Point(80 * 1 + 5, uw.GetClientAreaSizeY() - 23 - 5),
				Size = new Point(75, 23),
				Text = "Shutdown"
			});
			uw.AddControl(new UIButton()
			{
				Location = new Point(80 * 2 + 5, uw.GetClientAreaSizeY() - 23 - 5),
				Size = new Point(75, 23),
				Text = "Restart"
			});
		}
	}
}
