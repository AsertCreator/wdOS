using Cosmos.System.Network.IPv4.TCP;
using System.Runtime.InteropServices;
using System.Threading;

namespace wdOS.Platform.Core
{
    public unsafe class ElfFile
    {
		public const uint Magic = 1179403647;
		public ElfHeader32 Header;
		public ElfProgramHeader32[] ProgramHeader;
		public ElfSectionHeader32[] SectionHeader;
		public byte* RawData;
	}
	[StructLayout(LayoutKind.Sequential)]
	public unsafe struct ElfHeader32
	{
		public uint ei_magic;
		public byte ei_class;
		public byte ei_data;
		public byte ei_version;
		public byte ei_osabi;
		public byte ei_abiversion;
		public fixed byte ei_pad[7];
		public ushort e_type;
		public ushort e_machine;
		public uint e_version;
		public uint e_entry;
		public uint e_phoff;
		public uint e_shoff;
		public uint e_flags;
		public ushort e_ehsize;
		public ushort e_phentsize;
		public ushort e_phnum;
		public ushort e_shentsize;
		public ushort e_shnum;
		public ushort e_shstrndx;
	}
	[StructLayout(LayoutKind.Sequential)]
	public unsafe struct ElfProgramHeader32
	{
		public const uint PT_NULL = 0;
		public const uint PT_LOAD = 1;
		public const uint PT_DYNAMIC = 2;
		public const uint PT_INTERP = 3;
		public const uint PT_NOTE = 4;
		public const uint PT_SHLIB = 5;
		public const uint PT_PHDR = 6;
		public const uint PT_TLS = 7;
		public const uint PF_X = 1;
		public const uint PF_W = 2;
		public const uint PF_R = 4;
		public uint p_type;
		public uint p_offset;
		public uint p_vaddr;
		public uint p_paddr;
		public uint p_filesz;
		public uint p_memsz;
		public uint p_flags;
		public uint p_align;
	}
	[StructLayout(LayoutKind.Sequential)]
	public unsafe struct ElfSectionHeader32
	{
		public const uint SHT_NULL = 0;
		public const uint SHT_PROGBITS = 1;
		public const uint SHT_SYMTAB = 2;
		public const uint SHT_STRTAB = 3;
		public const uint SHT_RELA = 4;
		public const uint SHT_HASH = 5;
		public const uint SHT_DYNAMIC = 6;
		public const uint SHT_NOTE = 7;
		public const uint SHT_NOBITS = 8;
		public const uint SHT_REL = 9;
		public const uint SHT_SHLIB = 10;
		public const uint SHT_DYNSYM = 11;
		public const uint SHT_INIT_ARRAY = 14;
		public const uint SHT_FINI_ARRAY = 15;
		public const uint SHT_PREINIT_ARRAY = 16;
		public const uint SHT_GROUP = 17;
		public const uint SHT_SYMTAB_SHNDX = 18;
		public const uint SHF_WRITE	= 0x1;
		public const uint SHF_ALLOC	= 0x2;
		public const uint SHF_EXECINSTR = 0x4;
		public const uint SHF_MERGE = 0x10;
		public const uint SHF_STRINGS = 0x20;
		public const uint SHF_INFO_LINK = 0x40;
		public const uint SHF_LINK_ORDER = 0x80;
		public const uint SHF_OS_NONCONFORMING = 0x100;
		public const uint SHF_GROUP = 0x200;
		public const uint SHF_TLS = 0x400;
		public const uint SHF_COMPRESSED = 0x800;
		public uint sh_name;
		public uint sh_type;
		public uint sh_flags;
		public uint sh_addr;
		public uint sh_offset;
		public uint sh_size;
		public uint sh_link;
		public uint sh_info;
		public uint sh_addralign;
		public uint sh_entsize;
	}
}
