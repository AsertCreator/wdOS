﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wdOS.Platform.Core.Associations
{
	public unsafe class TarArchive : ArchiveBase
	{
		public static byte* TarFileIdentifier = (byte*)Utilities.ToCString("ustar");
		public override string Name { get; protected set; }
		public override string Type { get; protected set; }
		private byte[] FileContents;
		public TarArchive(string name, byte[] contents)
		{
			FileContents = contents;
			Name = name;
			Type = "USTAR";
		}
		public override bool FileExists(string path)
		{
			fixed (byte* aptr = &FileContents[0])
			{
				byte* result = (byte*)0;
				char* flname = Utilities.ToCString(path);
				int length = TarLookup(aptr, flname, &result);

				return length != -1;
			}
		}
		public override FileReadResult ReadFile(string path)
		{
			fixed (byte* aptr = &FileContents[0])
			{
				byte* result = (byte*)0;
				char* flname = Utilities.ToCString(path);
				int length = TarLookup(aptr, flname, &result);

				if (length != -1)
				{
					byte[] file = new byte[length];
					for (int i = 0; i < length; i++)
						file[i] = result[i];

					return new()
					{
						FileExists = true,
						Name = path,
						Result = file
					};
				}
				else
				{
					return new()
					{
						FileExists = false,
						Name = path,
						Result = null
					};
				}
			}
		}
		private int TarLookup(byte* archive, char* filename, byte** o)
		{
			byte* ptr = archive;

			while (!Utilities.StringCheck(ptr + 257, TarFileIdentifier, 5))
			{
				int filesize = Utilities.FromOctal(ptr + 0x7c, 11);
				if (!Utilities.StringCheck(ptr, (byte*)filename, Utilities.StringLength((byte*)filename) + 1))
				{
					*o = ptr + 512;
					return filesize;
				}
				ptr += ((filesize + 511) / 512 + 1) * 512;
			}
			return -1;
		}
	}
}
