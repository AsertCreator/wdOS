using System.Collections.Generic;
using System.IO;

namespace wdOS.Core.Foundation
{
    internal static class FileSystem
    {
        internal static string RootDir = "/";
        internal static string SystemDir = RootDir + "System/";
        internal static string SystemSettingsFile = RootDir + "System/SystemSettings.wpr";
        internal static string UsersDir = RootDir + "Users/";
        internal static void Initialize()
        {
            // nope
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
            if (!FileExists(filepath)) { File.Create(filepath).Close(); }
            return new StreamReader(File.OpenRead(filepath)).ReadToEnd();
        }
        internal static void WriteStringFile(string filepath, string data)
        {
            if (DirectoryExists(filepath)) return;
            if (!FileExists(filepath)) { File.Create(filepath).Close(); }
            new StreamWriter(File.OpenWrite(filepath)).WriteLine(data);
        }
        internal static byte[] ReadBytesFile(string filepath)
        {
            if (DirectoryExists(filepath)) return null;
            if (!FileExists(filepath)) { File.Create(filepath).Close(); }
            return StreamFullRead(File.OpenRead(filepath));
        }
        internal static void WriteBytesFile(string filepath, byte[] data)
        {
            if (DirectoryExists(filepath)) return;
            if (!FileExists(filepath)) { File.Create(filepath).Close(); }
            StreamFullWrite(File.OpenRead(filepath), data);
        }
        internal static void CreateDirectory(string dirpath)
        {
            if (DirectoryExists(dirpath)) return;
            if (!FileExists(dirpath)) { Directory.CreateDirectory(dirpath); }
        }
        internal static void DeleteDirectory(string dirpath)
        {
            if (FileExists(dirpath)) return;
            if (DirectoryExists(dirpath)) { Directory.Delete(dirpath, true); }
        }
        internal static void DeleteFile(string filepath)
        {
            if (DirectoryExists(filepath)) return;
            if (FileExists(filepath)) { File.Delete(filepath); }
        }
        internal static bool FileExists(string filepath) => File.Exists(filepath);
        internal static bool DirectoryExists(string dirpath) => Directory.Exists(dirpath);
    }
}
