using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;

using MipsRemoteDebuggerUtils;

namespace VSMipsProjectExtension.DebugEngine
{
	class MipsDebugBreakpointResolution
		: IDebugBreakpointResolution2
	{
		enum_BP_TYPE m_type = enum_BP_TYPE.BPT_NONE;
		IDebugProgram2 m_program = null;
		IDebugThread2 m_thread = null;
		IElfSymbolProvider m_symbolProvider = null;
		IDebugAddress m_address = null;
		public enum_BP_TYPE Type => m_type;
		public MipsDebugBreakpointResolution(IElfSymbolProvider symbolProvider, enum_BP_TYPE type, IDebugProgram2 program, IDebugThread2 thread, IDebugAddress address)
		{
			m_symbolProvider = symbolProvider;
			m_type = type;
			m_program = program;
			m_thread = thread;
			m_address = address;
		}
		public int GetBreakpointType(enum_BP_TYPE[] pBPType)
		{
			if (pBPType?.Length != 1)
				return VSConstants.E_INVALIDARG;

			pBPType[0] = m_type;
			return VSConstants.S_OK;
		}

		public int GetResolutionInfo(enum_BPRESI_FIELDS dwFields, BP_RESOLUTION_INFO[] pBPResolutionInfo)
		{
			if (pBPResolutionInfo?.Length != 1)
				return VSConstants.E_INVALIDARG;

			pBPResolutionInfo[0].dwFields = 0;
			if ((dwFields & enum_BPRESI_FIELDS.BPRESI_PROGRAM) == enum_BPRESI_FIELDS.BPRESI_PROGRAM)
			{
				pBPResolutionInfo[0].pProgram = m_program;
				pBPResolutionInfo[0].dwFields |= enum_BPRESI_FIELDS.BPRESI_PROGRAM;
			}
			if ((dwFields & enum_BPRESI_FIELDS.BPRESI_THREAD) == enum_BPRESI_FIELDS.BPRESI_THREAD)
			{
				pBPResolutionInfo[0].pThread = m_thread;
				pBPResolutionInfo[0].dwFields |= enum_BPRESI_FIELDS.BPRESI_THREAD;
			}
			if ((dwFields & enum_BPRESI_FIELDS.BPRESI_BPRESLOCATION) == enum_BPRESI_FIELDS.BPRESI_BPRESLOCATION)
			{
				pBPResolutionInfo[0].bpResLocation.bpType = (uint)m_type;
				IDebugCodeContext2 codeContext = null;
				if (m_symbolProvider.GetCodeContextFromAddress(m_address, out codeContext) == VSConstants.S_OK)
					pBPResolutionInfo[0].dwFields |= enum_BPRESI_FIELDS.BPRESI_BPRESLOCATION;

				pBPResolutionInfo[0].bpResLocation.unionmember1 = Marshal.GetIUnknownForObject(codeContext);
			}

			return VSConstants.S_OK;
		}
	}
	internal class MipsDebugBoundBreakpoint
		: IDebugBoundBreakpoint2
	{
		IElfSymbolProvider m_symbolProvider = null;
		MipsDebugPendingBreakpoint m_debugBreakpoint = null;
		enum_BP_STATE m_state = enum_BP_STATE.BPS_NONE;
		uint m_hitCount = 0;
		uint m_oldInstruction = 0;
		uint m_oldInstruction2 = 0;
		IDebugAddress m_address = null;
		uint m_addressValue = 0;
		MipsRemoteDebuggerClient m_remoteClient = null;
		bool m_bound = false;
		bool m_bound2 = false;
		public MipsDebugBoundBreakpoint(
			IElfSymbolProvider symbolProvider,
			MipsRemoteDebuggerClient remoteClient,
			MipsDebugPendingBreakpoint pendingBreakpoint,
			IDebugAddress address)
		{
			m_symbolProvider = symbolProvider;
			m_remoteClient = remoteClient;
			m_debugBreakpoint = pendingBreakpoint;
			m_address = address;
			DEBUG_ADDRESS[] add = new DEBUG_ADDRESS[1];
			int hr = address.GetAddress(add);
			if (hr != VSConstants.S_OK)
				throw new COMException("", hr);

			m_addressValue = (uint)add[0].addr.unionmember.ToInt32();
		}
		public int GetPendingBreakpoint(out IDebugPendingBreakpoint2 ppPendingBreakpoint)
		{
			ppPendingBreakpoint = m_debugBreakpoint;
			return VSConstants.S_OK;
		}

		public int GetState(enum_BP_STATE[] pState)
		{
			if (pState?.Length != 1)
				return VSConstants.E_INVALIDARG;

			pState[0] = m_state;
			return VSConstants.S_OK;
		}

		public int GetHitCount(out uint pdwHitCount)
		{
			pdwHitCount = m_hitCount;
			return VSConstants.S_OK;
		}

		public int GetBreakpointResolution(out IDebugBreakpointResolution2 ppBPResolution)
		{
			ppBPResolution = new MipsDebugBreakpointResolution(m_symbolProvider, enum_BP_TYPE.BPT_CODE, m_debugBreakpoint.Program, m_debugBreakpoint.Program.Threads.FirstOrDefault(), m_address);
			return VSConstants.S_OK;
		}

		public int Enable(int fEnable)
		{
			if (m_state == enum_BP_STATE.BPS_DELETED)
				return Constants.E_BP_DELETED;

			if (fEnable != 0)
			{
				using (m_debugBreakpoint.Program.SafePause())
				{
					Microsoft.VisualStudio.Shell.ThreadHelper.JoinableTaskFactory.Run(async () =>
					{
						await BindAsync();
						m_state = enum_BP_STATE.BPS_ENABLED;
					});
				}
				return VSConstants.S_OK;
			}
			else
			{
				using (m_debugBreakpoint.Program.SafePause())
				{
					Microsoft.VisualStudio.Shell.ThreadHelper.JoinableTaskFactory.Run(async () =>
					{
						await UnbindAsync();
						await Unbind2Async();
						m_state = enum_BP_STATE.BPS_DISABLED;
					});
				}
				return VSConstants.S_OK;
			}
		}

		public int SetHitCount(uint dwHitCount)
		{
			m_hitCount = dwHitCount;
			return VSConstants.S_OK;
		}

		public int SetCondition(BP_CONDITION bpCondition)
		{
			throw new NotImplementedException();
		}

		public int SetPassCount(BP_PASSCOUNT bpPassCount)
		{
			throw new NotImplementedException();
		}

		public int Delete()
		{
			if (m_state != enum_BP_STATE.BPS_DELETED)
			{
				using (m_debugBreakpoint.Program.SafePause())
				{
					Microsoft.VisualStudio.Shell.ThreadHelper.JoinableTaskFactory.Run(async () =>
					{
						await UnbindAsync();
						await Unbind2Async();
					});
					m_state = enum_BP_STATE.BPS_DELETED;
				}
			}
			return VSConstants.S_OK;
		}

		/// <summary>
		/// Replace instruction by a breakpoint
		/// </summary>
		private async Task BindAsync()
		{
			if (m_bound)
				return;

			byte[] buffer = new byte[4];
			if (await m_remoteClient.ReadMemoryAsync(buffer, m_addressValue) != 4)
				throw new Exception();
			m_oldInstruction = BitConverter.ToUInt32(buffer, 0);
			buffer = BitConverter.GetBytes((uint)0x7000003F);
			await m_remoteClient.WriteMemoryAsync(buffer, m_addressValue);

			m_debugBreakpoint.Program.AddBoundBreakpoint(m_addressValue, this);
			m_bound = true;
		}
		/// <summary>
		/// Restore old instruction
		/// </summary>
		public async Task UnbindAsync()
		{
			if (!m_bound)
				return;

			byte[] buffer = new byte[4];
			if (await m_remoteClient.ReadMemoryAsync(buffer, m_addressValue) != 4)
				throw new Exception();
			uint instr = BitConverter.ToUInt32(buffer, 0);
			if (instr != 0x7000003F)
				throw new Exception();
			buffer = BitConverter.GetBytes(m_oldInstruction);
			await m_remoteClient.WriteMemoryAsync(buffer, m_addressValue);

			m_debugBreakpoint.Program.RemoveBoundBreakpoint(m_addressValue);
			m_bound = false;
		}

		private async Task Bind2Async()
		{
			if (m_bound2)
				return;

			byte[] buffer = new byte[4];
			if (await m_remoteClient.ReadMemoryAsync(buffer, m_addressValue + 4) != 4)
				throw new Exception();
			m_oldInstruction2 = BitConverter.ToUInt32(buffer, 0);
			buffer = BitConverter.GetBytes((uint)0x7000003F);
			await m_remoteClient.WriteMemoryAsync(buffer, m_addressValue + 4);

			m_debugBreakpoint.Program.AddBoundBreakpoint(m_addressValue + 4, this);
			m_bound2 = true;
		}
		private async Task Unbind2Async()
		{
			if (!m_bound2)
				return;

			byte[] buffer = new byte[4];
			if (await m_remoteClient.ReadMemoryAsync(buffer, m_addressValue + 4) != 4)
				throw new Exception();
			uint instr = BitConverter.ToUInt32(buffer, 0);
			if (instr != 0x7000003F)
				throw new Exception();
			buffer = BitConverter.GetBytes(m_oldInstruction2);
			await m_remoteClient.WriteMemoryAsync(buffer, m_addressValue + 4);

			m_debugBreakpoint.Program.RemoveBoundBreakpoint(m_addressValue + 4);
			m_bound2 = false;
		}
		public void OnBreak(uint pc)
		{
			if (pc - 4 == m_addressValue)
			{
				Microsoft.VisualStudio.Shell.ThreadHelper.JoinableTaskFactory.Run(async () =>
				{
					await UnbindAsync();
					await Bind2Async();
					await m_remoteClient.WriteRegisterAsync(md_register.md_register_pc, 0, m_addressValue);
				});
			}
			else if (pc - 4 == m_addressValue + 4)
			{
				Microsoft.VisualStudio.Shell.ThreadHelper.JoinableTaskFactory.Run(async () =>
				{
					await Unbind2Async();
					await BindAsync();
					await m_remoteClient.WriteRegisterAsync(md_register.md_register_pc, 0, m_addressValue + 4);
				});
			}
		}
	}
}
