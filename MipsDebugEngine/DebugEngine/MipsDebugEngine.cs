using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;

using MipsRemoteDebuggerUtils;

namespace VSMipsProjectExtension.DebugEngine
{
	[ComImport]
	[Guid("22F26F84-2D86-4835-A3E3-56C0837D0B0D")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	interface IMipsDEEventCallback
	{
		[MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall)]
		int SendEECompleteEvent(IDebugThread2 pThread, IDebugExpression2 pExpression, IDebugProperty2 pDebugProperty);
		[MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall)]
		int SendDebugBreakEvent(IDebugThread2 pThread);
		[MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall)]
		int SendPropertyCreateEvent(IDebugProperty2 pProperty);
	}

	[ComVisible(true)]
	[Guid("AD8869A4-D013-4C71-ABDE-B33998A0CECA")]
	public class MipsDebugEngine
		: IDebugEngine2,
		IDebugEngineLaunch2,
		IMipsDEEventCallback,
		IDebugIDECallback
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

		MipsRemoteDebuggerClient m_client = null;
		IDebugExpressionEvaluator2 m_expressionEvaluator = null;
		IElfSymbolProvider m_symbolProvider = null;

		public int SendPropertyCreateEvent(IDebugProperty2 pProperty)
		{
			MipsPropertyCreateEvent e = new MipsPropertyCreateEvent(pProperty);
			m_callback.Event(this,
				m_debuggedProgram.Process,
				m_debuggedProgram,
				null,
				e,
				e.IID,
				e.Attributes);
			return VSConstants.S_OK;
		}
		public int SendEECompleteEvent(IDebugThread2 pThread, IDebugExpression2 pExpression, IDebugProperty2 pDebugProperty)
		{
			var qzdqzd = System.Threading.Thread.CurrentThread.GetApartmentState();
			MipsDebugThread mt = pThread as MipsDebugThread;
			MipsDebugEECompleteEvent e = new MipsDebugEECompleteEvent(mt, pExpression, pDebugProperty);
			m_callback.Event(this,
				mt?.Program?.Process,
				mt?.Program,
				mt,
				e,
				e.IID,
				e.Attributes);
			return VSConstants.S_OK;
		}
		public int SendDebugBreakEvent(IDebugThread2 pThread)
		{
			MipsDebugThread mt = pThread as MipsDebugThread;
			MipsDebugBreakEvent e = new MipsDebugBreakEvent(mt);
			m_callback.Event(this,
				mt?.Program?.Process,
				mt?.Program,
				mt,
				e,
				e.IID,
				e.Attributes);
			return VSConstants.S_OK;
		}
		void SendEvent(MipsDebugProcessDestroyEvent e)
		{
			m_callback.Event(this,
				e.Process,
				null,
				null,
				e,
				e.IID,
				e.Attributes);
		}
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
		void SendEvent(MipsDebugProcessCreateEvent e)
		{
			m_callback.Event(this,
				e.Process,
				null,
				null,
				e,
				e.IID,
				e.Attributes);
		}
		void SendEvent(MipsDebugProgramCreateEvent e)
		{
			m_callback.Event(this,
				e.Program?.Process,
				e.Program,
				null,
				e,
				e.IID,
				e.Attributes);
		}
		void SendEvent(MipsDebugThreadCreateEvent e)
		{
			m_callback.Event(this,
				e.Thread?.Program?.Process,
				e.Thread?.Program,
				e.Thread,
				e,
				e.IID,
				e.Attributes);
		}
		void SendEvent(MipsDebugLoadCompleteEvent e)
		{
			m_callback.Event(this,
				e.Program?.Process,
				e.Program,
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

			m_client.OnMipsEvent += OnMipsEvent;

			//MipsDebugModule module = program.LoadModule(program.Process.Filepath);
			//SendEvent(new MipsDebugModuleLoadEvent(module, true));

			SendEvent(new MipsDebugProgramCreateEvent(program));
			SendEvent(new MipsDebugLoadCompleteEvent(program));



			//SendEvent(new MipsDebugModuleLoadEvent(program.LoadModule(program.Process.Filepath), true));
			//SendEvent(new MipsDebugThreadCreateEvent(), program.Thread);

			if (dwReason == enum_ATTACH_REASON.ATTACH_REASON_LAUNCH)
			{
				//SendEvent(new MipsDebugEntryPointEvent(), program.Thread);
			}
			return VSConstants.S_OK;
		}

		void OnMipsEvent(object sender, MipsEventArgs e)
		{
			MipsDebugProgram program = m_debuggedProgram;
			if (program == null)
				return;

			if (e.Event == md_event.Breakpoint)
			{
				program.OnBreak();
			}
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
			if (pProgram != m_debuggedProgram)
				return VSConstants.E_FAIL;

			// This is a notification.
			// We should cleanup everything program related and send the event
			SendEvent(new MipsDebugProgramDestroyEvent(0, m_debuggedProgram));
			m_debuggedProgram = null;
			return VSConstants.S_OK;
		}
		// Called by the SDM to indicate that a synchronous debug event, previously sent by the DE to the SDM,
		// was received and processed. The only event the sample engine sends in this fashion is Program Destroy.
		// It responds to that event by shutting down the engine.
		public int ContinueFromSynchronousEvent(IDebugEvent2 pEvent)
		{
			return VSConstants.S_OK;
		}
		ushort m_wLangID;
		public int SetLocale(ushort wLangID)
		{
			m_wLangID = wLangID;
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
			return VSConstants.E_NOTIMPL;
		}

		async Task<int> LoadFileInMemoryAsync(string filepath, uint offset)
		{
			try
			{
				System.IO.FileStream fs = new System.IO.FileStream(filepath, System.IO.FileMode.Open, System.IO.FileAccess.Read);
				byte[] buffer = new byte[fs.Length];
				if (await fs.ReadAsync(buffer, 0, buffer.Length) != buffer.Length)
					return VSConstants.E_FAIL;

				// We load the .elf at 0x10000, then the bootloader launches it
				if (await m_client.WriteMemoryAsync(buffer, offset) != buffer.Length)
					return VSConstants.E_FAIL;
			}
			catch (Exception)
			{
				return VSConstants.E_FAIL;
			}
			return VSConstants.S_OK;
		}
		Guid GetEECppCLSID()
		{
			string EECppGuid = "{3A12D0B7-C26C-11D0-B442-00A0244A1DD2}";
			string EEVendorGuid = "{994B45C4-E6E9-11D2-903F-00C04FA302A1}";
			Microsoft.Win32.RegistryKey EEKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(string.Format("{0}\\AD7Metrics\\ExpressionEvaluator\\{1}\\{2}", m_registryRoot, EECppGuid, EEVendorGuid));
			return new Guid((string)EEKey.GetValue("CLSID"));
		}
		VSComHelper.ActivationContext m_activationContext = null;
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
			ppProcess = null;

			m_callback = pCallback;

			// For some reason the manifest is not loaded correctly, so we need to do it manually with activation context
			string asmFilepath = System.Reflection.Assembly.GetExecutingAssembly().Location;
			string manifestFilepath = System.IO.Path.Combine(
				System.IO.Path.GetDirectoryName(asmFilepath),
				System.IO.Path.GetFileNameWithoutExtension(asmFilepath) + ".manifest");
			m_activationContext = new VSComHelper.ActivationContext(manifestFilepath);

			// Com object creation need to be wrapped in that so the CLSID can be found (since its not registered)
			using (var guard = m_activationContext.Activate())
			{
				Type t = Type.GetTypeFromCLSID(new Guid("305ae8e3-ce4d-4a0b-889c-9836bf4ddedf"));
				m_symbolProvider = (IElfSymbolProvider)Activator.CreateInstance(t);
				m_symbolProvider.SetEventCallback(this);
			}
			//m_expressionEvaluator = CreateExpressionEvaluator();

			// Delegate to port because its remote
			MipsRemoteDebugPort port = pPort as MipsRemoteDebugPort;
			m_client = port.Client;

			Func<string, uint> parseUInt = (s) => s.StartsWith("0x") ? uint.Parse(s.Substring(2), System.Globalization.NumberStyles.HexNumber) : uint.Parse(s);
			var filesToLoad = pszExe.Split(';').Select(x =>
			{
				var p = x.Split('|');
				return Tuple.Create(p[0], parseUInt(p[1]));
			}).ToArray();
			if (filesToLoad.Count() < 1)
				return VSConstants.E_INVALIDARG;

			int result = Microsoft.VisualStudio.Shell.ThreadHelper.JoinableTaskFactory.Run(async () =>
			{
				try
				{
					// Stop the processor if it's running
					await m_client.SetStateAsync(md_state.md_state_paused);

					// Load each file in memory at the given offset
					// Maybe we should check for overlap but...
					foreach (var (filepath, offset) in filesToLoad)
					{
						int r = await LoadFileInMemoryAsync(filepath, offset);
						if (r != VSConstants.S_OK)
							return r;
					}

					return VSConstants.S_OK;
				}
				catch (Exception)
				{
					return VSConstants.E_FAIL;
				}
			});

			if (result != VSConstants.S_OK)
				return result;

			// Create the process with the name of the first file
			MipsDebugProcess process = new MipsDebugProcess(filesToLoad.First().Item1, pPort);
			ppProcess = process;
			m_debuggedProcess = process;

			MipsDebugProgram program = new MipsDebugProgram(m_client, this, process, "main program", m_symbolProvider);
			process.AddProgram(program);

			// Load all modules asynchronously
			_ = Task.Run(async () =>
			{
				// the symbol provider is apartment and is not in the registry so it can only be accessed from the main thread
				await Microsoft.VisualStudio.Shell.ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
				foreach (var (fp, ofs) in filesToLoad)
					program.LoadModule(fp, ofs);
			});

			return VSConstants.S_OK;
		}

		public int ResumeProcess(IDebugProcess2 pProcess)
		{
			if (pProcess != m_debuggedProcess)
				return VSConstants.E_FAIL;

			try
			{
				IDebugPort2 port;
				int err = pProcess.GetPort(out port);
				IDebugDefaultPort2 defaultPort = (IDebugDefaultPort2)port;

				IDebugPortNotify2 portNotify;
				err = defaultPort.GetPortNotify(out portNotify);

				AD_PROCESS_ID[] pid = new AD_PROCESS_ID[1];
				pProcess.GetPhysicalProcessId(pid);
				string name;
				pProcess.GetName(enum_GETNAME_TYPE.GN_NAME, out name);

				MipsDebugProgram program = (pProcess as MipsDebugProcess).Programs.FirstOrDefault();
				// Notify port of program creation
				// Not too sure how to do that, but the port need to send a program created event
				// This is done asynchronously otherwise Attach is called before ResumeProcess returns, but who knows...
				// maybe its better to do it synchronously, so we can attach before we resume the program
				err = portNotify.AddProgramNode(program);

				program.Execute();

				// Create a thread
				_ = Task.Run(() => SendEvent(new MipsDebugThreadCreateEvent(program?.Threads.FirstOrDefault())));
				return VSConstants.S_OK;
			}
			catch (Exception)
			{
				return VSConstants.E_FAIL;
			}
		}

		public int CanTerminateProcess(IDebugProcess2 pProcess)
		{
			if (pProcess != m_debuggedProcess)
				return VSConstants.S_FALSE;
			return VSConstants.S_OK;
		}

		public int TerminateProcess(IDebugProcess2 pProcess)
		{
			if (pProcess != m_debuggedProcess)
				return VSConstants.E_FAIL;

			SendEvent(new MipsDebugProcessDestroyEvent(m_debuggedProcess));

			m_debuggedProcess = null;
			return VSConstants.S_OK;
		}

		public int DisplayMessage(string szMessage)
		{
			return VSConstants.S_OK;
		}
	}
}
