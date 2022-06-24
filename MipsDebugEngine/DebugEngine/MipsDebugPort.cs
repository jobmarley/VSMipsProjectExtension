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
	internal class MipsDebugPort
		: IDebugPort2
	{
		string m_name = null;
		public Guid Guid { get; private set; } = Guid.Empty;
		public MipsDebugPortSupplier PortSupplier { get; private set; } = null;
		public IDebugPortRequest2 Request { get; private set; } = null;
		public MipsDebugPort(string name, MipsDebugPortSupplier supplier, IDebugPortRequest2 request)
		{
			m_name = name;
			Guid = Guid.NewGuid();
			PortSupplier = supplier;
			Request = request;
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
	}
}
