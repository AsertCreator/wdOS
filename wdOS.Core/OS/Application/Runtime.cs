using Cosmos.Core;
using Cosmos.Core.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wdOS.Core.OS.Application
{
    internal static class Runtime
    {
        internal static Application[] CurrentApplications;
        internal static uint DefaultStackSize = 16 * 1024;
        internal static void Initialize() 
        {
            INTs.SetIntHandler(0xF0, HandleAPI);
            INTs.GeneralProtectionFault = delegate(ref INTs.IRQContext x) 
            {
                Console.BackgroundColor = ConsoleColor.Black;
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Current application tried to execute forbidden instruction");
                DumpRegisters(x);
                Console.WriteLine("This application will be terminated");
            };
        }
        internal static void 
        internal static void DumpRegisters(INTs.IRQContext x)
        {
            uint eax = x.EAX; uint ebx = x.EBX; uint ecx = x.ECX; uint edx = x.EDX;
            uint esi = x.ESI; uint edi = x.EDI; uint eip = x.EIP; uint efl = (uint)x.EFlags;
            Console.WriteLine($"EAX={eax:X},EBX={ebx:X},ECX={ecx:X},EDX={edx:X},");
            Console.WriteLine($"ESI={esi:X},EDI={edi:X},EIP={eip:X},EFL={efl:X}");
        } 
        internal static uint CreateStack() => Heap.SafeAlloc(DefaultStackSize);
        internal static void HandleAPI(ref INTs.IRQContext context) 
        {
            switch ((API)context.EDI)
            {
                case API.SysExit:
                    break;
                case API.SysRead:
                    break;
                case API.SysWrite:
                    break;
                case API.ShlSwitch:
                    break;
                case API.ShlSet:
                    break;
                case API.ShlGet:
                    break;
                case API.MemAlloc:
                    break;
                case API.MemFree:
                    break;
                case API.MemSet:
                    break;
                case API.PwrShutdown:
                    break;
                case API.PwrReset:
                    break;
                default:
                    break;
            }
        }
    }
    internal enum API : uint
    {
        SysExit, SysRead, SysWrite,
        ShlSwitch, ShlSet, ShlGet,
        MemAlloc, MemFree, MemSet,
        PwrShutdown, PwrReset
    }
    internal class PlainFile
    {
        internal byte[] Data;
        internal static PlainFile Parse(byte[] data) => new PlainFile() { Data = data };
    }
}
