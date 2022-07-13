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

		public MipsDebugProcess Process => m_process;
		List<MipsDebugThread> m_threads = new List<MipsDebugThread>();
		public IEnumerable<MipsDebugThread> Threads => m_threads;

		private ConcurrentBag<MipsDebugModule> m_modules = new ConcurrentBag<MipsDebugModule>();
		public IEnumerable<MipsDebugModule> Modules => m_modules;
		IElfSymbolProvider m_symbolProvider = null;
		object m_eventLock = new object();
		public MipsDebugModule LoadModule(string filepath, uint address)
		{
			MipsDebugModule m = new MipsDebugModule(this, filepath, address);
			m_modules.Add(m);
			int hr = m_symbolProvider.LoadModule(m, filepath, address);
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
			//SendEvent(new MipsDebugProgramDestroyEvent(0, m_debuggedProgram));
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
			lock (m_eventLock)
			{
				Microsoft.VisualStudio.Shell.ThreadHelper.JoinableTaskFactory.Run(async () => await m_remoteClient.SetStateAsync(md_state.md_state_enabled));

				m_threads.ForEach(x =>
				{
					uint count;
					x.Resume(out count);
				});
				return VSConstants.S_OK;
			}
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
			lock (m_eventLock)
			{
				// Not too sure there...
				Microsoft.VisualStudio.Shell.ThreadHelper.JoinableTaskFactory.Run(async () =>
				{
					await m_remoteClient.SetStateAsync(md_state.md_state_paused);
					await Microsoft.VisualStudio.Shell.ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
					m_threads.ForEach(x =>
					{
						uint count;
						x.Suspend(out count);
					});
					m_eventCallback.SendDebugBreakEvent(Threads.FirstOrDefault());
				});

				return VSConstants.S_OK;
			}
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

		public int OnBreak()
		{
			lock (m_eventLock)
			{
				Microsoft.VisualStudio.Shell.ThreadHelper.JoinableTaskFactory.Run(async () =>
				{
					await Microsoft.VisualStudio.Shell.ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
					m_threads.ForEach(x =>
					{
						uint count;
						x.Suspend(out count);
					});
					m_eventCallback.SendDebugBreakEvent(Threads.FirstOrDefault());
				});
				return VSConstants.S_OK;
			}
		}
	}
}
