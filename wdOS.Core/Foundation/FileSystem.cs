using Cosmos.System.FileSystem;
using Cosmos.System.FileSystem.VFS;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace wdOS.Core.Foundation
{
    public static class FileSystem
    {
        public static CosmosVFS VFS = new();
        public static List<FileDevice> Devices = new();
        public static void Initialize()
        {
            VFSManager.RegisterVFS(VFS);
        }
        public static string Normalize(string path) 
        {
            string result = path.Replace('/', '\\').Replace("\\\\", "\\");
            if (result.EndsWith("\\")) result.Remove(result.Length - 2);
            return result;
        } 
        public static byte[] StreamFullRead(Stream stream)
        {
            byte[] buffer = new byte[stream.Length];
            stream.Read(buffer);
            return buffer;
        }
        public static void StreamFullWrite(Stream stream, byte[] data)
        {
            stream.Flush(); stream.Write(data);
        }
        public static string ReadStringFile(string filepath)
        {
            if (!FileExists(filepath)) return null;
            var stream = VFSManager.GetFileStream(filepath);
            var result = Encoding.ASCII.GetString(StreamFullRead(stream));
            stream.Close();
            return result;
        }
        public static void WriteStringFile(string filepath, string data)
        {
            if (!FileExists(filepath)) { VFSManager.CreateFile(filepath); }
            var stream = VFSManager.GetFileStream(filepath);
            StreamFullWrite(stream, Encoding.ASCII.GetBytes(data));
            stream.Close();
        }
        public static byte[] ReadBytesFile(string filepath)
        {
            if (!FileExists(filepath)) return null;
            var stream = VFSManager.GetFileStream(filepath);
            var result = StreamFullRead(stream);
            stream.Close();
            return result;
        }
        public static void WriteBytesFile(string filepath, byte[] data)
        {
            if (!FileExists(filepath)) { VFSManager.CreateFile(filepath); }
            var stream = VFSManager.GetFileStream(filepath);
            StreamFullWrite(stream, data);
            stream.Close();
        }
        public static void CreateDevice(FileDevice device)
        {
            if (device != null) { Devices.Add(device); return; }
            throw new ArgumentNullException(nameof(device));
        }
        public static void CreateDirectory(string dirpath)
        {
            if (!DirectoryExists(dirpath) && !FileExists(dirpath)) 
                VFSManager.CreateDirectory(dirpath);
        }
        public static void DeleteDirectory(string dirpath)
        {
            if (DirectoryExists(dirpath) && !FileExists(dirpath)) 
                VFSManager.DeleteDirectory(dirpath, true);
        }
        public static void DeleteFile(string filepath)
        {
            if (FileExists(filepath) && !DirectoryExists(filepath)) 
                VFSManager.DeleteFile(filepath);
        }
        public static bool FileExists(string filepath) => VFSManager.FileExists(filepath);
        public static bool DirectoryExists(string dirpath) => VFSManager.DirectoryExists(dirpath);
    }
    public abstract class FileDevice
    {
        public string FriendlyName;
        public string DevicePath;
        public Func<int, byte[]> Read;
        public Action<byte[]> Write;
    }
}
