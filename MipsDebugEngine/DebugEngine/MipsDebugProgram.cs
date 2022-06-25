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

		public MipsDebugProcess Process => m_process;
		public MipsDebugThread Thread { get; private set; }

		private List<MipsDebugModule> m_modules = new List<MipsDebugModule>();
		public IEnumerable<MipsDebugModule> Modules => m_modules;
		public MipsDebugModule LoadModule(string filepath)
		{
			MipsDebugModule m = new MipsDebugModule(this, filepath);
			m_modules.Add(m);
			return m;
		}
		public MipsDebugProgram(MipsDebugProcess process, string name)
		{
			m_process = process;
			m_name = name;
			m_guid = Guid.NewGuid();
			Thread = new MipsDebugThread("main thread", this);
		}
		public int EnumThreads(out IEnumDebugThreads2 ppEnum)
		{
			throw new NotImplementedException();
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
			throw new NotImplementedException();
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
			throw new NotImplementedException();
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
	}
}
