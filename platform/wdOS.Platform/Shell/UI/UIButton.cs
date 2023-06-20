using PrismAPI.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wdOS.Platform.Shell.UI
{
    public class UIButton : UIControl
    {
        public override void Render(Canvas cnv)
        {
            // wow i spent 69 hours writing this
            CommonRenderer.RenderButton(Text, Location.X, Location.Y, Size.X, Size.Y, cnv);
        }
    }
}
