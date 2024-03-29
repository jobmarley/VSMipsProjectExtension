﻿/*
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
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;

using MipsRemoteDebuggerUtils;

namespace VSMipsProjectExtension.DebugEngine
{
	class MipsDebugPortEventsConnectionPoint
		: IConnectionPoint,
		IDebugPortEvents2
	{
		int m_cookie = 1;
		IConnectionPointContainer m_container = null;
		Dictionary<int, IDebugPortEvents2> m_sinks = new Dictionary<int, IDebugPortEvents2>();
		public MipsDebugPortEventsConnectionPoint(IConnectionPointContainer container)
		{
			m_container = container;
		}

		public void GetConnectionInterface(out Guid pIID)
		{
			throw new NotImplementedException();
		}

		public void GetConnectionPointContainer(out IConnectionPointContainer ppCPC)
		{
			throw new NotImplementedException();
		}

		public void Advise(object pUnkSink, out int pdwCookie)
		{
			lock (m_sinks)
			{
				int c = m_cookie++;
				pdwCookie = c;
				m_sinks[c] = (IDebugPortEvents2)pUnkSink;
			}
		}

		public void Unadvise(int dwCookie)
		{
			lock (m_sinks)
			{
				m_sinks.Remove(dwCookie);
			}
		}

		public void EnumConnections(out IEnumConnections ppEnum)
		{
			throw new NotImplementedException();
		}

		public int Event(IDebugCoreServer2 pServer, IDebugPort2 pPort, IDebugProcess2 pProcess, IDebugProgram2 pProgram, IDebugEvent2 pEvent, ref Guid riidEvent)
		{
			foreach(var s in m_sinks.Values)
				s.Event(pServer, pPort, pProcess, pProgram, pEvent, ref riidEvent);
			return VSConstants.S_OK;
		}
	}
	internal class MipsRemoteDebugPort
		: IDebugPort2,
		IDebugPortEx2,
		IDebugPortNotify2,
		IConnectionPointContainer,
		IDebugDefaultPort2,
		ICustomQueryInterface
	{
		string m_name = null;
		public Guid Guid { get; private set; } = Guid.Empty;
		public MipsRemotePortSupplier PortSupplier { get; private set; } = null;
		public IDebugPortRequest2 Request { get; private set; } = null;

		MipsDebugPortEventsConnectionPoint m_eventConnectionPoint = null;
		IDebugProgramPublisher2 m_programPublisher = null;
		IDebugCoreServer3 m_server = null;

		MipsRemoteDebuggerClient m_client = null;
		public MipsRemoteDebuggerClient Client => m_client;

		public MipsRemoteDebugPort(string name, MipsRemotePortSupplier supplier, IDebugPortRequest2 request, IDebugProgramPublisher2 programPublisher, IDebugCoreServer3 server)
		{
			string[] parts = name.Split(':');
			if (parts.Length != 2)
				throw new Exception();

			string address = parts[0];
			ushort port = ushort.Parse(parts[1]);

			m_client = new MipsRemoteDebuggerClient();
			if (!m_client.Connect(address, port))
				throw new Exception();

			m_name = name;
			Guid = Guid.NewGuid();
			PortSupplier = supplier;
			Request = request;
			m_eventConnectionPoint = new MipsDebugPortEventsConnectionPoint(this);
			m_programPublisher = programPublisher;
			m_server = server;
		}

		void SendProcessCreateEvent(MipsDebugProcess process)
		{
			MipsDebugProcessCreateEvent e = new MipsDebugProcessCreateEvent(process);
			Guid iid = e.IID;
			m_eventConnectionPoint.Event(m_server, this, process, null, e, ref iid);
		}
		async Task SendProgramCreateEventAsync(MipsDebugProgram program) => await Task.Run(() => SendProgramCreateEvent(program));
		void SendProgramCreateEvent(MipsDebugProgram program)
		{
			MipsDebugProgramCreateEvent e = new MipsDebugProgramCreateEvent(program);
			Guid iid = e.IID;
			m_eventConnectionPoint.Event(m_server, this, program.Process, program, e, ref iid);
		}
		void SendProgramDestroyEvent(MipsDebugProgram program)
		{
			MipsDebugProgramDestroyEvent e = new MipsDebugProgramDestroyEvent(0, program);
			Guid iid = e.IID;
			m_eventConnectionPoint.Event(m_server, this, program.Process, program, e, ref iid);
		}
		public int GetPortName(out string pbstrName)
		{
			pbstrName = m_name;
			return VSConstants.S_OK;
		}

		public int GetPortId(out Guid pguidPort)
		{
			pguidPort = Guid;
			return VSConstants.S_OK;
		}

		public int GetPortRequest(out IDebugPortRequest2 ppRequest)
		{
			ppRequest = Request;
			return VSConstants.S_OK;
		}

		public int GetPortSupplier(out IDebugPortSupplier2 ppSupplier)
		{
			throw new NotImplementedException();
		}

		public int GetProcess(AD_PROCESS_ID ProcessId, out IDebugProcess2 ppProcess)
		{
			throw new NotImplementedException();
		}

		public int EnumProcesses(out IEnumDebugProcesses2 ppEnum)
		{
			throw new NotImplementedException();
		}

		public int LaunchSuspended(string pszExe, string pszArgs, string pszDir, string bstrEnv, uint hStdInput, uint hStdOutput, uint hStdError, out IDebugProcess2 ppPortProcess)
		{
			ppPortProcess = null;
			return VSConstants.E_NOTIMPL;
		}

		public int ResumeProcess(IDebugProcess2 pPortProcess)
		{
			throw new NotImplementedException();
		}

		public int CanTerminateProcess(IDebugProcess2 pPortProcess)
		{
			throw new NotImplementedException();
		}

		public int TerminateProcess(IDebugProcess2 pPortProcess)
		{
			throw new NotImplementedException();
		}

		public int GetPortProcessId(out uint pdwProcessId)
		{
			throw new NotImplementedException();
		}

		public int GetProgram(IDebugProgramNode2 pProgramNode, out IDebugProgram2 ppProgram)
		{
			throw new NotImplementedException();
		}

		public int AddProgramNode(IDebugProgramNode2 pProgramNode)
		{
			MipsDebugProgram program = pProgramNode as MipsDebugProgram;
			SendProgramCreateEventAsync(program);
			//SendProgramCreateEvent(program);
			//SendProcessCreateEvent(program?.Process);
			m_programPublisher.PublishProgramNode(pProgramNode);
			return VSConstants.S_OK;
		}

		public int RemoveProgramNode(IDebugProgramNode2 pProgramNode)
		{
			MipsDebugProgram program = pProgramNode as MipsDebugProgram;
			SendProgramDestroyEvent(program);
			return m_programPublisher.UnpublishProgramNode(pProgramNode);
		}

		public void EnumConnectionPoints(out IEnumConnectionPoints ppEnum)
		{
			throw new NotImplementedException();
		}

		public void FindConnectionPoint(ref Guid riid, out IConnectionPoint ppCP)
		{
			ppCP = null;
			if (riid == typeof(IDebugPortEvents2).GUID)
				ppCP = m_eventConnectionPoint;
		}

		public int GetPortNotify(out IDebugPortNotify2 ppPortNotify)
		{
			ppPortNotify = this;
			return VSConstants.S_OK;
		}

		public int GetServer(out IDebugCoreServer3 ppServer)
		{
			ppServer = m_server;
			return VSConstants.S_OK;
		}

		public int QueryIsLocal()
		{
			throw new NotImplementedException();
		}

		public CustomQueryInterfaceResult GetInterface(ref Guid iid, out IntPtr ppv)
		{
			ppv = IntPtr.Zero;
			return CustomQueryInterfaceResult.NotHandled;
		}

		public void Close()
		{
			m_client.Close();
			m_client = null;
		}
	}
}
