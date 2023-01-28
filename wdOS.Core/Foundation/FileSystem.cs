using Cosmos.System.FileSystem;
using Cosmos.System.FileSystem.VFS;
using System.Collections.Generic;
using System.IO;
using wdOS.Core.Foundation.FSHandlers;

namespace wdOS.Core.Foundation
{
    internal static class FileSystem
    {
        internal static CosmosVFS VFS = new();
        internal const string PrivateDir = "/.private";
        internal const string SystemTrash = PrivateDir + "/SystemTrash";
        internal const string UserTrashes = PrivateDir + "/UserTrashes";
        internal const string SystemDir = PrivateDir + "/System";
        internal const string UserDataDir = "/Users";
        internal static FileSystemHandler RootHandler;
        internal static void Initialize()
        {
            if (Kernel.SystemSettings.CDROMBoot)
            {
                RootHandler = new CDRamdiskFileSystemHandler();
            }
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
        internal static string[] GetEntryListing(string dirpath)
        {
            if (DirectoryExists(dirpath)) return RootHandler.GetEntryListing(dirpath);
            else return null;
        }
        internal static bool FileExists(string filepath) => VFSManager.FileExists(filepath);
        internal static bool DirectoryExists(string dirpath) => VFSManager.DirectoryExists(dirpath);
    }
    internal abstract class FileSystemHandler
    {
        internal abstract string Name { get; }
        internal abstract string Desc { get; }
        internal abstract int Version { get; }
        internal abstract string ReadStringFile(string filepath);
        internal abstract void WriteStringFile(string filepath, string data);
        internal abstract byte[] ReadBytesFile(string filepath);
        internal abstract void WriteBytesFile(string filepath, byte[] data);
        internal abstract void CreateDirectory(string dirpath);
        internal abstract void DeleteDirectory(string dirpath);
        internal abstract void DeleteFile(string filepath);
        internal abstract bool FileExists(string filepath);
        internal abstract bool DirectoryExists(string dirpath);
        internal abstract string[] GetEntryListing(string dirpath);
    }
}
