using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;


namespace FPGAProjectExtension.DebugEngine
{
	internal class MipsDebugModule
		: IDebugModule2
	{
		public string Filepath { get; } = null;
		public MipsDebugProgram Program { get; } = null;
		public MipsDebugModule(MipsDebugProgram program, string filepath)
		{
			Program = program;
			Filepath = filepath;
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
				pinfo[0].m_addrLoadAddress = 0x10000;
				pinfo[0].m_addrPreferredLoadAddress = 0x10000;
				pinfo[0].dwValidFields |= enum_MODULE_INFO_FIELDS.MIF_LOADADDRESS;
			}
			if (dwFields.HasFlag(enum_MODULE_INFO_FIELDS.MIF_SIZE))
			{
				pinfo[0].m_dwSize = 0x10000;
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
