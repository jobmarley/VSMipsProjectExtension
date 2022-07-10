using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;

using MipsRemoteDebuggerUtils;
using System.Runtime.InteropServices;

namespace FPGAProjectExtension.DebugEngine
{
	class MipsProcessorOperation
		: IMemoryOperation,
		IRegisterOperation
	{
		MipsRemoteDebuggerClient m_remoteClient = null;
		public MipsProcessorOperation(MipsRemoteDebuggerClient remoteClient)
		{
			m_remoteClient = remoteClient;
		}

		public int Read(byte[] buffer, uint offset, uint count, out uint pReadCount)
		{
			pReadCount = 0;
			try
			{
				pReadCount = Microsoft.VisualStudio.Shell.ThreadHelper.JoinableTaskFactory.Run(
					 async () => await m_remoteClient.ReadMemoryAsync(buffer, offset));
				return VSConstants.S_OK;
			}
			catch (Exception)
			{
			}
			return VSConstants.E_FAIL;
		}

		public int Read(uint index, out uint pValue)
		{
			pValue = 0;
			try
			{
				pValue = Microsoft.VisualStudio.Shell.ThreadHelper.JoinableTaskFactory.Run(async () => await m_remoteClient.ReadRegisterAsync((md_register)index));
				return VSConstants.S_OK;
			}
			catch(Exception)
			{
			}
			return VSConstants.E_FAIL;
		}

		public int Write(byte[] buffer, uint offset, uint count, out uint pWrittenCount)
		{
			pWrittenCount = 0;
			try
			{
				pWrittenCount = Microsoft.VisualStudio.Shell.ThreadHelper.JoinableTaskFactory.Run(
					 async () => await m_remoteClient.WriteMemoryAsync(buffer, offset));
				return VSConstants.S_OK;
			}
			catch (Exception)
			{
			}
			return VSConstants.E_FAIL;
		}

		public int Write(uint index, uint value)
		{
			try
			{
				Microsoft.VisualStudio.Shell.ThreadHelper.JoinableTaskFactory.Run(
					async () => await m_remoteClient.WriteRegisterAsync((md_register)index, value));
				return VSConstants.S_OK;
			}
			catch (Exception)
			{
			}
			return VSConstants.E_FAIL;
		}
	}
	internal class MipsDebugThread
		: IDebugThread2
	{
		public string Name { get; private set; } = null;
		public MipsDebugProgram Program { get; private set; } = null;
		public uint ID { get; private set; } = 0;
		private enum_THREADSTATE m_state = enum_THREADSTATE.THREADSTATE_FRESH;
		public enum_THREADSTATE State => (enum_THREADSTATE)m_state;

		List<IDebugStackFrame2> m_stackFrames = new List<IDebugStackFrame2>();
		public IEnumerable<IDebugStackFrame2> StackFrames => m_stackFrames;

		public uint SuspendCount { get; private set; } = 0;

		MipsRemoteDebuggerClient m_remoteClient = null;
		MipsProcessorOperation m_processorOp = null;
		IElfSymbolProvider m_symbolProvider = null;
		public MipsDebugThread(MipsRemoteDebuggerClient remoteClient, string name, MipsDebugProgram program, IElfSymbolProvider symbolProvider)
		{
			m_remoteClient = remoteClient;
			Random r = new Random();
			ID = (uint)r.Next();
			Name = name;
			Program = program;
			m_symbolProvider = symbolProvider;
			m_processorOp = new MipsProcessorOperation(remoteClient);
		}
		EnumDebugFrameInfo2 m_zozozo = null;
		EnumDebugFrameInfo2 m_zozozo2 = null;
		public int EnumFrameInfo(enum_FRAMEINFO_FLAGS dwFieldSpec, uint nRadix, out IEnumDebugFrameInfo2 ppEnum)
		{
			var fis = StackFrames.Select(x =>
			{
				FRAMEINFO[] fi = new FRAMEINFO[1];
				x.GetInfo(dwFieldSpec, nRadix, fi);
				return fi[0];
			}).ToArray();
			m_zozozo = new EnumDebugFrameInfo2(fis);
			m_zozozo2 = m_zozozo;
			ppEnum = m_zozozo;
			return VSConstants.S_OK;
		}

		public int GetName(out string pbstrName)
		{
			pbstrName = Name;
			return VSConstants.S_OK;
		}

		public int SetThreadName(string pszName)
		{
			Name = pszName;
			return VSConstants.S_OK;
		}

		public int GetProgram(out IDebugProgram2 ppProgram)
		{
			ppProgram = Program;
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
			pdwThreadId = ID;
			return VSConstants.S_OK;
		}

		// NOTE: It doesnt really makes sense to thread safe this. It should be synchronized at the application level
		public int Suspend(out uint pdwSuspendCount)
		{
			// Just change the state
			if (m_state != enum_THREADSTATE.THREADSTATE_RUNNING)
			{
				pdwSuspendCount = SuspendCount;
				return VSConstants.E_FAIL;
			}

			m_state = enum_THREADSTATE.THREADSTATE_STOPPED;
			pdwSuspendCount = ++SuspendCount;
			uint pc = Microsoft.VisualStudio.Shell.ThreadHelper.JoinableTaskFactory.Run(async () => await m_remoteClient.ReadRegisterAsync(md_register.md_register_pc));

			// We cannot return an error there, the thread is stopped already
			IDebugAddress address = null;
			int hr = m_symbolProvider.GetAddressFromMemory(pc, out address);
			if (hr != VSConstants.S_OK)
				return VSConstants.S_OK;

			// Get all stack frames 
			IDebugStackFrame2 stackFrame = null;
			hr = m_symbolProvider.GetStackFrame(address, this, m_processorOp, m_processorOp, out stackFrame);
			if (hr != VSConstants.S_OK)
				return VSConstants.S_OK;

			m_stackFrames.Add(stackFrame);

			while (stackFrame != null)
			{
				hr = m_symbolProvider.GetPreviousStackFrame(stackFrame, out stackFrame);
				if (hr != VSConstants.S_OK)
					break;

				m_stackFrames.Add(stackFrame);
			}
			return VSConstants.S_OK;
		}

		public int Resume(out uint pdwSuspendCount)
		{
			// Just change the state
			pdwSuspendCount = SuspendCount;
			if (m_state != enum_THREADSTATE.THREADSTATE_STOPPED &&
				m_state != enum_THREADSTATE.THREADSTATE_FRESH)
			{
				return VSConstants.E_FAIL;
			}

			m_state = enum_THREADSTATE.THREADSTATE_RUNNING;
			m_stackFrames.Clear();
			return VSConstants.S_OK;
		}

		public int GetThreadProperties(enum_THREADPROPERTY_FIELDS dwFields, THREADPROPERTIES[] ptp)
		{
			if (ptp == null || ptp.Length != 1)
				return VSConstants.E_INVALIDARG;

			if (dwFields.HasFlag(enum_THREADPROPERTY_FIELDS.TPF_ID))
			{
				ptp[0].dwThreadId = ID;
				ptp[0].dwFields |= enum_THREADPROPERTY_FIELDS.TPF_ID;
			}
			if (dwFields.HasFlag(enum_THREADPROPERTY_FIELDS.TPF_SUSPENDCOUNT))
			{
				ptp[0].dwSuspendCount = 1;
				ptp[0].dwFields |= enum_THREADPROPERTY_FIELDS.TPF_SUSPENDCOUNT;
			}
			if (dwFields.HasFlag(enum_THREADPROPERTY_FIELDS.TPF_STATE))
			{
				ptp[0].dwThreadState = (uint)State;
				ptp[0].dwFields |= enum_THREADPROPERTY_FIELDS.TPF_STATE;
			}
			if (dwFields.HasFlag(enum_THREADPROPERTY_FIELDS.TPF_PRIORITY))
			{
				ptp[0].bstrPriority = "Normal";
				ptp[0].dwFields |= enum_THREADPROPERTY_FIELDS.TPF_PRIORITY;
			}
			if (dwFields.HasFlag(enum_THREADPROPERTY_FIELDS.TPF_NAME))
			{
				ptp[0].bstrName = Name;
				ptp[0].dwFields |= enum_THREADPROPERTY_FIELDS.TPF_NAME;
			}
			if (dwFields.HasFlag(enum_THREADPROPERTY_FIELDS.TPF_LOCATION))
			{
				ptp[0].bstrLocation = "";
				ptp[0].dwFields |= enum_THREADPROPERTY_FIELDS.TPF_LOCATION;
			}
			return VSConstants.S_OK;
		}

		public int GetLogicalThread(IDebugStackFrame2 pStackFrame, out IDebugLogicalThread2 ppLogicalThread)
		{
			throw new NotImplementedException();
		}
	}
}
