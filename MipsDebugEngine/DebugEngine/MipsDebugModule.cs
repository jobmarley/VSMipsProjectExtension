using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;


namespace FPGAProjectExtension.DebugEngine
{
	internal class MipsDebugModule
		: IDebugModule2
	{
		public string Filepath { get; } = null;
		public MipsDebugProgram Program { get; } = null;
		public uint Address { get; } = 0;
		public uint Size { get; private set; } = 0;

		uint CalculateSize()
		{
			try
			{
				uint lower = uint.MaxValue;
				uint upper = uint.MinValue;
				FileStream fs = new FileStream(Filepath, FileMode.Open, FileAccess.Read);
				var header = ElfUtils.RawDeserialize<ElfHeader>(fs);
				if (header.magic == ElfUtils.ELF_MAGIC)
				{
					for (uint i = 0; i < header.e_phnum; i++)
					{
						uint ofs = header.e_phoff + i * header.e_phentsize;
						if (fs.Seek(ofs, SeekOrigin.Begin) != ofs)
							throw new Exception();

						var ph = ElfUtils.RawDeserialize<ElfProgramHeader>(fs);

						if ((ph.p_type & ElfUtils.PT_LOAD) == 0)
							continue;

						if (ph.p_memsz == 0)
							continue;

						lower = Math.Min(lower, ph.p_vaddr);
						upper = Math.Max(upper, ph.p_vaddr + ph.p_memsz);
					}
					return (upper >= lower) ? (upper - lower) : 0;
				}
			}
			catch (Exception)
			{

			}
			return (uint)new FileInfo(Filepath).Length;
		}
		public MipsDebugModule(MipsDebugProgram program, string filepath, uint address)
		{
			Program = program;
			Filepath = filepath;
			Address = address;
			Size = CalculateSize();
		}
		public int GetInfo(enum_MODULE_INFO_FIELDS dwFields, MODULE_INFO[] pinfo)
		{
			if (pinfo == null || pinfo.Length != 1)
				return VSConstants.E_INVALIDARG;

			if (dwFields.HasFlag(enum_MODULE_INFO_FIELDS.MIF_NAME))
			{
				pinfo[0].m_bstrName = System.IO.Path.GetFileName(Filepath);
				pinfo[0].dwValidFields |= enum_MODULE_INFO_FIELDS.MIF_NAME;
			}
			if (dwFields.HasFlag(enum_MODULE_INFO_FIELDS.MIF_URL))
			{
				pinfo[0].m_bstrUrl = Filepath;
				pinfo[0].dwValidFields |= enum_MODULE_INFO_FIELDS.MIF_URL;
			}
			if (dwFields.HasFlag(enum_MODULE_INFO_FIELDS.MIF_VERSION))
			{
				pinfo[0].m_bstrVersion = "1.0";
				pinfo[0].dwValidFields |= enum_MODULE_INFO_FIELDS.MIF_VERSION;
			}
			if (dwFields.HasFlag(enum_MODULE_INFO_FIELDS.MIF_DEBUGMESSAGE))
			{
				pinfo[0].m_bstrDebugMessage = "Symbols cannot be loaded";
				pinfo[0].dwValidFields |= enum_MODULE_INFO_FIELDS.MIF_DEBUGMESSAGE;
			}
			if (dwFields.HasFlag(enum_MODULE_INFO_FIELDS.MIF_LOADADDRESS))
			{
				pinfo[0].m_addrLoadAddress = Address;
				pinfo[0].m_addrPreferredLoadAddress = Address;
				pinfo[0].dwValidFields |= enum_MODULE_INFO_FIELDS.MIF_LOADADDRESS;
			}
			if (dwFields.HasFlag(enum_MODULE_INFO_FIELDS.MIF_SIZE))
			{
				pinfo[0].m_dwSize = Size;
				pinfo[0].dwValidFields |= enum_MODULE_INFO_FIELDS.MIF_SIZE;
			}
			if (dwFields.HasFlag(enum_MODULE_INFO_FIELDS.MIF_LOADORDER))
			{
				pinfo[0].m_dwLoadOrder = 0;
				pinfo[0].dwValidFields |= enum_MODULE_INFO_FIELDS.MIF_LOADORDER;
			}
			if (dwFields.HasFlag(enum_MODULE_INFO_FIELDS.MIF_TIMESTAMP))
			{
				pinfo[0].m_TimeStamp.dwLowDateTime = 0;
				pinfo[0].m_TimeStamp.dwHighDateTime = 0;
				pinfo[0].dwValidFields |= enum_MODULE_INFO_FIELDS.MIF_TIMESTAMP;
			}
			if (dwFields.HasFlag(enum_MODULE_INFO_FIELDS.MIF_URLSYMBOLLOCATION))
			{
				pinfo[0].m_bstrUrlSymbolLocation = "";//@".\";
				pinfo[0].dwValidFields |= enum_MODULE_INFO_FIELDS.MIF_URLSYMBOLLOCATION;
			}
			if (dwFields.HasFlag(enum_MODULE_INFO_FIELDS.MIF_FLAGS))
			{
				pinfo[0].m_dwModuleFlags = enum_MODULE_FLAGS.MODULE_FLAG_SYSTEM | enum_MODULE_FLAGS.MODULE_FLAG_UNOPTIMIZED;
				pinfo[0].dwValidFields |= enum_MODULE_INFO_FIELDS.MIF_FLAGS;
			}

			return VSConstants.S_OK;
		}

		public int ReloadSymbols_Deprecated(string pszUrlToSymbols, out string pbstrDebugMessage)
		{
			throw new NotImplementedException();
		}
	}
}
