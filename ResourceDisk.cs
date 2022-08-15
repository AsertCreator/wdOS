using System.Collections.Generic;
using System.IO;
using System.Text;
using static System.BitConverter;

namespace wdOS
{
    public class ResourceDisk
    {
        public string Label = "RDFS Temp Disk";
        public int Version = VERSION;
        public List<RDFile> AllFiles = new();
        public const int MAGIC = 93864327;
        public const int VERSION = 1;
        public const string EXTENSION = "rdif";
        public ResourceDisk(string label) { Label = label; }
        public RDFile Address(string path)
        {
            string realpath = Canonical(path);
            foreach (var item in AllFiles)
            {
                if (item.Name == path)
                {
                    return item;
                }
            }
            return null;
        }
        public static string Canonical(string path)
        {
            List<string> strings = new();
            string[] paths = path.Split('/');
            paths = Cut(paths, 1, paths.Length);
            foreach (var p in paths)
            {
                if (p == ".." && strings.Count > 0) { strings.RemoveAt(strings.Count - 1); }
                else if (p == ".") { }
                else { strings.Add(p); }
            }
            return Path.Combine(strings.ToArray());
        }
        public static byte[] Save(ResourceDisk rd)
        {
            List<byte> bytes = new();
            bytes.AddRange(GetBytes(MAGIC));
            bytes.AddRange(GetBytes(VERSION));
            bytes.AddRange(GetBytes(rd.Label.Length));
            bytes.AddRange(STRUtils.GetBytes(rd.Label));
            bytes.AddRange(GetBytes(rd.AllFiles.Count));
            foreach (var file in rd.AllFiles)
            {
                bytes.AddRange(file.Save());
            }
            return bytes.ToArray();
        }
        public static T[] Cut<T>(T[] arr, int index, int index2)
        {
            List<T> bytes = new();
            for (int i = index; i < index2; i++)
            {
                bytes.Add(bytes[i]);
            }
            return bytes.ToArray();
        }
        public static ResourceDisk Parse(byte[] data)
        {
            ResourceDisk rd = new("");
            if (ToInt32(Cut(data, 0, 4)) == MAGIC)
            {
                rd.Version = ToInt32(Cut(data, 4, 8));
                int length = ToInt32(Cut(data, 8, 12));
                int index = 12;
                rd.Label = STRUtils.GetString(Cut(data, index, index + length));
                index += length;
                int count = ToInt32(Cut(data, index, index + 4));
                index += 4;
                for (int i = 0; i < count; i++)
                {
                    RDFile file = new();
                    int length1 = ToInt32(Cut(data, index, index + 4));
                    index += 4;
                    file.Name = STRUtils.GetString(Cut(data, index, index + length1));
                    index += length1;
                    int length2 = ToInt32(Cut(data, index, index + 4));
                    index += 4;
                    List<byte> bytes = new();
                    for (int o = 0; o < length2; o++)
                    {
                        bytes.Add(data[index]);
                        index++;
                    }
                    file.WriteBytes(bytes.ToArray());
                    rd.AllFiles.Add(file);
                }
                return rd;
            }
            else
            {
                return rd;
            }
        }
    }
    public class RDFile
    {
        public string Name;
        private byte[] Data;
        public byte[] ReadAsBytes() => Data;
        public string ReadAsString() => Encoding.UTF8.GetString(Data);
        public byte[] Save()
        {
            List<byte> bytes = new();
            bytes.AddRange(GetBytes(Name.Length));
            bytes.AddRange(STRUtils.GetBytes(Name));
            bytes.AddRange(GetBytes(Data.Length));
            bytes.AddRange(Data);
            return bytes.ToArray();
        }
        public void WriteBytes(byte[] data) => Data = data;
        public void WriteString(string str) => Data = Encoding.UTF8.GetBytes(str);
    }
}
