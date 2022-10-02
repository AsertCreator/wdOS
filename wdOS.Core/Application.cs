using Cosmos.Core.Memory;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using wdOS.Core.Threading;

namespace wdOS.Core
{
    internal unsafe class Application
    {
        internal static int NextPID = 0;
        internal int PID = NextPID++;
        internal string FullPath = "0:/RAM Disk App";
        internal string[] CmdArgs;
        internal LEFHeader ExecHeader = new();
        internal LEFSectionHeader CodeSectionHeader = new();
        internal LEFSectionHeader DataSectionHeader = new();
        internal byte* ExecData;
        internal Thread MainThread;
        internal CrashInfo Info;
        internal bool Destroyed;
        internal bool IsPlain;
        internal Application(byte[] data) => CommonConstructor(data);
        internal Application(byte[] data, string path) { FullPath = Path.GetFullPath(path); CommonConstructor(data); }
        internal Application(string path) 
        {
            if (FSUtils.FileExists(path))
            {
                FullPath = Path.GetFullPath(path);
                CommonConstructor(FSUtils.ReadBytesFile(path));
            }
        }
        private void CommonConstructor(byte[] data)
        {
            if (data.Length >= int.MaxValue / 2)
            {
                Kernel.Applications.Add(this);
                IsPlain = !(BitConverter.ToUInt16(new byte[] { data[1], data[0] }) == LEFHeader.RequiredMagic);
                if (!IsPlain) ParseAsLEF(data);
                CopyExecData(data);
            }
        }
        internal void ParseAsLEF(byte[] data)
        {
            try
            {
                using (BinaryReader br = new(new MemoryStream(data), Encoding.ASCII))
                {
                    ExecHeader.LEF_Magic = br.ReadUInt16();
                    ExecHeader.LEF_Class = br.ReadByte();
                    ExecHeader.LEF_Version = br.ReadByte();
                    ExecHeader.LEF_FO_CodeSection = br.ReadUInt32();
                    ExecHeader.LEF_FO_DataSection = br.ReadUInt32();
                    ExecHeader.LEF_FO_Entrypoint = br.ReadUInt32();
                    if (ExecHeader.LEF_Class == (byte)LEFClass.Executable64 ||
                        ExecHeader.LEF_Class == (byte)LEFClass.Library64)
                    {
                        Console.WriteLine("Error! Tried to open 64-bit app, that is not compatible");
                        Info = new CrashInfo("Tried to open 64-bit app", CrashType.ParseError);
                        return;
                    }
                    LEFSectionHeader ReadSection()
                    {
                        LEFSectionHeader sh = new();
                        sh.LEF_Name0 = (byte)br.ReadChar();
                        sh.LEF_Name1 = (byte)br.ReadChar();
                        sh.LEF_Name2 = (byte)br.ReadChar();
                        sh.LEF_Name3 = (byte)br.ReadChar();
                        sh.LEF_FileOffset = (byte)br.ReadUInt32();
                        return sh;
                    }
                    br.BaseStream.Position = ExecHeader.LEF_FO_CodeSection;
                    CodeSectionHeader = ReadSection();
                    br.BaseStream.Position = ExecHeader.LEF_FO_DataSection;
                    DataSectionHeader = ReadSection();
                }
            }
            catch
            {
                Info = new CrashInfo("Can't parse LEF application", CrashType.ParseError);
            }
        }
        internal void CopyExecData(byte[] data)
        {
            ExecData = Heap.Alloc((uint)data.Length);
            for (int i = 0; i < data.Length; i++)
            { ExecData[i] = data[i]; }
        }
        internal int StartExec(string[] args)
        {
            try
            {
                var result = 0;
                CmdArgs = args;
                if (Kernel.BuildConstants.UseMultiThreading) result = MTStart();
                else result = NMTStart();
                return result;
            }
            catch
            {
                Info = new CrashInfo("Some unexcepted error occurred!", CrashType.ParseError);
                return -1;
            }
        }
        private int NMTStart()
        {
            fixed (void* addr = &ExecData) 
            {
                ProcessorScheduler.JumpTo(ExecData[CodeSectionHeader.LEF_FileOffset]);
                Destroy();
                return 0; 
            }
        }
        private int MTStart()
        {
            MainThread = new();
            MainThread.PID = PID;
            MainThread.Entry = () =>
            {
                fixed (void* addr = &ExecData)
                {
                    ProcessorScheduler.JumpTo(ExecData[CodeSectionHeader.LEF_FileOffset]);
                    Destroy();
                }
            };
            return 0;
        }
        internal void Destroy()
        {
            Destroyed = true;
            Kernel.Applications.Remove(this);
            if (Kernel.BuildConstants.UseMultiThreading)
            { ProcessorScheduler.StopThread(MainThread); }
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
