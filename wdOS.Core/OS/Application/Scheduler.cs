//#define MULTITHREADING
using Cosmos.HAL;
using IL2CPU.API.Attribs;
using XSharp.Assembler;

#if MULTITHREADING
namespace wdOS.Core.OS.Application
{
    internal static class Scheduler
    {
        internal static int SwitchDelay = 10;
        internal static int SwitchCount = 0;
        internal static PIT.PITTimer Timer = null;
        internal static Thread LastThread = null;
        internal static Thread FirstThread = null;
        internal static Thread CurrentThread = null;
        internal static int ThreadCount = 0;
        internal static void Initialize()
        {
            Timer = new PIT.PITTimer(x => 
            {
                CurrentThread.RegisterEAX = x.EAX;
                CurrentThread.RegisterEBX = x.EBX;
                CurrentThread.RegisterECX = x.ECX;
                CurrentThread.RegisterEDX = x.EDX;
                CurrentThread.RegisterESP = x.ESP;
                CurrentThread.RegisterEBP = x.EBP;
                CurrentThread.RegisterESI = x.ESI;
                CurrentThread.RegisterEDI = x.EDI;
                CurrentThread.RegisterEIP = x.EIP;
                CurrentThread.RegisterEFlags = (uint)x.EFlags;
                if (CurrentThread.TID != CurrentThread.NextThread.TID)
                {
                    CurrentThread = CurrentThread.NextThread;
                    NativeSwitch(CurrentThread.RegisterEAX, CurrentThread.RegisterEBX,
                                 CurrentThread.RegisterECX, CurrentThread.RegisterEDX,
                                 CurrentThread.RegisterESP, CurrentThread.RegisterEBP,
                                 CurrentThread.RegisterESI, CurrentThread.RegisterEDI,
                                 CurrentThread.RegisterEIP, CurrentThread.RegisterEFlags);
                }
            }, 
            (ulong)SwitchDelay * 1000000ul, true);
            CurrentThread = FirstThread;
            Global.PIT.RegisterTimer(Timer);
        }
        [PlugMethod(PlugRequired = true)]
        internal static void NativeSwitch(
            uint eax, uint ebx, uint ecx, uint edx, 
            uint esp, uint ebp, uint esi, uint edi, 
            uint eip, uint efl)
        { }
    }
    [Plug(Target = typeof(Scheduler))]
    internal static class NativeSwitchImpl
    {
        [PlugMethod(Assembler = typeof(NativeSwitchASM))]
        internal static void NativeSwitch(
            uint eax, uint ebx, uint ecx, uint edx,
            uint esp, uint ebp, uint esi, uint edi,
            uint eip, uint efl)
        { }
    }
    internal class NativeSwitchASM : AssemblerMethod
    {
        public override void AssembleNew(Assembler aAssembler, object aMethodInfo)
        {
            new LiteralAssemblerCode("mov esp, [esp + 44]");
            new LiteralAssemblerCode("pushf");
            new LiteralAssemblerCode("mov esp, [esp - 44]");
            new LiteralAssemblerCode("mov eax, [esp + 8]");
            new LiteralAssemblerCode("mov ebx, [esp + 12]");
            new LiteralAssemblerCode("mov ecx, [esp + 16]");
            new LiteralAssemblerCode("mov edx, [esp + 20]");
            new LiteralAssemblerCode("mov esi, [esp + 24]");
            new LiteralAssemblerCode("mov edi, [esp + 28]");
            new LiteralAssemblerCode("mov ebp, [esp + 32]");
            new LiteralAssemblerCode("mov esp, [esp + 36]");
            new LiteralAssemblerCode("jmp [esp + 40]");
        }
    }
}
#endif
