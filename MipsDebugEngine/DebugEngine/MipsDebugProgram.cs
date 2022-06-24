using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;

namespace FPGAProjectExtension.DebugEngine
{
	[ComVisible(true)]
	[Guid("972617D7-8244-4FB7-9870-4062B6086D5D")]
	internal class MipsDebugProgram
		: IDebugProgram2
	{
		MipsDebugProcess m_process = null;
		string m_name = null;
		Guid m_guid = Guid.Empty;

		public MipsDebugThread Thread { get; private set; }
		public MipsDebugProgram(MipsDebugProcess process, string name)
		{
			m_process = process;
			m_name = name;
			m_guid = Guid.NewGuid();
			Thread = new MipsDebugThread("main thread", this);
		}
		public int EnumThreads(out IEnumDebugThreads2 ppEnum)
		{
			throw new NotImplementedException();
		}

		public int GetName(out string pbstrName)
		{
			pbstrName = m_name;
			return VSConstants.S_OK;
		}

		public int GetProcess(out IDebugProcess2 ppProcess)
		{
			ppProcess = m_process;
			return VSConstants.S_OK;
		}

		public int Terminate()
		{
			throw new NotImplementedException();
		}

		public int Attach(IDebugEventCallback2 pCallback)
		{
			throw new NotImplementedException();
		}

		public int CanDetach()
		{
			throw new NotImplementedException();
		}

		public int Detach()
		{
			return VSConstants.S_OK;
		}

		public int GetProgramId(out Guid pguidProgramId)
		{
			pguidProgramId = m_guid;
			return VSConstants.S_OK;
		}

		public int GetDebugProperty(out IDebugProperty2 ppProperty)
		{
			throw new NotImplementedException();
		}

		public int Execute()
		{
			throw new NotImplementedException();
		}

		public int Continue(IDebugThread2 pThread)
		{
			throw new NotImplementedException();
		}

		public int Step(IDebugThread2 pThread, enum_STEPKIND sk, enum_STEPUNIT Step)
		{
			throw new NotImplementedException();
		}

		public int CauseBreak()
		{
			throw new NotImplementedException();
		}

		public int GetEngineInfo(out string pbstrEngine, out Guid pguidEngine)
		{
			throw new NotImplementedException();
		}

		public int EnumCodeContexts(IDebugDocumentPosition2 pDocPos, out IEnumDebugCodeContexts2 ppEnum)
		{
			throw new NotImplementedException();
		}

		public int GetMemoryBytes(out IDebugMemoryBytes2 ppMemoryBytes)
		{
			throw new NotImplementedException();
		}

		public int GetDisassemblyStream(enum_DISASSEMBLY_STREAM_SCOPE dwScope, IDebugCodeContext2 pCodeContext, out IDebugDisassemblyStream2 ppDisassemblyStream)
		{
			throw new NotImplementedException();
		}

		public int EnumModules(out IEnumDebugModules2 ppEnum)
		{
			throw new NotImplementedException();
		}

		public int GetENCUpdate(out object ppUpdate)
		{
			throw new NotImplementedException();
		}

		public int EnumCodePaths(string pszHint, IDebugCodeContext2 pStart, IDebugStackFrame2 pFrame, int fSource, out IEnumCodePaths2 ppEnum, out IDebugCodeContext2 ppSafety)
		{
			throw new NotImplementedException();
		}

		public int WriteDump(enum_DUMPTYPE DUMPTYPE, string pszDumpUrl)
		{
			throw new NotImplementedException();
		}
	}
}
