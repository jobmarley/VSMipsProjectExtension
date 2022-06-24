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

		/*public CustomQueryInterfaceResult GetInterface(ref Guid iid, out IntPtr ppv)
		{
			throw new NotImplementedException();
		}*/

		public int EnumPrograms(out IEnumDebugPrograms2 ppEnum)
		{
			throw new NotImplementedException();
		}

		public int Attach(IDebugProgram2[] rgpPrograms, IDebugProgramNode2[] rgpProgramNodes, uint celtPrograms, IDebugEventCallback2 pCallback, enum_ATTACH_REASON dwReason)
		{
			throw new NotImplementedException();
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

		public int ContinueFromSynchronousEvent(IDebugEvent2 pEvent)
		{
			throw new NotImplementedException();
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
			throw new NotImplementedException();
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
			MipsDebugProcess process = new MipsDebugProcess(pszExe);
			ppProcess = process;

			MipsDebugProgram program = process.Program;

			pCallback.Event(this,
				process,
				program,
				program.Thread,
				new MipsDebugProcessCreateEvent(Constants.EVENT_IMMEDIATE | Constants.EVENT_SYNCHRONOUS),
				typeof(IDebugProcessCreateEvent2).GUID,
				Constants.EVENT_IMMEDIATE | Constants.EVENT_SYNCHRONOUS);

			return VSConstants.S_OK;
		}

		public int ResumeProcess(IDebugProcess2 pProcess)
		{
			return VSConstants.S_OK;
		}

		public int CanTerminateProcess(IDebugProcess2 pProcess)
		{
			throw new NotImplementedException();
		}

		public int TerminateProcess(IDebugProcess2 pProcess)
		{
			throw new NotImplementedException();
		}
	}
}
