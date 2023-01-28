using Cosmos.Core;
using Cosmos.HAL;
using System;
using IL2CPU.API.Attribs;

namespace wdOS.Core.Foundation.Threading
{
    internal class Thread
    {
        internal static int NextTID = 0;
        internal static int ThreadCount = 0;
        internal static Thread FirstThread;
        internal static Thread LastThread;
        internal static Thread CurrentThread;
        internal static bool IsInitialized;
        internal int TID = NextTID++;
        internal Context ThreadContext = new();
        internal string Name = "Unnamed Thread";
        internal Action Entry;
        internal Thread NextThread;
        internal Thread PrevThread;
        internal ThreadState State = ThreadState.Paused;
        internal ThreadType Type = ThreadType.System;
        internal Exception ThreadException;
        private Thread() { }
        internal static void Initialize()
        {
            var pit = Cosmos.HAL.Global.PIT;
            var timer = new PIT.PITTimer(SwitchTask, 1000000, true);
            pit.RegisterTimer(timer);
            CreateIdleThread();
        }
        internal static unsafe void CreateIdleThread()
        {
            Thread thread = new();
            thread.PrevThread = thread;
            thread.NextThread = thread;
            FirstThread = thread;
            LastThread = thread;
            CurrentThread = thread;
            thread.ThreadContext.StackData = new byte[4096];
            thread.ThreadContext.RegisterEIP = (uint)new Action(() => { ExecuteThread(); }).GetHashCode();
            fixed (byte* ptr = &thread.ThreadContext.StackData[0])
            {
                thread.ThreadContext.RegisterESP = ((uint)ptr) + 4096;
                thread.ThreadContext.RegisterEBP = (uint)ptr;
            }
            thread.Entry = () => { IdleProcess(); };
            ThreadCount++;
        }
        internal static unsafe Thread CreateThread(string name, Action act, int stacksize = 32 * 1024)
        {
            Thread thread = new();
            thread.PrevThread = LastThread;
            LastThread.PrevThread.NextThread = thread;
            LastThread = thread;
            FirstThread.PrevThread = thread;
            thread.Name = name;
            thread.NextThread = FirstThread;
            thread.ThreadContext.StackData = new byte[stacksize];
            thread.ThreadContext.RegisterEIP = (uint)new Action(() => { ExecuteThread(); }).GetHashCode();
            fixed (byte* ptr = &thread.ThreadContext.StackData[0])
            {
                thread.ThreadContext.RegisterESP = ((uint)ptr) + (uint)stacksize;
                thread.ThreadContext.RegisterEBP = (uint)ptr;
            }
            thread.Entry = act;
            ThreadCount++;
            IsInitialized = true;
            return thread;
        }
        internal static void SwitchTask(INTs.IRQContext context)
        {
            if (IsInitialized)
            {
                CurrentThread.State = ThreadState.Paused;
                CurrentThread.ThreadContext.RegisterEAX = context.EAX;
                CurrentThread.ThreadContext.RegisterEBX = context.EBX;
                CurrentThread.ThreadContext.RegisterECX = context.ECX;
                CurrentThread.ThreadContext.RegisterEDX = context.EDX;
                CurrentThread.ThreadContext.RegisterESP = context.ESP;
                CurrentThread.ThreadContext.RegisterEBP = context.EBP;
                CurrentThread.ThreadContext.RegisterESI = context.ESI;
                CurrentThread.ThreadContext.RegisterEDI = context.EDI;
                CurrentThread.ThreadContext.RegisterEFL = (uint)context.EFlags;
                CurrentThread.ThreadContext.RegisterEIP = context.EIP;
                var next = CurrentThread.NextThread;
                next.State = ThreadState.Running;
                CurrentThread = next;
                var ctx = CurrentThread.ThreadContext;
                Cosmos.Core.Global.PIC.EoiMaster();
                NativeSwitch(
                    ctx.RegisterEAX, ctx.RegisterEBX, ctx.RegisterECX, ctx.RegisterEDX,
                    ctx.RegisterESP, ctx.RegisterEBP, ctx.RegisterESI, ctx.RegisterEDI,
                    ctx.RegisterEFL, ctx.RegisterEIP);
            }
        }
        internal static void IdleProcess()
        {
            Kernel.Log($"Being idle process!");
            while (true) { }
        }
        private static void ExecuteThread()
        {
            try
            {
                Kernel.Log($"Thread {CurrentThread.TID} has been created!");
                CurrentThread.Entry();
                Kernel.Log($"Thread {CurrentThread.TID} has died!");
                CurrentThread.State = ThreadState.Dead;
            }
            catch (Exception e)
            {
                Kernel.Log($"Thread {CurrentThread.TID} crashed with exception: {e} {e.Message}");
                CurrentThread.ThreadException = e;
                CurrentThread.State = ThreadState.Failed;
            }
            while (true) { }
        }
        [PlugMethod(Assembler = typeof(NativeSwitchImpl))]
        internal static void NativeSwitch(
            uint eax, uint ebx, uint ecx, uint edx,
            uint esp, uint ebp, uint esi, uint edi,
            uint efl, uint eip)
        { }
    }
    internal enum ThreadState { Paused, Running, Failed, Dead }
    internal enum ThreadType { System, Idle, App }
}
