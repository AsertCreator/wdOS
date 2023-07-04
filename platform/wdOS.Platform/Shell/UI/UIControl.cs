using PrismAPI.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wdOS.Platform.Shell.UI
{
    public abstract class UIControl
    {
        public UIWindow Window;
        public string Text;
        public Point Location;
        public Point Size;
        public Color Foreground = Color.Black;
		public Color Background = Color.Transparent;
		public Action OnActivation;

        public abstract void Render(Canvas cnv);
    }
}
