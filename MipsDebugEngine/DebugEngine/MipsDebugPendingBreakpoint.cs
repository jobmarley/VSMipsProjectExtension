using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;

using MipsRemoteDebuggerUtils;

namespace VSMipsProjectExtension.DebugEngine
{
	internal class MipsDebugPendingBreakpoint
		: IDebugPendingBreakpoint2
	{
		IDebugBreakpointRequest2 m_request = null;
		enum_PENDING_BP_STATE m_state = enum_PENDING_BP_STATE.PBPS_DISABLED;
		enum_PENDING_BP_STATE_FLAGS m_flags = enum_PENDING_BP_STATE_FLAGS.PBPSF_NONE;
		List<MipsDebugBoundBreakpoint> m_boundBreakpoints = new List<MipsDebugBoundBreakpoint>();
		List<MipsDebugErrorBreakpoint> m_errorBreakpoints = new List<MipsDebugErrorBreakpoint>();
		IElfSymbolProvider m_symbolProvider = null;
		IMipsDEEventCallback m_eventCallback = null;
		MipsDebugProgram m_program = null;
		bool m_bound = false;

		public MipsDebugProgram Program => m_program;
		public MipsDebugPendingBreakpoint(
			IDebugBreakpointRequest2 request,
			IElfSymbolProvider symbolProvider,
			IMipsDEEventCallback eventCallback,
			MipsDebugProgram program)
		{
			m_request = request;
			m_symbolProvider = symbolProvider;
			m_eventCallback = eventCallback;
			m_program = program;
		}
		public int CanBind(out IEnumDebugErrorBreakpoints2 ppErrorEnum)
		{
			throw new NotImplementedException();
		}

		public int Bind()
		{
			if (m_state == enum_PENDING_BP_STATE.PBPS_DELETED)
				return Constants.E_BP_DELETED;

			if (m_bound)
				return VSConstants.E_FAIL;

			BP_REQUEST_INFO[] infos = new BP_REQUEST_INFO[1];
			if (m_request.GetRequestInfo(enum_BPREQI_FIELDS.BPREQI_ALLFIELDS, infos) != VSConstants.S_OK)
				return VSConstants.E_FAIL;

			switch ((enum_BP_LOCATION_TYPE)infos[0].bpLocation.bpLocationType)
			{
				case enum_BP_LOCATION_TYPE.BPLT_CODE_FILE_LINE:
					{
						string context = infos[0].bpLocation.unionmember1 != IntPtr.Zero ? Marshal.PtrToStringBSTR(infos[0].bpLocation.unionmember1) : null;
						IDebugDocumentPosition2 documentPosition = infos[0].bpLocation.unionmember2 != IntPtr.Zero ? (IDebugDocumentPosition2)Marshal.GetObjectForIUnknown(infos[0].bpLocation.unionmember2) : null;

						IEnumDebugAddresses beg = null;
						IEnumDebugAddresses end = null;
						if (m_symbolProvider.GetAddressesFromPosition(documentPosition, 1, out beg, out end) == VSConstants.S_OK)
						{
							uint count = 0;
							if (beg.GetCount(out count) != VSConstants.S_OK)
								throw new Exception();
							IDebugAddress[] addresses = new IDebugAddress[count];
							uint fetched = 0;
							if (beg.Next((uint)addresses.Length, addresses, ref fetched) != VSConstants.S_OK)
								throw new Exception();


							foreach (IDebugAddress a in addresses)
							{
								MipsDebugBoundBreakpoint bp = new MipsDebugBoundBreakpoint(m_symbolProvider, m_program.RemoteClient, this, a);
								bp.Enable(m_state == enum_PENDING_BP_STATE.PBPS_ENABLED ? 1 : 0);
								m_boundBreakpoints.Add(bp);
							}

							m_eventCallback.SendBreakpointBoundEvent(this, new EnumDebugBoundBreakpoints2(m_boundBreakpoints));
						}
						else
						{
							MipsDebugErrorBreakpoint error = new MipsDebugErrorBreakpoint(this,
								new MipsDebugErrorBreakpointResolution(enum_BP_TYPE.BPT_CODE, m_program, null, "Unable to bind to breakpoint", enum_BP_ERROR_TYPE.BPET_SEV_GENERAL | enum_BP_ERROR_TYPE.BPET_TYPE_ERROR));
							m_errorBreakpoints.Add(error);
							m_eventCallback.SendBreakpointErrorEvent(error);
						}
						break;
					}
				default:
					break;
			}
			m_bound = true;
			return VSConstants.S_OK;
		}

		public int GetState(PENDING_BP_STATE_INFO[] pState)
		{
			if (pState.Length != 1)
				return VSConstants.E_INVALIDARG;

			pState[0].state = m_state;
			pState[0].Flags = m_flags;
			return VSConstants.S_OK;
		}

		public int GetBreakpointRequest(out IDebugBreakpointRequest2 ppBPRequest)
		{
			ppBPRequest = m_request;
			return VSConstants.S_OK;
		}

		public int Virtualize(int fVirtualize)
		{
			if (fVirtualize == 0)
			{
				m_flags |= enum_PENDING_BP_STATE_FLAGS.PBPSF_VIRTUALIZED;
			}
			else
			{
				m_flags &= ~enum_PENDING_BP_STATE_FLAGS.PBPSF_VIRTUALIZED;
			}
			return VSConstants.S_OK;
		}

		public int Enable(int fEnable)
		{
			if (m_state == enum_PENDING_BP_STATE.PBPS_DELETED)
				return Constants.E_BP_DELETED;

			if (fEnable != 0)
			{
				foreach (MipsDebugBoundBreakpoint bp in m_boundBreakpoints)
					bp.Enable(fEnable);

				m_state = enum_PENDING_BP_STATE.PBPS_ENABLED;
				return VSConstants.S_OK;
			}
			else
			{
				foreach (MipsDebugBoundBreakpoint bp in m_boundBreakpoints)
					bp.Enable(fEnable);

				m_state = enum_PENDING_BP_STATE.PBPS_DISABLED;
				return VSConstants.S_OK;
			}
		}

		public int SetCondition(BP_CONDITION bpCondition)
		{
			throw new NotImplementedException();
		}

		public int SetPassCount(BP_PASSCOUNT bpPassCount)
		{
			throw new NotImplementedException();
		}

		public int EnumBoundBreakpoints(out IEnumDebugBoundBreakpoints2 ppEnum)
		{
			ppEnum = new EnumDebugBoundBreakpoints2(m_boundBreakpoints);
			return VSConstants.S_OK;
		}

		public int EnumErrorBreakpoints(enum_BP_ERROR_TYPE bpErrorType, out IEnumDebugErrorBreakpoints2 ppEnum)
		{
			ppEnum = new EnumDebugErrorBreakpoints2(m_errorBreakpoints.Where(x => (x.ErrorType & bpErrorType) != 0));
			return VSConstants.S_OK;
		}

		public int Delete()
		{
			if (m_state == enum_PENDING_BP_STATE.PBPS_DELETED)
				return Constants.E_BP_DELETED;

			foreach (MipsDebugBoundBreakpoint bp in m_boundBreakpoints)
				bp.Delete();

			m_state = enum_PENDING_BP_STATE.PBPS_DELETED;
			return VSConstants.S_OK;
		}
	}
}
