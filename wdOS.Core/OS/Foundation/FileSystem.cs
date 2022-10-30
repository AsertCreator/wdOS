using Cosmos.System.FileSystem;
using Cosmos.System.FileSystem.VFS;
using System.IO;

namespace wdOS.Core.OS.Foundation
{
    internal static class FileSystem
    {
        internal static CosmosVFS VFS = new();
        internal const string PrivateDir = "0:\\.private";
        internal const string SystemTrash = PrivateDir + "\\SystemTrash";
        internal const string UserTrashes = PrivateDir + "\\UserTrashes";
        internal const string SystemDir = PrivateDir + "\\System";
        internal const string UserDataDir = "0:\\Users";
        internal static void Initialize()
        {
            // todo: initialize more things
            VFSManager.RegisterVFS(VFS, false);
        }
        internal static byte[] StreamFullRead(Stream stream)
        {
            byte[] buffer = new byte[stream.Length];
            _ = stream.Read(buffer, 0, (int)stream.Length);
            return buffer;
        }
        internal static void StreamFullWrite(Stream stream, byte[] data)
        {
            stream.Flush();
            stream.Write(data, 0, data.Length);
        }
        internal static string ReadStringFile(string filepath)
        {
            if (DirectoryExists(filepath)) return null;
            if (!FileExists(filepath)) { _ = VFSManager.CreateFile(filepath); }
            return new StreamReader(VFSManager.GetFileStream(filepath)).ReadToEnd();
        }
        internal static void WriteStringFile(string filepath, string data)
        {
            if (DirectoryExists(filepath)) return;
            if (!FileExists(filepath)) { _ = VFSManager.CreateFile(filepath); }
            new StreamWriter(VFSManager.GetFileStream(filepath)).WriteLine(data);
        }
        internal static byte[] ReadBytesFile(string filepath)
        {
            if (DirectoryExists(filepath)) return null;
            if (!FileExists(filepath)) { _ = VFSManager.CreateFile(filepath); }
            return StreamFullRead(VFSManager.GetFileStream(filepath));
        }
        internal static void WriteBytesFile(string filepath, byte[] data)
        {
            if (DirectoryExists(filepath)) return;
            if (!FileExists(filepath)) { _ = VFSManager.CreateFile(filepath); }
            StreamFullWrite(VFSManager.GetFileStream(filepath), data);
        }
        internal static void CreateDirectory(string dirpath)
        {
            if (DirectoryExists(dirpath)) return;
            if (!FileExists(dirpath)) { _ = VFSManager.CreateDirectory(dirpath); }
        }
        internal static void DeleteDirectory(string dirpath)
        {
            if (FileExists(dirpath)) return;
            if (DirectoryExists(dirpath)) { VFSManager.DeleteDirectory(dirpath, true); }
        }
        internal static void DeleteFile(string filepath)
        {
            if (DirectoryExists(filepath)) return;
            if (FileExists(filepath)) { VFSManager.DeleteFile(filepath); }
        }
        internal static bool FileExists(string filepath) => VFSManager.FileExists(filepath);
        internal static bool DirectoryExists(string dirpath) => VFSManager.DirectoryExists(dirpath);
    }
}
