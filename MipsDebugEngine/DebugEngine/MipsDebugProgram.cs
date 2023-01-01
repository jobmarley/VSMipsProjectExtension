/*
 Copyright (C) 2022 jobmarley

 This file is part of VSMipsProjectExtension.

 This program is free software; you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation; either version 3 of the License, or (at your option) any later version.

 This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

 You should have received a copy of the GNU General Public License along with Foobar. If not, see <https://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;

using MipsRemoteDebuggerUtils;
using System.Security.AccessControl;
using System.Threading;
using Microsoft.VisualStudio.Shell;

namespace VSMipsProjectExtension.DebugEngine
{
	class MipsDebugProgramNode
		: IDebugProgramNode2
	{
		string m_name;
		AD_PROCESS_ID m_pid;
		public MipsDebugProgramNode(AD_PROCESS_ID pid, string name)
		{
			m_pid = pid;
			m_name = name;
		}
		public int GetProgramName(out string pbstrProgramName)
		{
			pbstrProgramName = m_name;
			return VSConstants.S_OK;
		}

		public int GetHostName(enum_GETHOSTNAME_TYPE dwHostNameType, out string pbstrHostName)
		{
			pbstrHostName = m_name;
			return VSConstants.S_OK;
		}

		public int GetHostPid(AD_PROCESS_ID[] pHostProcessId)
		{
			pHostProcessId[0] = m_pid;
			return VSConstants.S_OK;
		}

		public int GetHostMachineName_V7(out string pbstrHostMachineName)
		{
			throw new NotImplementedException();
		}

		public int Attach_V7(IDebugProgram2 pMDMProgram, IDebugEventCallback2 pCallback, uint dwReason)
		{
			throw new NotImplementedException();
		}

		public int GetEngineInfo(out string pbstrEngine, out Guid pguidEngine)
		{
			pbstrEngine = "MipsDebugger";
			pguidEngine = MipsDEGuids.EngineGuid;
			return VSConstants.S_OK;
		}

		public int DetachDebugger_V7()
		{
			throw new NotImplementedException();
		}
	}
	[ComVisible(true)]
	[Guid("972617D7-8244-4FB7-9870-4062B6086D5D")]
	internal class MipsDebugProgram
		: IDebugProgram2,
		IDebugProgramNode2
	{
		MipsDebugProcess m_process = null;
		string m_name = null;
		Guid m_guid = Guid.Empty;
		IMipsDEEventCallback m_eventCallback = null;
		MipsRemoteDebuggerClient m_remoteClient = null;
		public MipsRemoteDebuggerClient RemoteClient => m_remoteClient;

		public MipsDebugProcess Process => m_process;
		List<MipsDebugThread> m_threads = new List<MipsDebugThread>();
		public IEnumerable<MipsDebugThread> Threads => m_threads;

		private ConcurrentBag<MipsDebugModule> m_modules = new ConcurrentBag<MipsDebugModule>();
		public IEnumerable<MipsDebugModule> Modules => m_modules;
		IElfSymbolProvider m_symbolProvider = null;
		Semaphore m_semaphore = new Semaphore(1, 1);
		public Semaphore Semaphore => m_semaphore;
		public MipsDebugModule LoadModule(string filepath, uint address)
		{
			MipsDebugModule m = new MipsDebugModule(this, filepath, address);
			m_modules.Add(m);
			int hr = m_symbolProvider.LoadModule(m, filepath, address);
			if (hr != VSConstants.S_OK)
				throw new COMException("", hr);
			m.SymbolsLoaded = (hr == VSConstants.S_OK);
			return m;
		}
		public MipsDebugProgram(MipsRemoteDebuggerClient client, IMipsDEEventCallback eventCallback, MipsDebugProcess process, string name, IElfSymbolProvider symbolProvider)
		{
			m_remoteClient = client;
			m_eventCallback = eventCallback;
			m_process = process;
			m_name = name;
			m_guid = Guid.NewGuid();
			m_symbolProvider = symbolProvider;

			MipsDebugThread t = new MipsDebugThread(m_remoteClient, "main thread", this, m_symbolProvider);
			m_threads.Add(t);
		}
		public int EnumThreads(out IEnumDebugThreads2 ppEnum)
		{
			ppEnum = new EnumDebugThreads2(Threads);
			return VSConstants.S_OK;
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
			// This must kill the program

			// We cannot really kill the program, lets just break to stop execution
			CauseBreak();

			IDebugDefaultPort2 ddp = (IDebugDefaultPort2)Process.Port;
			IDebugPortNotify2 dpn;
			ddp.GetPortNotify(out dpn);
			dpn.RemoveProgramNode(this);

			return VSConstants.S_OK;
		}

		public int Attach(IDebugEventCallback2 pCallback)
		{
			throw new NotImplementedException();
		}

		public int CanDetach()
		{
			return VSConstants.S_OK;
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
			Semaphore.WaitOne();
			Microsoft.VisualStudio.Shell.ThreadHelper.JoinableTaskFactory.Run(async () =>
			{
				m_threads.ForEach(x =>
				{
					uint count;
					x.Resume(out count);
				});

				await m_remoteClient.SetStateAsync(md_state.md_state_enabled);
			});
			Semaphore.Release();
			return VSConstants.S_OK;
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
			// This is required to avoid race condition between resume/break
			// otherwise, thread.Suspend/Resume could execute on the wrong order
			Semaphore.WaitOne();
			// Not too sure there...
			m_remoteClient.SetState(md_state.md_state_paused);
			m_threads.ForEach(x =>
			{
				uint count;
				x.Suspend(out count);
			});
			m_eventCallback.SendDebugBreakEvent(Threads.FirstOrDefault());
			Semaphore.Release();
			return VSConstants.S_OK;
		}

		public int GetEngineInfo(out string pbstrEngine, out Guid pguidEngine)
		{
			pbstrEngine = "MipsDebugger";
			pguidEngine = MipsDEGuids.EngineGuid;
			return VSConstants.S_OK;
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
			ppEnum = new EnumDebugModules2(m_modules.Cast<IDebugModule2>().ToArray());
			return VSConstants.S_OK;
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

		public int GetProgramName(out string pbstrProgramName)
		{
			throw new NotImplementedException();
		}

		public int GetHostName(enum_GETHOSTNAME_TYPE dwHostNameType, out string pbstrHostName)
		{
			pbstrHostName = null;
			switch (dwHostNameType)
			{
				case enum_GETHOSTNAME_TYPE.GHN_FRIENDLY_NAME:
					pbstrHostName = "localhost";
					break;
				case enum_GETHOSTNAME_TYPE.GHN_FILE_NAME:
					pbstrHostName = "localhost";
					break;
				default:
					return VSConstants.E_INVALIDARG;
			}
			return VSConstants.S_OK;
		}

		public int GetHostPid(AD_PROCESS_ID[] pHostProcessId)
		{
			return Process.GetPhysicalProcessId(pHostProcessId);
		}

		public int GetHostMachineName_V7(out string pbstrHostMachineName)
		{
			throw new NotImplementedException();
		}

		public int Attach_V7(IDebugProgram2 pMDMProgram, IDebugEventCallback2 pCallback, uint dwReason)
		{
			throw new NotImplementedException();
		}

		public int DetachDebugger_V7()
		{
			throw new NotImplementedException();
		}


		async Task<bool> IsModuleLoadedBreakpointAsync(uint address)
		{
			byte[] buffer = new byte[4];
			await m_remoteClient.ReadMemoryAsync(buffer, address);
			uint bpInstr = BitConverter.ToUInt32(buffer, 0);
			if (bpInstr >> 26 != 0x1C)
				return false;
			uint code = (bpInstr >> 6) & 0xFFFFF;
			return code == 0x10AD;
		}
		
		void NotifyBreak()
		{
			m_threads.ForEach(x =>
			{
				uint count;
				x.Suspend(out count);
			});
			m_eventCallback.SendDebugBreakEvent(Threads.FirstOrDefault());
		}
		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
		struct debug_notify_module_loaded_data_t
		{
			public uint address;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 300)]
			public string filename;
		};
		async Task<T> ReadStructureFromMemoryAsync<T>(uint address)
		{
			byte[] buffer = new byte[Marshal.SizeOf(typeof(T))];
			if (await m_remoteClient.ReadMemoryAsync(buffer, address) != buffer.Length)
				throw new Exception();

			IntPtr ptr = Marshal.AllocHGlobal(buffer.Length);
			try
			{
				Marshal.Copy(buffer, 0, ptr, buffer.Length);
				T t = Marshal.PtrToStructure<T>(ptr);
				Marshal.FreeHGlobal(ptr);
				return t;
			}
			catch (Exception e)
			{
				Marshal.FreeHGlobal(ptr);
				throw e;
			}
		}

		Dictionary<uint, MipsDebugBoundBreakpoint> m_boundBreakpoints = new Dictionary<uint, MipsDebugBoundBreakpoint>();
		public void AddBoundBreakpoint(uint address, MipsDebugBoundBreakpoint breakpoint)
		{
			m_boundBreakpoints.Add(address, breakpoint);
		}
		public void RemoveBoundBreakpoint(uint address)
		{
			m_boundBreakpoints.Remove(address);
		}
		public int OnBreak()
		{
			m_semaphore.WaitOne();
			Microsoft.VisualStudio.Shell.ThreadHelper.JoinableTaskFactory.Run(async () =>
			{
				uint pc = await m_remoteClient.ReadRegisterAsync(md_register.md_register_pc, 0);

				if (await IsModuleLoadedBreakpointAsync(pc - 4))
				{
					try
					{
						uint loadedDataOfs = await m_remoteClient.ReadRegisterAsync(md_register.md_register_cop0_r22, 1);
						var loadedData = await ReadStructureFromMemoryAsync<debug_notify_module_loaded_data_t>(loadedDataOfs);

						if (string.IsNullOrEmpty(loadedData.filename))
						{
							var mod = Modules.FirstOrDefault(x => x.Address == loadedData.address);
							if (mod != null)
								m_eventCallback.SendModuleLoadEvent(mod, true);
						}
						else
						{
							// what if we load a module from somewhere we dont have access to... like a ssd?
							// probably should dump the elf file to have access to dwarf
							if (System.IO.File.Exists(loadedData.filename))
							{
								var mod = LoadModule(loadedData.filename, loadedData.address);
								m_eventCallback.SendModuleLoadEvent(mod, true);
							}
						}
					}
					catch (Exception)
					{

					}
					md_state s = await m_remoteClient.GetStateAsync();
					await m_remoteClient.SetStateAsync(s | md_state.md_state_enabled);
				}
				else
				{
					MipsDebugBoundBreakpoint breakpoint = null;
					if (m_boundBreakpoints.TryGetValue(pc - 4, out breakpoint))
						breakpoint.OnBreak(pc);

					// If we dont switch to main thread, use of elfSymbolProvider fails with E_NOINTERFACE on IElfSymbolProvider
					// after trying to marshal it seems. Not sure why it doesnt work
					await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
					NotifyBreak();
				}
			});
			m_semaphore.Release();
			return VSConstants.S_OK;
		}
		public int CreatePendingBreakpoint(IDebugBreakpointRequest2 pBPRequest, out IDebugPendingBreakpoint2 ppPendingBP)
		{
			ppPendingBP = null;
			try
			{

				enum_BP_LOCATION_TYPE[] type = new enum_BP_LOCATION_TYPE[1];
				if (pBPRequest.GetLocationType(type) != VSConstants.S_OK)
					return VSConstants.E_INVALIDARG;

				switch (type[0])
				{
					case enum_BP_LOCATION_TYPE.BPLT_CODE_FILE_LINE:
						ppPendingBP = new MipsDebugPendingBreakpoint(pBPRequest, m_symbolProvider, m_eventCallback, this);
						return VSConstants.S_OK;
					default:
						return VSConstants.E_FAIL;
				}
			}
			catch (Exception e)
			{

			}
			return VSConstants.E_FAIL;
		}

		public MipsProcessorPauseGuard SafePause()
		{
			return new MipsProcessorPauseGuard(m_remoteClient, m_semaphore);
		}
	}

	class MipsProcessorPauseGuard
		: IDisposable
	{
		private bool disposedValue;
		private MipsRemoteDebuggerClient m_remoteClient = null;
		md_state m_oldState = 0;
		Semaphore m_semaphore = null;
		public MipsProcessorPauseGuard(MipsRemoteDebuggerClient remoteClient, Semaphore semaphore)
		{
			m_remoteClient = remoteClient;
			m_semaphore = semaphore;
			m_semaphore.WaitOne();

			m_oldState = m_remoteClient.GetState();
			m_remoteClient.SetState(m_oldState & ~md_state.md_state_enabled);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					// TODO: dispose managed state (managed objects)
				}

				// TODO: free unmanaged resources (unmanaged objects) and override finalizer
				// TODO: set large fields to null


				m_remoteClient.SetState(m_oldState);
				m_semaphore.Release();

				disposedValue = true;
			}
		}

		// TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
		~MipsProcessorPauseGuard()
		{
			// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
			Dispose(disposing: false);
		}

		public void Dispose()
		{
			// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}
	}
}
