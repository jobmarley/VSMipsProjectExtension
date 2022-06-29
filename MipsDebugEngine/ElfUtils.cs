using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.InteropServices;

namespace FPGAProjectExtension
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	struct ElfHeader
	{
		public uint magic;
		public byte file_class;
		public byte encoding;
		public byte version;
		public byte abi;
		public byte abi_version;
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 7)]
		public byte[] padding;
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
	};
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	struct ElfProgramHeader
	{
		public uint p_type;
		public uint p_offset;
		public uint p_vaddr;
		public uint p_paddr;
		public uint p_filesz;
		public uint p_memsz;
		public uint p_flags;
		public uint p_align;
	};

	internal class ElfUtils
	{
		public const uint ELF_MAGIC = 0x464C457F;
		public static T RawDeserialize<T>(byte[] rawData)
		{
			int rawsize = Marshal.SizeOf(typeof(T));
			if (rawsize > rawData.Length)
				throw new Exception();
			IntPtr buffer = Marshal.AllocHGlobal(rawsize);
			Marshal.Copy(rawData, 0, buffer, rawsize);
			T retobj = (T)Marshal.PtrToStructure(buffer, typeof(T));
			Marshal.FreeHGlobal(buffer);
			return retobj;
		}
		public static T RawDeserialize<T>(Stream s)
		{
			int size = Marshal.SizeOf<T>();
			byte[] buffer = new byte[size];
			if (s.Read(buffer, 0, buffer.Length) != buffer.Length)
				throw new Exception();
			return RawDeserialize<T>(buffer);
		}
	}
}
