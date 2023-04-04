using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wdOS.Core.Foundation
{
    public class Application
    {
        public static uint NextPID = 0;
        public uint PID = NextPID++;
        public bool IsRunning;
        public bool IsRunningElevated;
        public bool EnableNXProtection;
        public bool EnableUserMode;
        public BinarySection[] Sections;
        public uint Entrypoint;
    }
    public struct Context
    {
        public uint EAX;
        public uint EBX;
        public uint ECX;
        public uint EDX;
        public uint ESI;
        public uint EDI;
        public uint ESP;
        public uint EBP;
        public uint EFL;
    }
    public unsafe struct BinarySection
    {
        public uint PhysicalAddress;
        public uint VirtualAddress;
        public uint Size;
        public bool IsCode;
        public bool IsData;
        public byte* Data;
    }
}
