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
		string m_filepath = null;
		public MipsDebugProgram Program { get; } = null;
		public MipsDebugModule(MipsDebugProgram program, string filepath)
		{
			Program = program;
			m_filepath = filepath;
		}
		public int GetInfo(enum_MODULE_INFO_FIELDS dwFields, MODULE_INFO[] pinfo)
		{
			throw new NotImplementedException();
		}

		public int ReloadSymbols_Deprecated(string pszUrlToSymbols, out string pbstrDebugMessage)
		{
			throw new NotImplementedException();
		}
	}
}
