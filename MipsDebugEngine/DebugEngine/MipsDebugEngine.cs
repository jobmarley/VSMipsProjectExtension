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
	[Guid("AD8869A4-D013-4C71-ABDE-B33998A0CECA")]
	public class MipsDebugEngine
		: IDebugEngine2,
		IDebugEngineLaunch2
		//, ICustomQueryInterface
	{

		string m_registryRoot;
		Dictionary<string, object> m_metrics = new Dictionary<string, object>();
		/*public CustomQueryInterfaceResult GetInterface(ref Guid iid, out IntPtr ppv)
		{
			throw new NotImplementedException();
		}*/

		IDebugEventCallback2 m_callback = null;
		bool m_createEventSent = false;

		MipsDebugProcess m_debuggedProcess = null;
		MipsDebugProgram m_debuggedProgram = null;

		void SendEvent(MipsDebugProgramDestroyEvent e)
		{
			m_callback.Event(this,
				e.Program?.Process,
				e.Program,
				null,
				e,
				e.IID,
				e.Attributes);
		}
		void SendEvent(MipsDebugEngineCreateEvent e)
		{
			m_callback.Event(this,
				null,
				null,
				null,
				e,
				e.IID,
				e.Attributes);
		}
		void SendEvent(MipsDebugProcessCreateEvent e, MipsDebugProcess process)
		{
			m_callback.Event(this,
				process,
				process?.Programs?.FirstOrDefault(),
				process?.Programs?.FirstOrDefault()?.Thread,
				e,
				e.IID,
				e.Attributes);
		}
		void SendEvent(MipsDebugProgramCreateEvent e, IDebugProgram2 program)
		{
			IDebugProcess2 process = null;
			program.GetProcess(out process);
			m_callback.Event(this,
				process,
				program,
				null,
				e,
				e.IID,
				e.Attributes);
		}
		void SendEvent(MipsDebugThreadCreateEvent e, MipsDebugThread thread)
		{
			m_callback.Event(this,
				thread.Program.Process,
				thread.Program,
				thread,
				e,
				e.IID,
				e.Attributes);
		}
		void SendEvent(MipsDebugLoadCompleteEvent e, IDebugProgram2 program)
		{
			IDebugProcess2 process = null;
			program.GetProcess(out process);
			m_callback.Event(this,
				process,
				program,
				null,
				e,
				e.IID,
				e.Attributes);
		}
		void SendEvent(MipsDebugModuleLoadEvent e)
		{
			m_callback.Event(this,
				e.Module.Program.Process,
				e.Module.Program,
				null,
				e,
				e.IID,
				e.Attributes);
		}
		void SendEvent(MipsDebugEntryPointEvent e, IDebugThread2 thread)
		{
			IDebugProgram2 program;
			thread.GetProgram(out program);
			IDebugProcess2 process = null;
			program.GetProcess(out process);

			m_callback.Event(this,
				process,
				program,
				thread,
				e,
				e.IID,
				e.Attributes);
		}
		public int EnumPrograms(out IEnumDebugPrograms2 ppEnum)
		{
			throw new NotImplementedException();
		}

		public int Attach(IDebugProgram2[] rgpPrograms, IDebugProgramNode2[] rgpProgramNodes, uint celtPrograms, IDebugEventCallback2 pCallback, enum_ATTACH_REASON dwReason)
		{
			// rpgPrograms are VSDebug.dll!sdm::CDebugProgram
			m_callback = pCallback;
			if (!m_createEventSent)
			{
				SendEvent(new MipsDebugEngineCreateEvent(this));
				m_createEventSent = true;
			}

			MipsDebugProgram program = rgpPrograms[0] as MipsDebugProgram;
			//MipsDebugProgram program = rgpProgramNodes[0] as MipsDebugProgram;
			m_debuggedProgram = program;


			SendEvent(new MipsDebugProgramCreateEvent(), program);
			SendEvent(new MipsDebugLoadCompleteEvent(), program);

			//SendEvent(new MipsDebugModuleLoadEvent(program.LoadModule(program.Process.Filepath), true));
			//SendEvent(new MipsDebugThreadCreateEvent(), program.Thread);

			if (dwReason == enum_ATTACH_REASON.ATTACH_REASON_LAUNCH)
			{
				//SendEvent(new MipsDebugEntryPointEvent(), program.Thread);
			}

			return VSConstants.S_OK;
		}

		public int CreatePendingBreakpoint(IDebugBreakpointRequest2 pBPRequest, out IDebugPendingBreakpoint2 ppPendingBP)
		{
			throw new NotImplementedException();
		}

		public int SetException(EXCEPTION_INFO[] pException)
		{
			throw new NotImplementedException();
		}

		public int RemoveSetException(EXCEPTION_INFO[] pException)
		{
			throw new NotImplementedException();
		}

		public int RemoveAllSetExceptions(ref Guid guidType)
		{
			throw new NotImplementedException();
		}

		public int GetEngineId(out Guid pguidEngine)
		{
			pguidEngine = MipsDEGuids.EngineGuid;
			return VSConstants.S_OK;
		}

		public int DestroyProgram(IDebugProgram2 pProgram)
		{
			throw new NotImplementedException();
		}
		// Called by the SDM to indicate that a synchronous debug event, previously sent by the DE to the SDM,
		// was received and processed. The only event the sample engine sends in this fashion is Program Destroy.
		// It responds to that event by shutting down the engine.
		public int ContinueFromSynchronousEvent(IDebugEvent2 pEvent)
		{
			return VSConstants.S_OK;
		}

		public int SetLocale(ushort wLangID)
		{
			return VSConstants.S_OK;
		}

		public int SetRegistryRoot(string pszRegistryRoot)
		{
			m_registryRoot = pszRegistryRoot;
			return VSConstants.S_OK;
		}

		public int SetMetric(string pszMetric, object varValue)
		{
			// Should write that into the registry apparently
			m_metrics[pszMetric] = varValue;
			return VSConstants.S_OK;
		}

		public int CauseBreak()
		{
			throw new NotImplementedException();
		}

		public int LaunchSuspended(string pszServer,
			IDebugPort2 pPort,
			string pszExe,
			string pszArgs,
			string pszDir,
			string bstrEnv,
			string pszOptions,
			enum_LAUNCH_FLAGS dwLaunchFlags,
			uint hStdInput,
			uint hStdOutput,
			uint hStdError,
			IDebugEventCallback2 pCallback,
			out IDebugProcess2 ppProcess)
		{
			m_callback = pCallback;
			MipsDebugProcess process = new MipsDebugProcess(pszExe, pPort);
			ppProcess = process;
			m_debuggedProcess = process;

			MipsDebugProgram program = new MipsDebugProgram(process, "main program");
			process.AddProgram(program);

			/*if (!m_createEventSent)
			{
				SendEvent(new MipsDebugEngineCreateEvent(this));
				m_createEventSent = true;
			}*/

			// This doesnt do anything because the event sink in the port is not connected yet
			//MipsDebugPort mipsDebugPort = (MipsDebugPort)pPort;
			//mipsDebugPort.OnCreateProcess(process);

			//SendEvent(new MipsDebugProcessCreateEvent(), process);

			return VSConstants.S_OK;
		}

		public int ResumeProcess(IDebugProcess2 pProcess)
		{
			if (pProcess != m_debuggedProcess)
				return VSConstants.E_FAIL;

			IDebugPort2 port;
			int err = pProcess.GetPort(out port);
			IDebugDefaultPort2 defaultPort = (IDebugDefaultPort2)port;

			IDebugPortNotify2 portNotify;
			err = defaultPort.GetPortNotify(out portNotify);

			AD_PROCESS_ID[] pid = new AD_PROCESS_ID[1];
			pProcess.GetPhysicalProcessId(pid);
			string name;
			pProcess.GetName(enum_GETNAME_TYPE.GN_NAME, out name);
			err = portNotify.AddProgramNode((pProcess as MipsDebugProcess).Programs.FirstOrDefault());
			//err = portNotify.AddProgramNode(new MipsDebugProgramNode(pid[0], name));//(pProcess as MipsDebugProcess).Programs.FirstOrDefault());

			return VSConstants.S_OK;
		}

		public int CanTerminateProcess(IDebugProcess2 pProcess)
		{
			if (pProcess != m_debuggedProcess)
				return VSConstants.S_FALSE;
			return VSConstants.S_OK;
		}

		public int TerminateProcess(IDebugProcess2 pProcess)
		{
			// do nothing for now
			SendEvent(new MipsDebugProgramDestroyEvent(0, m_debuggedProgram));
			m_debuggedProcess = null;
			m_debuggedProgram = null;
			return VSConstants.S_OK;
		}
	}
}
