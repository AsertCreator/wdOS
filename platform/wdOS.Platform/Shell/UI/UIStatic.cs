using PrismAPI.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wdOS.Platform.Shell.UI
{
	public class UIStatic : UIControl
	{
		public override void Render(Canvas cnv)
		{
			cnv.DrawString(Location.X, Location.Y, Text, WindowManager.SystemFont, Foreground);
		}
	}
}
