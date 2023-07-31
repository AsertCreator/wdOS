using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using wdOS.Platform.Shell.UI;

namespace wdOS.Platform.Core
{
    public static class ShellManager
    {
        private static object temp = new();
        public static void Start()
        {
			WindowManager.Initialize();
			WindowManager.Start();
		}
    }
}
