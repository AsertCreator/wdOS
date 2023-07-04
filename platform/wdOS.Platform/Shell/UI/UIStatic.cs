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
			if (Background != Color.Transparent)
				cnv.DrawFilledRectangle(Location.X, Location.Y, (ushort)Size.X, (ushort)Size.Y, 0, Background);
			CommonRenderer.RenderStatic(Location.X, Location.Y, Text, cnv, Foreground);
		}
	}
}
