using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;

using MipsRemoteDebuggerUtils;

namespace VSMipsProjectExtension.DebugEngine
{
	class MipsDebugErrorBreakpointResolution
		: IDebugErrorBreakpointResolution2
	{
		enum_BP_TYPE m_type = enum_BP_TYPE.BPT_NONE;
		IDebugProgram2 m_program = null;
		IDebugThread2 m_thread = null;
		string m_message = null;
		enum_BP_ERROR_TYPE m_errorType = 0;
		public enum_BP_TYPE Type => m_type;
		public enum_BP_ERROR_TYPE ErrorType => m_errorType;
		public MipsDebugErrorBreakpointResolution(enum_BP_TYPE type, IDebugProgram2 program, IDebugThread2 thread, string msg, enum_BP_ERROR_TYPE errorType)
		{
			m_type = type;
			m_program = program;
			m_thread = thread;
			m_message = msg;
			m_errorType = errorType;
		}
		public int GetBreakpointType(enum_BP_TYPE[] pBPType)
		{
			if (pBPType?.Length != 1)
				return VSConstants.E_INVALIDARG;

			pBPType[0] = m_type;
			return VSConstants.S_OK;
		}

		public int GetResolutionInfo(enum_BPERESI_FIELDS dwFields, BP_ERROR_RESOLUTION_INFO[] pErrorResolutionInfo)
		{
			if (pErrorResolutionInfo?.Length != 1)
				return VSConstants.E_INVALIDARG;

			if ((dwFields & enum_BPERESI_FIELDS.BPERESI_PROGRAM) == enum_BPERESI_FIELDS.BPERESI_PROGRAM)
				pErrorResolutionInfo[0].pProgram = m_program;
			if ((dwFields & enum_BPERESI_FIELDS.BPERESI_THREAD) == enum_BPERESI_FIELDS.BPERESI_THREAD)
				pErrorResolutionInfo[0].pThread = m_thread;
			if ((dwFields & enum_BPERESI_FIELDS.BPERESI_MESSAGE) == enum_BPERESI_FIELDS.BPERESI_MESSAGE)
				pErrorResolutionInfo[0].bstrMessage = m_message;
			if ((dwFields & enum_BPERESI_FIELDS.BPERESI_TYPE) == enum_BPERESI_FIELDS.BPERESI_TYPE)
				pErrorResolutionInfo[0].dwType = m_errorType;
			if ((dwFields & enum_BPERESI_FIELDS.BPERESI_BPRESLOCATION) == enum_BPERESI_FIELDS.BPERESI_BPRESLOCATION)
			{
				pErrorResolutionInfo[0].bpResLocation.bpType = (uint)m_type;
			}

			pErrorResolutionInfo[0].dwFields = dwFields;

			return VSConstants.S_OK;
		}
	}
	internal class MipsDebugErrorBreakpoint
		: IDebugErrorBreakpoint2
	{
		IDebugPendingBreakpoint2 m_pendingBreakpoint = null;
		MipsDebugErrorBreakpointResolution m_resolution = null;
		public enum_BP_TYPE Type => m_resolution.Type;
		public enum_BP_ERROR_TYPE ErrorType => m_resolution.ErrorType;
		public MipsDebugErrorBreakpoint(IDebugPendingBreakpoint2 pendingBreakpoint, MipsDebugErrorBreakpointResolution resolution)
		{
			m_pendingBreakpoint = pendingBreakpoint;
			m_resolution = resolution;
		}
		public int GetPendingBreakpoint(out IDebugPendingBreakpoint2 ppPendingBreakpoint)
		{
			ppPendingBreakpoint = m_pendingBreakpoint;
			return VSConstants.S_OK;
		}

		public int GetBreakpointResolution(out IDebugErrorBreakpointResolution2 ppErrorResolution)
		{
			ppErrorResolution = m_resolution;
			return VSConstants.S_OK;
		}
	}
}
