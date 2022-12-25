using Cosmos.Core;
using Cosmos.HAL;
using System;
using System.Runtime.InteropServices;
using wdOS.Core.OS.Foundation;
using IL2CPU.API.Attribs;
using XSharp.Assembler;
using XSharp;

namespace wdOS.Core.OS.Shells.CShell
{
    internal unsafe class UShellManager : Shell
    {
        internal static UShellManager Instance;
        internal static PIT PITManager;
        internal static PIT.PITTimer SwitchTimer;
        internal static Context LastContext = null;
        internal static Context FirstContext = null;
        internal static Context CurrentContext = null;
        internal static uint ContextCount;
        internal static ushort ContextIDNext;
        internal static ulong Framecount;
        internal override string Name => "UShell";
        internal override int MajorVersion => Kernel.BuildConstants.VersionMajor;
        internal override int MinorVersion => Kernel.BuildConstants.VersionMinor;
        internal override int PatchVersion => Kernel.BuildConstants.VersionPatch;
        internal override void BeforeRun()
        {
            try
            {
                PITManager = Cosmos.HAL.Global.PIT;
                CreateContext("run", Run);
                SwitchTimer = new(x =>
                {
                    Framecount++;
                    CurrentContext.RegisterEAX = x.EAX;
                    CurrentContext.RegisterEBX = x.EBX;
                    CurrentContext.RegisterECX = x.ECX;
                    CurrentContext.RegisterEDX = x.EDX;
                    CurrentContext.RegisterEIP = x.EIP;
                    CurrentContext.RegisterEFL = (uint)x.EFlags;
                    CurrentContext.RegisterESP = x.ESP;
                    CurrentContext.RegisterEBP = x.EBP;
                    CurrentContext.RegisterESI = x.ESI;
                    CurrentContext.RegisterEDI = x.EDI;
                    CurrentContext = CurrentContext.Next;
                    DumpContext();
                }, 
                1000000 * 1000, true);
                PITManager.RegisterTimer(SwitchTimer);
            }
            catch { }
        }
        internal override void Run()
        {
            Kernel.Log("Starting UShell...");
            ulong num = 0;
            while (true) 
            {
                num %= (ulong)new Random().Next();
            }
        }
        internal static void DumpContext()
        {
            Console.WriteLine($"S !!! dump {CurrentContext.ContextID}: EAX={CurrentContext.RegisterEAX:X}");
            Console.WriteLine($"M !!! dump {CurrentContext.ContextID}: EBX={CurrentContext.RegisterEBX:X}");
            Console.WriteLine($"M !!! dump {CurrentContext.ContextID}: ECX={CurrentContext.RegisterECX:X}");
            Console.WriteLine($"M !!! dump {CurrentContext.ContextID}: EDX={CurrentContext.RegisterEDX:X}");
            Console.WriteLine($"M !!! dump {CurrentContext.ContextID}: ESP={CurrentContext.RegisterESP:X}");
            Console.WriteLine($"M !!! dump {CurrentContext.ContextID}: EBP={CurrentContext.RegisterEBP:X}");
            Console.WriteLine($"M !!! dump {CurrentContext.ContextID}: ESI={CurrentContext.RegisterESI:X}");
            Console.WriteLine($"E !!! dump {CurrentContext.ContextID}: EDI={CurrentContext.RegisterEDI:X}");
        }
        internal static void StopContext()
        {
            Context destroy = CurrentContext;
            Context prev = destroy.Prev;
            prev.Next = destroy.Next;
            GCImplementation.Free(destroy);
            ContextCount--;
        }
        internal static void CreateContext(string name, Action action)
        {
            Context ctx = new Context() { Name = name, Entry = action };
            ctx.ContextID = ContextIDNext++;
            ctx.RegisterEIP = GetThreadEntryAddress();
            if (CurrentContext == null && LastContext == null && FirstContext == null)
            {
                CurrentContext = ctx;
                LastContext = ctx;
                FirstContext = ctx;
            }
            LastContext.Next = ctx;
            ctx.Next = FirstContext;
            ctx.Prev = LastContext;
            ContextCount++;
        }
        internal static void ThreadEntry()
        {
            Kernel.Log($"A !! starting context: name = \"{CurrentContext.Name}\", id = {CurrentContext.ContextID} !!");
            CurrentContext.Entry();
            Kernel.Log($"O !! stopping context: name = \"{CurrentContext.Name}\", id = {CurrentContext.ContextID} !!");
            StopContext();
        }
        [PlugMethod(Assembler = typeof(GTEAAssembler))]
        internal static uint GetThreadEntryAddress() { return 0; }
        internal class Mutex
        {
            private bool Gate;
            internal void Lock()
            {
                while (Gate) { }
                Gate = true;
            }
            internal void Unlock()
            {
                Gate = false;
            }
        }
    }
    [Plug(Target = typeof(UShellManager))]
    internal class GTEAAssembler : AssemblerMethod
    {
        public override void AssembleNew(Assembler aAssembler, object aMethodInfo)
        {
            XS.Push(Label.GetLabel(typeof(UShellManager).GetMethod("ThreadEntry")));
            XS.Return();
        }
    }
    internal struct FXContext
    {
        internal ulong Part0; internal ulong Part1; internal ulong Part2; internal ulong Part3;
        internal ulong Part4; internal ulong Part5; internal ulong Part6; internal ulong Part7;
        internal ulong Part8; internal ulong Part9; internal ulong Part10; internal ulong Part11;
        internal ulong Part12; internal ulong Part13; internal ulong Part14; internal ulong Part15;
        internal ulong Part16; internal ulong Part17; internal ulong Part18; internal ulong Part19;
        internal ulong Part20; internal ulong Part21; internal ulong Part22; internal ulong Part23;
        internal ulong Part24; internal ulong Part25; internal ulong Part26; internal ulong Part27;
        internal ulong Part28; internal ulong Part29; internal ulong Part30; internal ulong Part31;
        internal ulong Part32; internal ulong Part33; internal ulong Part34; internal ulong Part35;
        internal ulong Part36; internal ulong Part37; internal ulong Part38; internal ulong Part39;
        internal ulong Part40; internal ulong Part41; internal ulong Part42; internal ulong Part43;
        internal ulong Part44; internal ulong Part45; internal ulong Part46; internal ulong Part47;
        internal ulong Part48; internal ulong Part49; internal ulong Part50; internal ulong Part51;
        internal ulong Part52; internal ulong Part53; internal ulong Part54; internal ulong Part55;
        internal ulong Part56; internal ulong Part57; internal ulong Part58; internal ulong Part59;
        internal ulong Part60; internal ulong Part61; internal ulong Part62; internal ulong Part63;
    }
    [StructLayout(LayoutKind.Sequential)]
    internal unsafe class Context
    {
        internal ushort ContextID;
        internal ushort ContextType;
        internal uint RegisterEAX;
        internal uint RegisterEBX;
        internal uint RegisterECX;
        internal uint RegisterEDX;
        internal uint RegisterEIP;
        internal uint RegisterEFL;
        internal uint RegisterESP;
        internal uint RegisterEBP;
        internal uint RegisterESI;
        internal uint RegisterEDI;
        internal Context Prev;
        internal Context Next;
        internal string Name;
        internal Action Entry;
    }
}
