using Cosmos.System.Graphics.Fonts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace wdOS.Platform
{
    public unsafe static class ConfigurationManager
    {
        public const uint ConfigurationMagic = 0x55AA55AA;
        public const uint ConfigurationMaxSize = 128 * 1024;
        public static string SystemConfigrationDirectoryPath = "0:/PrivateSystem/config/";
        public static string SystemConfigrationFileName = "sysconf.bin";
        public static ConfigurationTableHeader* SystemConfigHeader;
        public static List<ConfigurationTableEntry> SystemConfigEntries = new();
        private static bool initialized = false;
        public static void Initialize()
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
            }
        }
        public static ConfigurationTableEntry[] LoadConfig(byte[] bytes)
        {
            var entries = new List<ConfigurationTableEntry>();
            ConfigurationTableHeader* header;

            fixed (byte* ptr = &bytes[0])
            {
                BinaryReader br = new(new MemoryStream(bytes));

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
        public static byte[] SaveConfig(ConfigurationTableEntry[] entries)
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
        public static void SaveSystemConfig()
        {
            var bytes = SaveConfig(SystemConfigEntries.ToArray());
            FileSystemManager.WriteBytesFile(SystemConfigrationDirectoryPath + SystemConfigrationFileName, bytes);
        }
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct ConfigurationTableHeader
    {
        public uint Magic;
        public uint Version;
        public uint SizeOfTable;
        public uint OffsetOfTable;
    }
    public struct ConfigurationTableEntry
    {
        public string Name;
        public string ContentString;
        public byte[] ContentBytes;
        public uint ContentNumber;
        public byte ContentType;
        public object Get() => ContentType switch
        {
            0 => ContentNumber,
            1 => ContentString,
            2 => ContentBytes,
            _ => null,
        };
	}
	public static class SystemSettings
	{
		public static int CrashPowerOffTimeout = 5;
		public static int SystemTerminalFont = 0;
		public static int ServicePeriod = 1000;
		public static bool EnableAudio = false;
		public static bool EnableLogging = true;
		public static bool EnableNetwork = false;
		public static bool EnableVerbose = false;
		public static bool EnableServices = false;
		public static bool EnablePeriodicGC = true;
		public static bool RamdiskAsRoot = false;
		public static int RamdiskSizeKB = 0;
		public static bool LoadUsersFromDisk = false;
		public static bool VerboseMode = false;
		public static bool LogIntoConsole = true;
		public static Dictionary<int, PCScreenFont> TerminalFonts = new()
		{
			[0] = PCScreenFont.Default
		};
	}
	public static class BuildConstants
	{
		public const int VersionMajor = 0;
		public const int VersionMinor = 11;
		public const int VersionPatch = 0;
		public const int CurrentOSType = TypePreBeta;
		public const int TypePreAlpha = 0;
		public const int TypeAlpha = 100;
		public const int TypePreBeta = 200;
		public const int TypeBeta = 300;
		public const int TypePreRelease = 400;
		public const int TypeRelease = 500;
		public static string GetDevStageName(int stage)
		{
			return stage switch
			{
				TypePreAlpha => "pre-alpha",
				TypeAlpha => "alpha",
				TypePreBeta => "pre-beta",
				TypeBeta => "beta",
				TypePreRelease => "pre-release",
				TypeRelease => "release",
				_ => "unknown"
			};
		}
	}
}
