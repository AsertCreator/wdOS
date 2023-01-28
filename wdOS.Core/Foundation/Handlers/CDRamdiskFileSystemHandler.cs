using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wdOS.Core.Foundation.FSHandlers
{
    internal class CDRamdiskFileSystemHandler : FileSystemHandler
    {
        internal override string Name => "CDBoot RAMDisk";
        internal override string Desc => "Used when wdOS is booted from CD";
        internal override int Version => 50;
        internal static List<RamdiskEntry> Entries;
        internal override void CreateDirectory(string dirpath) { throw new NotSupportedException(); }
        internal override void DeleteDirectory(string dirpath) { throw new NotSupportedException(); }
        internal override void DeleteFile(string filepath) { throw new NotSupportedException(); }
        internal override bool DirectoryExists(string dirpath)
        {

        }
        internal override bool FileExists(string filepath)
        {
            throw new NotImplementedException();
        }
        internal override byte[] ReadBytesFile(string filepath)
        {
            throw new NotImplementedException();
        }
        internal override string ReadStringFile(string filepath)
        {
            throw new NotImplementedException();
        }
        internal override void WriteBytesFile(string filepath, byte[] data)
        {
            throw new NotImplementedException();
        }
        internal override void WriteStringFile(string filepath, string data)
        {
            throw new NotImplementedException();
        }
        internal override string[] GetEntryListing(string dirpath)
        {
            throw new NotImplementedException();
        }
    }
    internal class Entry
}
