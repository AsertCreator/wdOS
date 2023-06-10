using System;
using System.Collections.Generic;
using System.Text;
using wdOS.Pillow;

namespace wdOS.Platform
{
    public unsafe class Process
    {
        public int PID;
        public string BinaryPath;
        public string ConsoleArguments;
        public string CurrentDirectory;
        public EEExecutable ExecutableFile;
        public Process Executor;
        public bool IsRunning = false;
    }
}
