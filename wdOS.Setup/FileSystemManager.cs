using Cosmos.System.FileSystem.VFS;
using Cosmos.System.FileSystem;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wdOS.Setup.Platform
{
    internal static class FileSystemManager
    {
        internal const string SystemDriveLabel = "wdOSDisk";
        internal static VFSBase VFS = new CosmosVFS();
        internal static List<FileDevice> Devices = new();
        private static bool initialized = false;
        internal static void Initialize()
        {
            if (!initialized)
            {
                VFSManager.RegisterVFS(VFS, false);

                PlatformLogger.Log("set up basic file system!", "filesystemmanager");
                initialized = true;
            }
        }
        internal static string Normalize(string path)
        {
            string result = path.Replace('/', '\\').Replace("\\\\", "\\");
            if (result.EndsWith("\\")) result = result.Remove(result.Length - 2);
            return result;
        }
        internal static byte[] StreamFullRead(Stream stream)
        {
            byte[] buffer = new byte[stream.Length];
            stream.Read(buffer);
            return buffer;
        }
        internal static void StreamFullWrite(Stream stream, byte[] data)
        {
            stream.Flush(); stream.Write(data);
        }
        internal static string ReadStringFile(string filepath)
        {
            if (!FileExists(filepath)) return null;
            var stream = VFSManager.GetFileStream(filepath);
            var result = Encoding.ASCII.GetString(StreamFullRead(stream));
            stream.Close();
            return result;
        }
        internal static void WriteStringFile(string filepath, string data)
        {
            if (!FileExists(filepath)) { VFSManager.CreateFile(filepath); }
            var stream = VFSManager.GetFileStream(filepath);
            StreamFullWrite(stream, Encoding.ASCII.GetBytes(data));
            stream.Close();
        }
        internal static byte[] ReadBytesFile(string filepath)
        {
            if (!FileExists(filepath)) return null;
            var stream = VFSManager.GetFileStream(filepath);
            var result = StreamFullRead(stream);
            stream.Close();
            return result;
        }
        internal static void WriteBytesFile(string filepath, byte[] data)
        {
            if (!FileExists(filepath)) { VFSManager.CreateFile(filepath); }
            var stream = VFSManager.GetFileStream(filepath);
            StreamFullWrite(stream, data);
            stream.Close();
        }
        internal static void CreateDevice(FileDevice device)
        {
            if (device != null) { Devices.Add(device); return; }
            throw new ArgumentNullException(nameof(device));
        }
        internal static void CreateDirectory(string dirpath)
        {
            if (!DirectoryExists(dirpath) && !FileExists(dirpath))
                VFSManager.CreateDirectory(dirpath);
        }
        internal static void DeleteDirectory(string dirpath)
        {
            if (DirectoryExists(dirpath) && !FileExists(dirpath))
                VFSManager.DeleteDirectory(dirpath, true);
        }
        internal static void DeleteFile(string filepath)
        {
            if (FileExists(filepath) && !DirectoryExists(filepath))
                VFSManager.DeleteFile(filepath);
        }
        internal static bool FileExists(string filepath) => VFSManager.FileExists(filepath);
        internal static bool DirectoryExists(string dirpath) => VFSManager.DirectoryExists(dirpath);
    }
    internal abstract class FileDevice
    {
        internal string FriendlyName;
        internal string DevicePath;
        internal Func<int, byte[]> Read;
        internal Action<byte[]> Write;
    }
    internal struct FileReadResult
    {
        internal string Name;
        internal bool FileExists;
        internal byte[] Result;
    }
}
