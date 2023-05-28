using Cosmos.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace wdOS.Platform
{
    public unsafe static class HardwareManager
    {
        public static bool ACPIAvailable { get; private set; }
        public static bool ForceDisableACPI = false;
        private static bool initialized = false;
        public static void Initialize()
        {
            if (!initialized && !ForceDisableACPI)
            {
                var ptr = LocateRSDTPointer();
                ACPIAvailable = ptr.Available && VerifyACPITable(ptr.Version10->RSDTAddress);

                if (!ACPIAvailable)
                {
                    PlatformManager.Log("acpi is not available or was corrupted", "acpimanager", LogLevel.Warning);
                    initialized = true;
                    return;
                }

                // todo: implement cool acpi things that cosmos doesn't have

                initialized = true;
            }
        }
        public static bool VerifyACPITable(ACPITableHeader* header)
        {
            byte sum = 0;

            for (int i = 0; i < header->Length; i++)
                sum += ((byte*)header)[i];

            return sum == 0;
        }
        public static RSDTPointer LocateRSDTPointer()
        {
            RSDTPointer ptr = new();
            byte* end = (byte*)(0x00080000 + 1024);
            byte* rsdp = (byte*)0;

            // this is edba area
            for (byte* i = (byte*)0x00080000; i < end; i += 16)
            {
                if (i[0] == (byte)'R' && i[1] == (byte)'S' && i[2] == (byte)'D' && i[3] == (byte)' ' &&
                    i[4] == (byte)'P' && i[5] == (byte)'T' && i[6] == (byte)'R' && i[7] == (byte)' ')
                {
                    rsdp = i;
                    break;
                }
            }

            // if didn't find rsdp in ebda, find there
            if (rsdp == (byte*)0)
            {
                end = (byte*)0x000FFFFF;
                for (byte* i = (byte*)0x000E0000; i < end; i += 16)
                {
                    if (i[0] == (byte)'R' && i[1] == (byte)'S' && i[2] == (byte)'D' && i[3] == (byte)' ' &&
                        i[4] == (byte)'P' && i[5] == (byte)'T' && i[6] == (byte)'R' && i[7] == (byte)' ')
                    {
                        rsdp = i;
                        break;
                    }
                }
            }

            // if still didn't find, fail
            if (rsdp == (byte*)0)
            {
                ptr.Available = false;
                ptr.Revision = 0;
                ptr.Version10 = (RSDTPointer10*)0;
                ptr.Version20 = (RSDTPointer20*)0;
                return ptr;
            }
            else
            {
                // yay, we found acpi
                ptr.Available = true;
                ptr.Version10 = (RSDTPointer10*)rsdp;
                ptr.Version20 = (RSDTPointer20*)rsdp;
                ptr.Revision = ptr.Version10->ACPIVersion;
            }

            return ptr;
        }
        public static void ForceShutdownPC()
        {
            // todo: implement acpi shutdown
        }
        public static void ForceRestartPC()
        {
            // todo: implement acpi restart
        }
        public struct RSDTPointer
        {
            public bool Available;
            public RSDTPointer10* Version10;
            public RSDTPointer20* Version20;
            public int Revision;
        }
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct ACPITableHeader
        {
            public fixed char Signature[4];
            public uint Length;
            public byte Revision;
            public byte Checksum;
            public fixed char OEMID[6];
            public fixed char OEMTableID[8];
            public uint OEMRevision;
            public uint CreatorID;
            public uint CreatorRevision;
        }
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct RSDTPointer10
        {
            public fixed char Signature[8];
            public byte Checksum;
            public fixed char OEMID[6];
            public byte ACPIVersion;
            public ACPITableHeader* RSDTAddress;
        }
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct RSDTPointer20
        {
            public fixed char Signature[8];
            public byte Checksum;
            public fixed char OEMID[6];
            public byte ACPIVersion;
            public ACPITableHeader* RSDTAddress;
            public uint RSDTLength;
            public ulong XSDTAddress;
            public byte ExtendedChecksum;
            public fixed byte Reserved[3];
        }
    }
}
