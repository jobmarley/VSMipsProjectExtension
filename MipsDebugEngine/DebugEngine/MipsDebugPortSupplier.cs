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
	[Guid("B473EA66-5824-4F62-97DB-7A57F01C0C7A")]
	public class MipsDebugPortSupplier
		: IDebugPortSupplier2
	{
		List<MipsDebugPort> m_ports = new List<MipsDebugPort>();
		public int GetPortSupplierName(out string pbstrName)
		{
			pbstrName = "Mips Port Supplier";
			return VSConstants.S_OK;
		}

		public int GetPortSupplierId(out Guid pguidPortSupplier)
		{
			pguidPortSupplier = MipsDEGuids.PortSupplierGuid;
			return VSConstants.S_OK;
		}

		public int GetPort(ref Guid guidPort, out IDebugPort2 ppPort)
		{
			Guid searchGuid = guidPort;
			ppPort = m_ports.FirstOrDefault(x => x.Guid == searchGuid);
			return ppPort == null ? Constants.E_PORTSUPPLIER_NO_PORT : VSConstants.S_OK;
		}

		public int EnumPorts(out IEnumDebugPorts2 ppEnum)
		{
			ppEnum = new EnumDebugPorts2(m_ports.Cast<IDebugPort2>().ToArray());
			return VSConstants.S_OK;
		}

		public int CanAddPort()
		{
			return VSConstants.S_OK;
		}

		public int AddPort(IDebugPortRequest2 pRequest, out IDebugPort2 ppPort)
		{
			ppPort = null;

			string name;
			int err = pRequest.GetPortName(out name);
			if (err != VSConstants.S_OK)
				return err;
			ppPort = new MipsDebugPort(name, this, pRequest);
			return VSConstants.S_OK;
		}

		public int RemovePort(IDebugPort2 pPort)
		{
			return m_ports.Remove(pPort as MipsDebugPort) ? VSConstants.S_OK : Constants.E_PORTSUPPLIER_NO_PORT;
		}
	}
}
