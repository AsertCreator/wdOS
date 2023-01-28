using Cosmos.Core;
using Cosmos.Core.Memory;
using IL2CPU.API.Attribs;
using System;
using System.Collections.Generic;
using XSharp.Assembler;
using XSharp;
using wdOS.Core.Shell.TShell;

namespace wdOS.Core.Foundation
{
    internal unsafe static class Runtime
    {
        internal static List<Application> CurrentApplications = new();
        internal static uint DefaultStackSize = 16 * 1024;
        internal static void Setup()
        {
            INTs.SetIntHandler(0xF0, HandleAPI);
            INTs.GeneralProtectionFault = HandleAPI;
            CPU.UpdateIDT(true);
        }
        internal static void HandleGeneralProtectionFault(ref INTs.IRQContext context)
        {
            Console.BackgroundColor = ConsoleColor.Black;
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Current application tried to execute forbidden instruction");
            DumpRegisters(context);
            Console.WriteLine("This application will be terminated");
        }
        internal static void HandleAPI(ref INTs.IRQContext context)
        {
            switch (context.EAX)
            {
                case 0x00: // Write char to next free position in terminal.
                    Console.Write((char)context.EBX);
                    break;
                case 0x01: // Write line without line terminator to next free position in terminal.
                    int index = 0;
                    char* str = (char*)context.EBX;
                    while (str[index] != 0) Console.Write(str[index++]);
                    break;
                case 0x02: // Read char from terminal and returns it.
                    context.EAX = Console.ReadKey().KeyChar;
                    break;
                case 0x03: // Read line from terminal and returns it.                              
                    string line = Console.ReadLine();
                    char* buffer = (char*)Heap.Alloc((uint)(line.Length + 1));
                    for (int i = 0; i < line.Length; i++) buffer[i] = line[i];
                    context.EAX = (uint)buffer;
                    break;
                case 0x04: // Put char at specified position in terminal.                          
                    Console.WriteLine("This runtime function (0x04) is not implemented!");
                    context.EAX = uint.MaxValue;
                    break;
                case 0x05: // Clear terminal a.k.a. puts space in every terminal char.             
                    Console.Clear();
                    break;
                case 0x06: // Set foreground color in terminal.   
                    Console.ForegroundColor = (ConsoleColor)context.EBX;
                    break;
                case 0x07: // Set background color in terminal.   
                    Console.BackgroundColor = (ConsoleColor)context.EBX;
                    break;
                case 0x08: // Get foreground color in terminal.   
                    context.EAX = (uint)Console.ForegroundColor;
                    break;
                case 0x09: // Get background color in terminal.    
                    context.EAX = (uint)Console.BackgroundColor;
                    break;
                case 0x10: // Allocate area in RAM.               
                    context.EAX = Heap.SafeAlloc(context.EBX);
                    break;
                case 0x11: // Free allocated area in RAM.
                    Heap.Free((void*)context.EBX);
                    break;
                case 0x20: // Terminate this program.             
                    Console.WriteLine("This runtime function (0x20) is not implemented!");
                    context.EAX = uint.MaxValue;
                    break;
                case 0x21: // Control a PIT chip on motherboard.  
                    Console.WriteLine("This runtime function (0x21) is not implemented!");
                    context.EAX = uint.MaxValue;
                    break;
                case 0x22: // Set interrupt in IDT.               
                    Console.WriteLine("This runtime function (0x22) is not implemented!");
                    context.EAX = uint.MaxValue;
                    break;
                case 0x23: // Get interrupt in IDT.               
                    Console.WriteLine("This runtime function (0x23) is not implemented!");
                    context.EAX = uint.MaxValue;
                    break;
                case 0x24: // Set UNIX timestamp.                 
                    Console.WriteLine("This runtime function (0x24) is not implemented!");
                    context.EAX = uint.MaxValue;
                    break;
                case 0x25: // Get UNIX timestamp.                 
                    Console.WriteLine("This runtime function (0x25) is not implemented!");
                    context.EAX = uint.MaxValue;
                    break;
            }
        }
        internal static void DumpRegisters(INTs.IRQContext x)
        {
            uint eax = x.EAX; uint ebx = x.EBX; uint ecx = x.ECX; uint edx = x.EDX;
            uint esi = x.ESI; uint edi = x.EDI; uint eip = x.EIP; uint efl = (uint)x.EFlags;
            Console.WriteLine($"EAX = {eax:X}, EBX = {ebx:X}, ECX = {ecx:X}, EDX = {edx:X},");
            Console.WriteLine($"ESI = {esi:X}, EDI = {edi:X}, EIP = {eip:X}, EFL = {efl:X}");
        }
        internal static void DumpRegisters(Application x)
        {
            uint eax = x.RegisterEAX; uint ebx = x.RegisterEBX; uint ecx = x.RegisterECX; uint edx = x.RegisterEDX;
            uint esi = x.RegisterESI; uint edi = x.RegisterEDI; uint eip = x.RegisterEIP; uint efl = x.RegisterEFL;
            Console.WriteLine($"EAX = {eax:X}, EBX = {ebx:X}, ECX = {ecx:X}, EDX = {edx:X},");
            Console.WriteLine($"ESI = {esi:X}, EDI = {edi:X}, EIP = {eip:X}, EFL = {efl:X}");
        }
        internal static uint CreateStack() => Heap.SafeAlloc(DefaultStackSize);
        internal static void StartRaw(void* file)
        {
            Kernel.Log("Starting binary file...");
            TShellManager.LastErrorCode = InternalRun(file);
            Kernel.Log("Done running binary file!");
        }
        public static int InternalRun(void* ptr) { return 0; }
        [Plug(Target = typeof(LEF32File))]
        public class RuntimeImpl : AssemblerMethod
        {
            [PlugMethod(Assembler = typeof(Runtime))]
            public static int InternalRun(void* ptr) { return 0; }
            public override void AssembleNew(Assembler aAssembler, object aMethodInfo)
            {
                XS.Set(XSRegisters.EAX, XSRegisters.ESP, destinationDisplacement: 4);
                XS.Call(XSRegisters.EAX);
                XS.Return();
            }
        }
    }
}
