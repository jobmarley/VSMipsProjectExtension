using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;

namespace FPGAProjectExtension.DebugEngine
{
	internal class MipsDebugThread
		: IDebugThread2
	{
		string m_name = null;
		MipsDebugProgram m_program = null;
		uint m_threadId = 0;

		public MipsDebugProgram Program => m_program;
		public MipsDebugThread(string name, MipsDebugProgram program)
		{
			m_name = name;
			m_program = program;
		}
		public int EnumFrameInfo(enum_FRAMEINFO_FLAGS dwFieldSpec, uint nRadix, out IEnumDebugFrameInfo2 ppEnum)
		{
			throw new NotImplementedException();
		}

		public int GetName(out string pbstrName)
		{
			pbstrName = m_name;
			return VSConstants.S_OK;
		}

		public int SetThreadName(string pszName)
		{
			m_name = pszName;
			return VSConstants.S_OK;
		}

		public int GetProgram(out IDebugProgram2 ppProgram)
		{
			ppProgram = m_program;
			return VSConstants.S_OK;
		}

		public int CanSetNextStatement(IDebugStackFrame2 pStackFrame, IDebugCodeContext2 pCodeContext)
		{
			return VSConstants.S_FALSE;
		}

		public int SetNextStatement(IDebugStackFrame2 pStackFrame, IDebugCodeContext2 pCodeContext)
		{
			throw new NotImplementedException();
		}

		public int GetThreadId(out uint pdwThreadId)
		{
			pdwThreadId = m_threadId;
			return VSConstants.S_OK;
		}

		public int Suspend(out uint pdwSuspendCount)
		{
			throw new NotImplementedException();
		}

		public int Resume(out uint pdwSuspendCount)
		{
			throw new NotImplementedException();
		}

		public int GetThreadProperties(enum_THREADPROPERTY_FIELDS dwFields, THREADPROPERTIES[] ptp)
		{
			throw new NotImplementedException();
		}

		public int GetLogicalThread(IDebugStackFrame2 pStackFrame, out IDebugLogicalThread2 ppLogicalThread)
		{
			throw new NotImplementedException();
		}
	}
}
