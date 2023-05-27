using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace wdOS.Platform
{
    internal unsafe static class ConfigurationManager
    {
        internal const uint ConfigurationMagic = 0x55AA55AA;
        internal const uint ConfigurationMaxSize = 128 * 1024;
        internal static string SystemConfigrationDirectoryPath = "0:/PrivateSystem/config/";
        internal static string SystemConfigrationFileName = "sysconf.bin";
        internal static ConfigurationTableHeader* SystemConfigHeader;
        internal static List<ConfigurationTableEntry> SystemConfigEntries = new();
        private static byte[] reference;
        private static bool initialized = false;
        internal static bool Initialize()
        {
            if (!initialized)
            {
                if (!FileSystemManager.FileExists(SystemConfigrationDirectoryPath + SystemConfigrationFileName))
                {
                    Console.WriteLine("no system configuration found! consider using debugshell to create one");
                    return;
                }

                var entries = LoadConfig(FileSystemManager.ReadBytesFile(SystemConfigrationDirectoryPath + SystemConfigrationFileName));
                SystemConfigEntries = entries.ToList();

                initialized = true;
                return true;
            }
            return false;
        }
        internal static ConfigurationTableEntry[] LoadConfig(byte[] bytes)
        {
            var entries = new List<ConfigurationTableEntry>();
            ConfigurationTableHeader* header;

            fixed (byte* ptr = &reference[0])
            {
                BinaryReader br = new(new MemoryStream(reference));

                br.ReadInt64(); br.ReadInt64();
                header = (ConfigurationTableHeader*)ptr;
                if (header->Magic != ConfigurationMagic ||
                   header->Version != 1) return null;

                uint ci = 0;
                StringBuilder sb = new();
                for (uint i = 0; i < header->SizeOfTable; i++)
                {
                    ConfigurationTableEntry res = new();
                    byte* entry = ptr + ci;
                    byte entrytype = br.ReadByte();
                    byte entrynmsz = br.ReadByte();
                    byte entryctsz = br.ReadByte();
                    byte entryflag = br.ReadByte();

                    sb.Append(br.ReadChars(entrynmsz));

                    res.Name = sb.ToString();

                    switch (entrytype)
                    {
                        case 0:
                            res.ContentNumber = br.ReadUInt32();
                            break;
                        case 1:
                            sb.Clear();
                            sb.Append(br.ReadChars(entryctsz));
                            res.ContentString = sb.ToString();
                            break;
                        case 2:
                            res.ContentBytes = br.ReadBytes(entryctsz);
                            break;
                    }
                    res.ContentType = entrytype;

                    entries.Add(res);
                    sb.Clear();
                }
            }

            return entries.ToArray();
        }
        internal static byte[] SaveConfig(ConfigurationTableEntry[] entries)
        {
            byte[] bytes = new byte[ConfigurationMaxSize];
            BinaryWriter bw = new(new MemoryStream(bytes));

            bw.Write(ConfigurationMagic);
            bw.Write(1);
            bw.Write(entries.Length);
            bw.Write(0);

            for (int i = 0; i < entries.Length; i++)
            {
                var entry = entries[i];
                bw.Write(entry.ContentString != null);
                bw.Write((byte)entry.Name.Length);
                bw.Write(entry.ContentType);
                bw.Write((byte)0);
                switch (entry.ContentType)
                {
                    case 0: bw.Write(entry.ContentNumber); break;
                    case 1: bw.Write(entry.ContentString.ToCharArray()); break;
                    case 2: bw.Write(entry.ContentBytes); break;
                    default: bw.Write(0); break;
                }
                if (entry.ContentString != null) bw.Write(entry.ContentString.ToCharArray());
                else bw.Write(entry.ContentNumber);
            }

            return bytes[0..new((int)bw.BaseStream.Position)];
        }
        internal static void SaveSystemConfig()
        {
            var bytes = SaveConfig(SystemConfigEntries.ToArray());
            FileSystemManager.WriteBytesFile(SystemConfigrationDirectoryPath + SystemConfigrationFileName, bytes);
        }
    }
    [StructLayout(LayoutKind.Sequential)]
    internal struct ConfigurationTableHeader
    {
        internal uint Magic;
        internal uint Version;
        internal uint SizeOfTable;
        internal uint OffsetOfTable;
    }
    internal struct ConfigurationTableEntry
    {
        internal string Name;
        internal string ContentString;
        internal byte[] ContentBytes;
        internal uint ContentNumber;
        internal byte ContentType;
        internal object Get() => ContentType switch
        {
            0 => ContentNumber,
            1 => ContentString,
            2 => ContentBytes,
            _ => null,
        };
    }
}
