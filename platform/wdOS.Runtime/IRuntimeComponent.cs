using System;
using System.Collections.Generic;
using System.Text;
using wdOS.Pillow;

namespace wdOS.Platform
{
    public interface IRuntimeComponent
    {
        public EEExecutable GetLibrary();
        public string GetName();
    }
}
