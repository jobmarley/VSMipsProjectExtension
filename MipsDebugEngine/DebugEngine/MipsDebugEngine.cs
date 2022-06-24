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

		[ComRegisterFunction]
		public static void RegisterFunction(Type t)
		{
			// The assembly must be registered with regasm.exe
			// But it struggles with dependencies, so it can be used with some options to generate a reg file.
			// I managed to make it work with:
			// regasm FPGAProjectExtension.dll /regfile:"regfile.reg" /asmpath:"C:\Program Files\Microsoft Visual Studio\2022\Community\Common7\IDE\PublicAssemblies" /asmpath:"C:\Program Files\Microsoft Visual Studio\2022\Community\Common7\IDE\CommonExtensions\Microsoft\Project" /asmpath:"C:\Windows\Microsoft.Net\assembly\GAC_MSIL\System\v4.0_4.0.0.0__b77a5c561934e089"
			// WARNING: The .reg file doesnt contain the entries below

			// for details:
			// https://docs.microsoft.com/en-us/visualstudio/extensibility/debugger/reference/sdk-helpers-for-debugging?view=vs-2022
			string registrationRoot = "Software\\Microsoft\\VisualStudio\\17.0";

			Microsoft.Win32.RegistryKey engineKey = Microsoft.Win32.Registry.LocalMachine.CreateSubKey(string.Format("{0}\\AD7Metrics\\Engine\\{{{1}}}", registrationRoot, MipsDEGuids.EngineGuid.ToString()));
			engineKey.SetValue("Name", "My Debugger");
			engineKey.SetValue("CLSID", string.Format("{{{0}}}", typeof(MipsDebugEngine).GUID.ToString()));
			engineKey.SetValue("ProgramProvider", string.Format("{{{0}}}", typeof(MipsDebugProgramProvider).GUID.ToString()));

			Microsoft.Win32.RegistryKey portSupplierKey = Microsoft.Win32.Registry.LocalMachine.CreateSubKey(string.Format("{0}\\AD7Metrics\\PortSupplier\\{{{1}}}", registrationRoot, MipsDEGuids.PortSupplierGuid.ToString()));
			portSupplierKey.SetValue("Name", "Mips Port Supplier");
			portSupplierKey.SetValue("CLSID", string.Format("{{{0}}}", typeof(MipsDebugPortSupplier).GUID.ToString()));

			engineKey.Close();
			portSupplierKey.Close();
		}
		[ComUnregisterFunction]
		public static void UnregisterFunction(Type t)
		{
			string registrationRoot = "Software\\Microsoft\\VisualStudio\\17.0";
			Microsoft.Win32.Registry.LocalMachine.DeleteSubKey(string.Format("{0}\\AD7Metrics\\Engine\\{{{1}}}", registrationRoot, MipsDEGuids.EngineGuid.ToString()));
			Microsoft.Win32.Registry.LocalMachine.DeleteSubKey(string.Format("{0}\\AD7Metrics\\PortSupplier\\{{{1}}}", registrationRoot, MipsDEGuids.PortSupplierGuid.ToString()));
		}

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
