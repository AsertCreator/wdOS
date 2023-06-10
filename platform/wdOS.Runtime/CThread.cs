using System;
using System.Collections.Generic;
using System.Text;
using wdOS.Pillow;

namespace wdOS.Platform
{
    public class CThread : IRuntimeComponent
    {
        public EEExecutable GetLibrary()
        {
            EEExecutable exe = new();


            return exe;
        }
        public string GetName() => nameof(CThread);
    }
}
