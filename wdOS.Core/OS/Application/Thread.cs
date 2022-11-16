//#define MULTITHREADING
using Cosmos.Core;
using Cosmos.Core.Memory;
using System;
using wdOS.Core.OS.Foundation;

#if MULTITHREADING
namespace wdOS.Core.OS.Application
{
    internal unsafe class Thread
    {
        internal static int FreeTID = 0;
        internal int TID = FreeTID++;
        internal int PID = 0;
        internal int Age = 0;
        internal bool ToDestroy;
        internal bool IsRunning;
        internal Thread NextThread;
        internal Action Entrypoint;
        internal uint RegisterEFlags = 0;
        internal uint RegisterEIP = 0;
        internal uint RegisterEAX = 0;
        internal uint RegisterEBX = 0;
        internal uint RegisterECX = 0;
        internal uint RegisterEDX = 0;
        internal uint RegisterESP = 0;
        internal uint RegisterEBP = 0;
        internal uint RegisterESI = 0;
        internal uint RegisterEDI = 0;
        internal Thread(Action act)
        {
            if (Scheduler.CurrentThread == null) Scheduler.CurrentThread = this;
            if (Scheduler.FirstThread == null) Scheduler.FirstThread = this;
            if (Scheduler.LastThread != null) Scheduler.LastThread.NextThread = this;
            Entrypoint = act;
            RegisterESP = CreateStack(256 * 1024);
            RegisterEBP = RegisterESP;
            Scheduler.LastThread = this;
            NextThread = Scheduler.FirstThread;
        }
        internal static void Execute()
        {
            var thread = Scheduler.CurrentThread;
            thread.IsRunning = true;
            thread.Entrypoint();
            thread.ToDestroy = true;
            while (true) { }
        }
        internal static uint CreateStack(uint size) => Heap.SafeAlloc(size);
        internal static void WaitForMilli(int milliseconds) => Kernel.WaitFor(milliseconds);
    }
}
#endif