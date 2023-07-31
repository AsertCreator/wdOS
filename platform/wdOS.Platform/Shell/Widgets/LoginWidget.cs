using PrismAPI.Graphics;
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

		public override bool IsHidden => true;
		public override bool IsSystem => true;
		public override uint WidgetID => ID;

		public LoginWidget()
		{
			Name = "wdOS: Login";
			Description = Name;
			Version = PlatformManager.GetPlatformVersion();
		}

		private class LoginWidgetWindow : UIWindow
		{
			protected override void OnCreate()
			{
				Location = new Point(
					WindowManager.ScreenWidth / 2 - 400 / 2,
					WindowManager.ScreenHeight / 2 - 150 / 2);

				BackgroundColor = WindowManager.GrayColor;

				AddControl(new UIButton()
				{
					Location = new Point(80 * 0 + 5, GetClientAreaSizeY() - 23 - 5),
					Size = new Point(75, 23),
					Text = "Login"
				});
				AddControl(new UIButton()
				{
					Location = new Point(80 * 1 + 5, GetClientAreaSizeY() - 23 - 5),
					Size = new Point(75, 23),
					Text = "Shutdown"
				});
				AddControl(new UIButton()
				{
					Location = new Point(80 * 2 + 5, GetClientAreaSizeY() - 23 - 5),
					Size = new Point(75, 23),
					Text = "Restart"
				});
			}
			protected override void OnRender(Canvas cnv)
			{
				cnv.DrawString(5, 5, "Welcome! Select your profile and login", WindowManager.SystemFont, Color.Black);
			}
		}

		public override UIWindow CreateWindow(object arg) => new LoginWidgetWindow();
	}
}
