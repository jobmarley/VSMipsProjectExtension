using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;

namespace VSMipsProjectExtension.DebugEngine
{
	[ComVisible(true)]
	[Guid("5F01F2FA-1A4B-4D32-AF99-BE3B67F1F162")]
	internal class MipsDebugProcess
		: IDebugProcess2,
		//IDebugProcessEx2,
		ICustomQueryInterface
	{
		string m_filepath;
		DateTime m_creationTime;
		Guid m_physicalProcessGuid;
		Guid m_processGuid;
		bool m_running = false;
		public IDebugPort2 Port { get; private set; } = null;

		private List<MipsDebugProgram> m_programs = new List<MipsDebugProgram>();
		public IEnumerable<MipsDebugProgram> Programs => m_programs;
		public void AddProgram(MipsDebugProgram program)
		{
			m_programs.Add(program);
		}
		public string Filepath => m_filepath;
		public MipsDebugProcess(string filepath, IDebugPort2 port)
		{
			m_filepath = filepath;
			m_creationTime = DateTime.Now;
			m_physicalProcessGuid = Guid.NewGuid();
			m_processGuid = Guid.NewGuid();
			Port = port;
		}
		public int GetInfo(enum_PROCESS_INFO_FIELDS Fields, PROCESS_INFO[] pProcessInfo)
		{
			pProcessInfo[0].Fields = 0;
			if (Fields.HasFlag(enum_PROCESS_INFO_FIELDS.PIF_FILE_NAME))
			{
				GetName(enum_GETNAME_TYPE.GN_FILENAME, out pProcessInfo[0].bstrFileName);
				pProcessInfo[0].Fields |= enum_PROCESS_INFO_FIELDS.PIF_FILE_NAME;
			}
			if (Fields.HasFlag(enum_PROCESS_INFO_FIELDS.PIF_BASE_NAME))
			{
				GetName(enum_GETNAME_TYPE.GN_BASENAME, out pProcessInfo[0].bstrBaseName);
				pProcessInfo[0].Fields |= enum_PROCESS_INFO_FIELDS.PIF_BASE_NAME;
			}
			if (Fields.HasFlag(enum_PROCESS_INFO_FIELDS.PIF_TITLE))
			{
				GetName(enum_GETNAME_TYPE.GN_TITLE, out pProcessInfo[0].bstrTitle);
				pProcessInfo[0].Fields |= enum_PROCESS_INFO_FIELDS.PIF_TITLE;
			}
			if (Fields.HasFlag(enum_PROCESS_INFO_FIELDS.PIF_PROCESS_ID))
			{
				AD_PROCESS_ID[] pid = new AD_PROCESS_ID[1];
				GetPhysicalProcessId(pid);
				pProcessInfo[0].ProcessId = pid[0];
				pProcessInfo[0].Fields |= enum_PROCESS_INFO_FIELDS.PIF_PROCESS_ID;
			}
			//if ((Fields & enum_PROCESS_INFO_FIELDS.PIF_SESSION_ID) == enum_PROCESS_INFO_FIELDS.PIF_SESSION_ID)
			if (Fields.HasFlag(enum_PROCESS_INFO_FIELDS.PIF_ATTACHED_SESSION_NAME))
			{
				GetAttachedSessionName(out pProcessInfo[0].bstrAttachedSessionName);
				pProcessInfo[0].Fields |= enum_PROCESS_INFO_FIELDS.PIF_ATTACHED_SESSION_NAME;
			}
			if (Fields.HasFlag(enum_PROCESS_INFO_FIELDS.PIF_CREATION_TIME))
			{
				pProcessInfo[0].CreationTime.dwLowDateTime = (uint)(m_creationTime.ToFileTime() & 0xFFFFFFFF);
				pProcessInfo[0].CreationTime.dwHighDateTime = (uint)((m_creationTime.ToFileTime() >> 32) & 0xFFFFFFFF);
				pProcessInfo[0].Fields |= enum_PROCESS_INFO_FIELDS.PIF_CREATION_TIME;
			}
			if (Fields.HasFlag(enum_PROCESS_INFO_FIELDS.PIF_FLAGS))
			{
				pProcessInfo[0].Flags = 0;
				//if (m_pEventCallback)
				//	pProcessInfo[0].Flags |= enum_PROCESS_INFO_FLAGS.PIFLAG_DEBUGGER_ATTACHED;

				if (m_running)
					pProcessInfo[0].Flags |= enum_PROCESS_INFO_FLAGS.PIFLAG_PROCESS_RUNNING;
				else
					pProcessInfo[0].Flags |= enum_PROCESS_INFO_FLAGS.PIFLAG_PROCESS_STOPPED;


				pProcessInfo[0].Fields |= enum_PROCESS_INFO_FIELDS.PIF_FLAGS;
			}
			return VSConstants.S_OK;
		}

		public int EnumPrograms(out IEnumDebugPrograms2 ppEnum)
		{
			ppEnum = new EnumDebugPrograms2(m_programs.Cast<IDebugProgram2>().ToArray());
			return VSConstants.S_OK;
		}

		public int GetName(enum_GETNAME_TYPE gnType, out string pbstrName)
		{
			pbstrName = null;
			switch (gnType)
			{
				case enum_GETNAME_TYPE.GN_NAME:
					pbstrName = System.IO.Path.GetFileNameWithoutExtension(m_filepath);
					return VSConstants.S_OK;
				case enum_GETNAME_TYPE.GN_FILENAME:
					pbstrName = m_filepath;
					return VSConstants.S_OK;
				case enum_GETNAME_TYPE.GN_BASENAME:
					pbstrName = System.IO.Path.GetFileName(m_filepath);
					return VSConstants.S_OK;
				case enum_GETNAME_TYPE.GN_MONIKERNAME:
					break;
				case enum_GETNAME_TYPE.GN_URL:
					break;
				case enum_GETNAME_TYPE.GN_TITLE:
					break;
				case enum_GETNAME_TYPE.GN_STARTPAGEURL:
					break;
				default:
					break;
			}
			return VSConstants.E_FAIL;
		}

		public int GetServer(out IDebugCoreServer2 ppServer)
		{
			throw new NotImplementedException();
		}

		public int Terminate()
		{
			return VSConstants.S_OK;
		}

		public int Attach(IDebugEventCallback2 pCallback, Guid[] rgguidSpecificEngines, uint celtSpecificEngines, int[] rghrEngineAttach)
		{
			throw new NotImplementedException();
		}

		public int CanDetach()
		{
			throw new NotImplementedException();
		}

		public int Detach()
		{
			throw new NotImplementedException();
		}

		public int GetPhysicalProcessId(AD_PROCESS_ID[] pProcessId)
		{
			pProcessId[0].ProcessIdType = 1;// AD_PROCESS_ID_GUID;
			pProcessId[0].guidProcessId = m_physicalProcessGuid;
			return VSConstants.S_OK;
		}

		public int GetProcessId(out Guid pguidProcessId)
		{
			pguidProcessId = m_processGuid;
			return VSConstants.S_OK;
		}

		public int GetAttachedSessionName(out string pbstrSessionName)
		{
			throw new NotImplementedException();
		}

		public int EnumThreads(out IEnumDebugThreads2 ppEnum)
		{
			throw new NotImplementedException();
		}

		public int CauseBreak()
		{
			throw new NotImplementedException();
		}

		public int GetPort(out IDebugPort2 ppPort)
		{
			ppPort = Port;
			return VSConstants.S_OK;
		}
		public CustomQueryInterfaceResult GetInterface(ref Guid iid, out IntPtr ppv)
		{
			ppv = IntPtr.Zero;
			return CustomQueryInterfaceResult.NotHandled;
		}

		//public int Attach(IDebugSession2 pSession)
		//{
		//	throw new NotImplementedException();
		//}

		//public int Detach(IDebugSession2 pSession)
		//{
		//	throw new NotImplementedException();
		//}

		//public int AddImplicitProgramNodes(ref Guid guidLaunchingEngine, Guid[] rgguidSpecificEngines, uint celtSpecificEngines)
		//{
		//	throw new NotImplementedException();
		//}
	}
}
