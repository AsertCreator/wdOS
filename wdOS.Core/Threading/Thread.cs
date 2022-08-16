using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace wdOS.Core.Threading
{
    internal unsafe class Thread
    {
        internal static Thread Current => ProcessorScheduler.CurrentThread;
        internal static int NextTID;
        internal string Name;
        internal int TID = NextTID++;
        internal int PID;
        internal ulong LeftWaiting;
        internal uint EIP;
        internal uint ESP;
        internal uint EBP;
        internal uint StackTop;
        internal ThreadStart Entry;
        internal bool IsRunning = false;
        internal uint EAX;
        internal uint EBX;
        internal uint ECX;
        internal uint EDX;
        internal uint ESI;
        internal uint EDI;
        internal void Wait(int ms)
        {
            Kernel.WaitForSeconds(ms);
        }
        internal uint* SetupStack(uint* stack)
        {
            Kernel.Log($"Setting up stack at 0x{(uint)stack:X8}");
            ProcessorScheduler.Dummy();
            uint origin = (uint)stack;
            *--stack = 0xFFFFFFFF; // trash
            *--stack = 0xFFFFFFFF; // trash
            *--stack = 0xFFFFFFFF; // trash
            *--stack = 0xFFFFFFFF; // trash
            *--stack = 0x10; // ss ?
            *--stack = 0x00000202; // eflags
            *--stack = 0x8; // cs
            *--stack = ProcessorScheduler.GetECAddress(); // eip
            *--stack = 0; // error
            *--stack = 0; // int
            *--stack = 0; // eax
            *--stack = 0; // ebx
            *--stack = 0; // ecx
            *--stack = 0; // offset
            *--stack = 0; // edx
            *--stack = 0; // esi
            *--stack = 0; // edi
            *--stack = origin; //ebp
            *--stack = 0x10; // ds
            *--stack = 0x10; // fs
            *--stack = 0x10; // es
            *--stack = 0x10; // gs
            EIP = ProcessorScheduler.GetECAddress();
            Kernel.Log("Stack setup done!");
            return stack;
        }
        internal static void ExecuteCode()
        {
            Kernel.Log($"Executing code of \"{Current.Name}\" thread");
            Current.Entry();
            Kernel.Log($"Reached end of \"{Current.Name}\" thread");
            ProcessorScheduler.StopThread(Current);
        }
    }
}
