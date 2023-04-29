using Cosmos.HAL.BlockDevice;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wdOS.Platform
{
    internal unsafe class RamdiskBlockDevice : BlockDevice
    {
        internal byte* StartAddress;
        internal byte* EndAddress;
        internal bool IsReadOnly;
        public override BlockDeviceType Type => BlockDeviceType.HardDrive;
        public RamdiskBlockDevice(byte* startaddr, uint size)
        {
            StartAddress = startaddr;
            EndAddress = startaddr + size;
            mBlockSize = 512;
            mBlockCount = size / 512;
        }
        public override void ReadBlock(ulong aBlockNo, ulong aBlockCount, ref byte[] aData)
        {
            uint index = 0;
            for (uint i = (uint)(aBlockNo * 512); i < (aBlockCount + aBlockNo) * 512; i++)
                aData[index++] = StartAddress[i];
        }
        public override void WriteBlock(ulong aBlockNo, ulong aBlockCount, ref byte[] aData)
        {
            if (!IsReadOnly)
            {
                uint index = 0;
                for (uint i = (uint)(aBlockNo * 512); i < (aBlockCount + aBlockNo) * 512; i++)
                    StartAddress[i] = aData[index++];
            }
        }
    }
}
