using Cosmos.Core;
using Cosmos.HAL.BlockDevice;
using Cosmos.System.FileSystem;
using Cosmos.System.FileSystem.Listing;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using wdOS.Core.OS.Shells;

namespace wdOS.Core.OS.FileSystems
{
    // block chained file system
    // no pun intended (it is not cryptocurrency-based)
    internal class BCFS : FileSystem
    {
        public override long AvailableFreeSpace => TotalFreeSpace;
        public override long TotalFreeSpace => throw new NotImplementedException();
        public override string Type => throw new NotImplementedException();
        public override string Label { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        internal string CurrentRootPath;
        internal BCFSHeader CurrentHeader;
        internal Partition CurrentDevice;
        internal const int BCFSv1Magic = 0x0100BCF5;
        internal const int BCFSv1MaxLabelLength = 32;
        internal const int BCFSv1MaxPathLength = 256;
        public BCFS(Partition aDevice, string aRootPath, long aSize) : base(aDevice, aRootPath, aSize)
        {
            CurrentDevice = aDevice;
            CurrentRootPath = aRootPath;

        }
        public override DirectoryEntry CreateDirectory(DirectoryEntry aParentDirectory, string aNewDirectory) { throw new NotImplementedException(); }
        public override DirectoryEntry CreateFile(DirectoryEntry aParentDirectory, string aNewFile) { throw new NotImplementedException(); }
        public override void DeleteDirectory(DirectoryEntry aPath) { throw new NotImplementedException(); }
        public override void DeleteFile(DirectoryEntry aPath) { throw new NotImplementedException(); }
        public override void DisplayFileSystemInfo() { throw new NotImplementedException(); }
        public override unsafe void Format(string aDriveFormat, bool aQuick) 
        { 
            if (aDriveFormat == "BCFS")
            {
                // do quick please
                if (!aQuick) 
                { CurrentDevice.WriteBlock(0, CurrentDevice.BlockCount, ref Utilities.ZeroBytes); }
                // restore header
                {
                    BCFSHeader hdr = new();
                    hdr.bcfs_Hdr_FirstFreeBlock = 1;
                    hdr.bcfs_Hdr_Magic = BCFSv1Magic;
                    hdr.bcfs_Hdr_FilesystemLabel = $"BCFS partition {new Random().Next()}";
                    byte[] hdrb = Utilities.FromStructure(hdr);
                    CurrentDevice.WriteBlock(0, 1, ref hdrb);
                    CurrentHeader = hdr;
                    GCImplementation.Free(hdr);
                    GCImplementation.Free(hdrb);
                }
            }
            else
            {
                Console.WriteLine("What else you can format this drive to?");
            }
        }
        internal uint AllocBlock()
        {
            uint block = CurrentHeader.bcfs_Hdr_FirstFreeBlock;
            uint index = 0;
            bool found = false;
            while (!found || index < uint.MaxValue)
            {
                if(block.)
                index++;
            }
        }
        internal void FreeBlock(uint point)
        {
            CurrentDevice.WriteBlock(CurrentHeader.bcfs_Hdr_FirstFreeBlock, 1, ref Utilities.ZeroBytes);
            CurrentDevice.WriteBlock(point, 1, ref Utilities.ZeroBytes);
            if (CurrentHeader.bcfs_Hdr_FirstFreeBlock > point) 
                CurrentHeader.bcfs_Hdr_FirstFreeBlock = point;
        }
        public override List<DirectoryEntry> GetDirectoryListing(DirectoryEntry baseDirectory) { throw new NotImplementedException(); }
        public override DirectoryEntry GetRootDirectory() { throw new NotImplementedException(); }
    }
    [StructLayout(LayoutKind.Sequential)]
    internal struct BCFSHeader
    {
        internal uint bcfs_Hdr_Magic;
        internal uint bcfs_Hdr_FirstFreeBlock;
        internal uint bcfs_Hdr_FileCount;
        internal uint bcfs_Hdr_FirstFilePointer;
        internal uint bcfs_Hdr_LastFilePointer;
        internal uint bcfs_Hdr_DirCount;
        internal uint bcfs_Hdr_FirstDirPointer;
        internal uint bcfs_Hdr_LastDirPointer;
        [MarshalAs(UnmanagedType.LPStr, SizeConst = BCFS.BCFSv1MaxLabelLength)]
        internal string bcfs_Hdr_FilesystemLabel;
        internal uint bcfs_Hdr_MBRBackupPointer;
        internal BCFSPropertiesFlags bcfs_Hdr_Properties;
    }
    [StructLayout(LayoutKind.Sequential)]
    internal struct BCFSMBRBackup
    {
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 446)]
        internal byte[] bcfs_MBRBack_Code;
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 16)]
        internal byte[] bcfs_MBRBack_Partition0;
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 16)]
        internal byte[] bcfs_MBRBack_Partition1;
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 16)]
        internal byte[] bcfs_MBRBack_Partition2;
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 16)]
        internal byte[] bcfs_MBRBack_Partition3;
        internal ushort bcfS_MBRBack_Signature;
    }
    [StructLayout(LayoutKind.Sequential)]
    internal struct BCFSFileDescriptor
    {
        internal uint bcfs_File_PreviousFileAddress;
        internal uint bcfs_File_NextFileAddress;
        internal uint bcfs_File_FileBackupAddress;
        internal uint bcfs_File_FileDataAddress;
        [MarshalAs(UnmanagedType.LPStr, SizeConst = BCFS.BCFSv1MaxPathLength)]
        internal string bcfs_File_Path;
        internal uint bcfs_File_ByteSize;
        internal uint bcfs_File_CreationTime;
        internal uint bcfs_File_AccessedTime;
        internal uint bcfs_File_ModifiedTime;
        internal BCFSAccessFlags bcfs_File_AccessFlags;
    }
    [StructLayout(LayoutKind.Sequential)]
    internal struct BCFSFileBackupDescriptor
    {
        internal uint bcfs_FileBack_ByteSize;
        internal uint bcfs_FileBack_DataAddress;
        internal uint bcfs_FileBack_CreationTime;
        internal uint bcfs_FileBack_AccessedTime;
        internal uint bcfs_FileBack_ModifiedTime;
    }
    [StructLayout(LayoutKind.Sequential)]
    internal struct BCFSDirectoryDescriptor
    {
        internal uint bcfs_Dir_PreviousDirAddress;
        internal uint bcfs_Dir_NextDirAddress;
        internal uint bcfs_Dir_FirstFilePointer;
        internal uint bcfs_Dir_LastFilePointer;
        [MarshalAs(UnmanagedType.LPStr, SizeConst = BCFS.BCFSv1MaxPathLength)]
        internal string bcfs_Dir_Path;
        internal uint bcfs_Dir_CreationTime;
        internal uint bcfs_Dir_AccessedTime;
        internal uint bcfs_Dir_ModifiedTime;
        internal BCFSAccessFlags bcfs_Dir_AccessFlags;
    }
    internal enum BCFSAccessFlags : uint
    {
        Read = 1, Write = 2, Execute = 4, UserDelete = 8
    }
    internal enum BCFSPropertiesFlags : uint
    {
        Readonly = 1, BackupMBRCode = 2, BackupMBRTable = 4, BackupFiles = 8
    }
}
