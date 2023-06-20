using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wdOS.Platform.Core
{
    public abstract class ArchiveBase
    {
        public abstract string Name { get; protected set; }
        public abstract string Type { get; protected set; }
        public abstract bool FileExists(string filename);
        public abstract FileReadResult ReadFile(string filename);
    }
    public struct FileReadResult
    {
        public string Name;
        public bool FileExists;
        public byte[] Result;
    }
}
