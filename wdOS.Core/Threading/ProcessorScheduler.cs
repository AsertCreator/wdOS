using Cosmos.Core;
using Cosmos.Core.Memory;
using Cosmos.HAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using IL2CPU.API.Attribs;
using XSharp;
using XSharp.Assembler;
using IL2CPU.API;
using System.Reflection;

namespace wdOS.Core.Threading
{
    internal static class ProcessorScheduler
    {
        internal static string MultithreadingModelVersion = Kernel.GetVersion();
        internal static List<Thread> AllThreads = new();
        internal static Thread CurrentThread;
        internal static PIT.PITTimer Timer;
        internal static bool Initiliazed;
        internal static bool DidRunCreate;
        internal static int ThreadID;
        internal static int ThreadCount;
        internal const int NanosecondTimeout = 2 * 1000 * 1000 + 1000;
        internal const int NanosecondRoom = 1000;
        internal const int StackSize = 16 * 1024;
        internal static void StartMultiThreading()
        {
            if (!Initiliazed)
            {
                Kernel.Log("Initilizing multithreading");
                CreateThread(0, "System Idle", () =>
                {
                    // do nothing, its  i d l e  after all
                    Kernel.Log("idle");
                    while (true) { }
                });
                PITControl(1);
                INTs.SetIrqHandler(0, SwitchTask);
                Initiliazed = true;
                Kernel.Log("Done initilizing multithreading");
            }
        }
        internal static unsafe Thread CreateThread(int pid, string name, ThreadStart start)
        {
            Kernel.Log("Creating thread...");
            var thread = new Thread()
            {
                PID = pid,
                Name = name,
                Entry = start,
            };
            thread.StackTop = Heap.SafeAlloc(StackSize);
            thread.ESP = (uint)thread.SetupStack((uint*)(thread.StackTop + 4000));
            AllThreads.Add(thread);
            ThreadCount++;
            if (!DidRunCreate) { CurrentThread = thread; DidRunCreate = true; }
            Kernel.Log($"Created \"{name}\" thread: pid={pid},tid={thread.TID}");
            return thread;
        }
        internal static unsafe void StopThread(Thread tid)
        {
            if (tid != null)
            {
                Heap.Free((void*)tid.StackTop);
                AllThreads.Remove(tid);
                ThreadCount--;
                Kernel.Log($"Stopped \"{tid.Name}\" thread: pid={tid.PID},tid={tid.TID}");
            }
        }
        internal static void PITControl(ushort freq)
        {
            IOPort counter0 = new IOPort(0x40);
            IOPort cmd = new IOPort(0x43);
            ushort divisor = 0;
            cmd.Byte = 0x36;
            counter0.Byte = (byte)divisor;
            counter0.Byte = (byte)(divisor >> 8);
            IOPort pA1 = new IOPort(0xA1);
            IOPort p21 = new IOPort(0xA1);
            pA1.Byte = 0x00;
            p21.Byte = 0x00;
        }
        internal static void SwitchTask(ref INTs.IRQContext context)
        {
            Kernel.Log("Switch!");
            if (ThreadCount > 1)
            {
                CurrentThread.IsRunning = false;
                // save current stack and pointers state
                CurrentThread.EIP = context.EIP;
                CurrentThread.EBP = context.EBP;
                CurrentThread.ESP = context.ESP;
                // save current general registers
                CurrentThread.EAX = context.EAX;
                CurrentThread.EBX = context.EBX;
                CurrentThread.ECX = context.ECX;
                CurrentThread.EDX = context.EDX;
                CurrentThread.ESI = context.ESI;
                CurrentThread.EDI = context.EDI;
            // select next thread
            tryagain:
                ThreadID++;
                if (ThreadID == ThreadCount) ThreadID = 0;
                CurrentThread = AllThreads[ThreadID];
                // switch only if thread is on paused state
                if (!CurrentThread.IsRunning)
                {
                    // switch thread
                    CurrentThread.IsRunning = true;
                    NativeSwitch(CurrentThread.EIP,
                        CurrentThread.EAX, CurrentThread.EBX, CurrentThread.ECX, CurrentThread.EDX,
                        CurrentThread.ESP, CurrentThread.EBP, CurrentThread.ESI, CurrentThread.EDI);
                }
                else goto tryagain;
            }
        }
        [PlugMethod(Assembler = typeof(JumpASM))]
        internal static void JumpTo(uint address) { }
        [PlugMethod(Assembler = typeof(NativeSwitchASM))]
        internal static void NativeSwitch(uint address, uint eax, uint ebx, uint ecx, uint edx, uint ebp, uint esp, uint esi, uint edi) { }
        [PlugMethod(Assembler = typeof(GetECAddressASM))]
        internal static uint GetECAddress() => 0;
        [Plug(Target = typeof(ProcessorScheduler))]
        internal class NativeSwitchASM : AssemblerMethod
        {
            public override void AssembleNew(Assembler aAssembler, object aMethodInfo)
            {
                new LiteralAssemblerCode("mov eax, [esp + 12]");
                new LiteralAssemblerCode("mov ebx, [esp + 16]");
                new LiteralAssemblerCode("mov ecx, [esp + 20]");
                new LiteralAssemblerCode("mov edx, [esp + 24]");
                new LiteralAssemblerCode("mov ebp, [esp + 28]");
                new LiteralAssemblerCode("mov esp, [esp + 32]");
                new LiteralAssemblerCode("mov esi, [esp + 36]");
                new LiteralAssemblerCode("mov edi, [esp + 40]");
                new LiteralAssemblerCode("jmp dword[esp + 8]");
            }
        }
        [Plug(Target = typeof(ProcessorScheduler))]
        internal class JumpASM : AssemblerMethod
        {
            public override void AssembleNew(Assembler aAssembler, object aMethodInfo)
            {
                new LiteralAssemblerCode("jmp dword[esp + 8]");
            }
        }
        internal static void Dummy() { bool lol = false; if (lol) Thread.ExecuteCode(); }
        [Plug(Target = typeof(ProcessorScheduler))]
        internal class GetECAddressASM : AssemblerMethod
        {
            public override void AssembleNew(Assembler aAssembler, object aMethodInfo)
            {
                XS.Set(XSRegisters.EAX, LabelName.Get(typeof(Thread).GetRuntimeMethods().Where(x => x.Name == "ExecuteCode").First()));
                XS.Push(XSRegisters.EAX);
            }
        }
    }
}
