using Cosmos.Core;
using Cosmos.Core.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace wdOS.Platform.Foundation
{
    public static class ProcessExecutor
    {
        public const int FileStdOut = -1;
        public const int FileStdErr = -2;
        public const int FileStdIn = -3;
        public static void Initialize()
        {
            INTs.SetIntHandler(0xF0, HandleSWI);
        }
        public unsafe static void HandleSWI(ref INTs.IRQContext ctx)
        {
            uint eax = ctx.EAX;
            uint ecx = ctx.ECX;
            uint result = unchecked((uint)-1);

            switch ((Syscall)eax)
            {
                case Syscall.MSG_IO_WRITE:
                    {
                        IOInfo* info = (IOInfo*)ecx;
                        switch (info->File)
                        {
                            case FileStdErr:
                            case FileStdOut:
                                for (int i = 0; i < info->Size; i++)
                                    Console.Write(info->Buffer[i]);
                                break;
                            case FileStdIn:
                            default:
                                break;
                        }
                        result = 0;
                        break;
                    }
                case Syscall.MSG_IO_READ:
                    {
                        IOInfo* info = (IOInfo*)ecx;
                        result = 0;
                        break;
                    }
                case Syscall.MSG_MEMORY_ALLOC:
                    {
                        result = (uint)Heap.Alloc(ecx);
                        break;
                    }
                case Syscall.MSG_MEMORY_FREE:
                    {
                        Heap.Free((void*)ecx);
                        result = 0;
                        break;
                    }
                case Syscall.MSG_CONTROL_STOP:
                    break;
                case Syscall.MSG_CONTROL_SETTIME:
                    break;
                case Syscall.MSG_CONTROL_GETTIME:
                    break;
                case Syscall.MSG_CONTROL_EXEC:
                    break;
                default:
                    break;
            }

            ctx.EAX = result;
        }
        public static int Execute(Process process)
        {
            int result = 0;
            switch (process.SubsystemType)
            {
                case 0: break; // embedded 
                case 1: // native
                    // todo: this
                    break;
                case 2: break; // script
                    
            }
            return result;
        }
        public enum Syscall
        {
            MSG_IO_WRITE = 10, 
            MSG_IO_READ = 11, 
            MSG_MEMORY_ALLOC = 20, 
            MSG_MEMORY_FREE = 21, 
            MSG_CONTROL_STOP = 30,
            MSG_CONTROL_SETTIME = 31, 
            MSG_CONTROL_GETTIME = 32,
            MSG_CONTROL_EXEC = 33,
        }
        [StructLayout(LayoutKind.Sequential)]
        public unsafe struct IOInfo
        {
            public char* Buffer;
            public int Index;
            public int Size;
            public int File;
        }
    }
}
