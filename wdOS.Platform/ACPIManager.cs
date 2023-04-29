using Cosmos.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace wdOS.Platform
{
    internal unsafe static class ACPIManager
    {
        internal static bool IsAvailable { get; private set; }
        internal static bool ForceDisable = false;
        internal static bool initialized = false;
        internal static void Initialize()
        {
            if (!initialized && !ForceDisable)
            {
                var ptr = LocateRSDTPointer();
                IsAvailable = ptr.Available && VerifyACPITable(ptr.Version10->RSDTAddress);

                if (!IsAvailable)
                {
                    PlatformLogger.Log("acpi is not available or was corrupted", "acpimanager", LogLevel.Warning);
                    initialized = true;
                    return;
                }

                // todo: implement cool acpi things that cosmos doesn't have

                initialized = true;
            }
        }
        internal static bool VerifyACPITable(ACPITableHeader* header)
        {
            byte sum = 0;

            for (int i = 0; i < header->Length; i++)
                sum += ((byte*)header)[i];

            return sum == 0;
        }
        internal static RSDTPointer LocateRSDTPointer()
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
        internal static void ForceShutdownPC()
        {

        }
        internal static void ForceRestartPC()
        {

        }
        internal struct RSDTPointer
        {
            internal bool Available;
            internal RSDTPointer10* Version10;
            internal RSDTPointer20* Version20;
            internal int Revision;
        }
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        internal struct ACPITableHeader
        {
            internal fixed char Signature[4];
            internal uint Length;
            internal byte Revision;
            internal byte Checksum;
            internal fixed char OEMID[6];
            internal fixed char OEMTableID[8];
            internal uint OEMRevision;
            internal uint CreatorID;
            internal uint CreatorRevision;
        }
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        internal struct RSDTPointer10
        {
            internal fixed char Signature[8];
            internal byte Checksum;
            internal fixed char OEMID[6];
            internal byte ACPIVersion;
            internal ACPITableHeader* RSDTAddress;
        }
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        internal struct RSDTPointer20
        {
            internal fixed char Signature[8];
            internal byte Checksum;
            internal fixed char OEMID[6];
            internal byte ACPIVersion;
            internal ACPITableHeader* RSDTAddress;
            internal uint RSDTLength;
            internal ulong XSDTAddress;
            internal byte ExtendedChecksum;
            internal fixed byte Reserved[3];
        }
    }
}
