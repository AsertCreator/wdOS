using Cosmos.Core.Memory;
using System.IO;

namespace wdOS.Core
{
    internal unsafe class Application
    {
        internal static int NextPID = 0;
        internal int PID = NextPID++;
        internal string FullPath = "0:/virtapp";
        internal byte* ExecData;
        internal CrashInfo Info;
        internal bool Destroyed;
        internal Application(byte[] data) => CommonConstructor(data);
        internal Application(byte[] data, string path) { FullPath = Path.GetFullPath(path); CommonConstructor(data); }
        internal Application(string path)
        {
            if (FileSystemManager.FileExists(path))
            {
                FullPath = Path.GetFullPath(path);
                CommonConstructor(FileSystemManager.ReadBytesFile(path));
            }
        }
        private void CommonConstructor(byte[] data)
        {
            if (data.Length <= uint.MaxValue / 16)
            {
                Kernel.Applications.Add(this);
                CopyExecData(data);
            }
        }
        internal void CopyExecData(byte[] data)
        {
            ExecData = Heap.Alloc((uint)data.Length);
            for (int i = 0; i < data.Length; i++)
            { ExecData[i] = data[i]; }
        }
        internal int StartExec()
        {
            try
            {
                //ProcessorScheduler.JumpTo(ExecData[CodeSectionHeader.LEF_FileOffset]);
                var result = 0;
                Destroy();
                return result;
            }
            catch
            {
                Info = new CrashInfo("Some unexcepted error occurred!", CrashType.ParseError);
                return -1;
            }
        }
        internal void Destroy()
        {
            Destroyed = true;
            _ = Kernel.Applications.Remove(this);
        }
    }
    internal struct CrashInfo
    {
        internal CrashInfo(string msg, CrashType type)
        {
            Message = msg;
            CrashType = type;
        }
        internal string Message;
        internal CrashType CrashType;
    }
    internal enum CrashType
    {
        ParseError, ExecError, MTError,
        GPFError, PFError
    }
}
