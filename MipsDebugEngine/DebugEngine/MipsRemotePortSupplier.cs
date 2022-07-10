using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;

namespace VSMipsProjectExtension.DebugEngine
{
	class MipsDebugServer
		: IDebugCoreServer3
	{
		public MipsDebugServer()
		{

		}
		public int GetMachineInfo(enum_MACHINE_INFO_FIELDS Fields, MACHINE_INFO[] pMachineInfo)
		{
			throw new NotImplementedException();
		}

		public int GetMachineName(out string pbstrName)
		{
			throw new NotImplementedException();
		}

		public int GetPortSupplier(ref Guid guidPortSupplier, out IDebugPortSupplier2 ppPortSupplier)
		{
			throw new NotImplementedException();
		}

		public int GetPort(ref Guid guidPort, out IDebugPort2 ppPort)
		{
			throw new NotImplementedException();
		}

		public int EnumPorts(out IEnumDebugPorts2 ppEnum)
		{
			throw new NotImplementedException();
		}

		public int EnumPortSuppliers(out IEnumDebugPortSuppliers2 ppEnum)
		{
			throw new NotImplementedException();
		}

		public int Unused()
		{
			throw new NotImplementedException();
		}

		public int GetServerName(out string pbstrName)
		{
			throw new NotImplementedException();
		}

		public int GetServerFriendlyName(out string pbstrName)
		{
			throw new NotImplementedException();
		}

		public int EnableAutoAttach(Guid[] rgguidSpecificEngines, uint celtSpecificEngines, string pszStartPageUrl, out string pbstrSessionId)
		{
			throw new NotImplementedException();
		}

		public int DiagnoseWebDebuggingError(string pszUrl)
		{
			throw new NotImplementedException();
		}

		public int CreateInstanceInServer(string szDll, ushort wLangID, ref Guid clsidObject, ref Guid riid, out IntPtr ppvObject)
		{
			throw new NotImplementedException();
		}

		public int QueryIsLocal()
		{
			throw new NotImplementedException();
		}

		public int GetConnectionProtocol(CONNECTION_PROTOCOL[] pProtocol)
		{
			throw new NotImplementedException();
		}

		public int DisableAutoAttach()
		{
			throw new NotImplementedException();
		}
	}
	// NOTE: Not to sure about that, port suppliers seem to have a debug engine associated, it fails if the debug port is not in some kind of engine list
	[ComVisible(true)]
	[Guid("B473EA66-5824-4F62-97DB-7A57F01C0C7A")]
	public class MipsRemotePortSupplier
		: IDebugPortSupplier2,
		IConnectionPointContainer,
		IDebugPortSupplierDescription2,
		ICustomQueryInterface,
		IDebugPortSupplierEx2,
		IDebugPortSupplierLocale2
	{
		List<MipsRemoteDebugPort> m_ports = new List<MipsRemoteDebugPort>();

		IDebugProgramPublisher2 m_programPublisher = null;
		IDebugCoreServer3 m_server = null;
		public MipsRemotePortSupplier()
		{
			Type type = Type.GetTypeFromCLSID(MipsDEGuids.CLSID_ProgramPublisher, true);
			m_programPublisher = (IDebugProgramPublisher2)Activator.CreateInstance(type);
			//m_server = new MipsDebugServer();
		}
		public int GetPortSupplierName(out string pbstrName)
		{
			pbstrName = "Mips Remote";
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

			MipsRemoteDebugPort port = new MipsRemoteDebugPort(name, this, pRequest, m_programPublisher, m_server);
			ppPort = port;
			m_ports.Add(port);

			return VSConstants.S_OK;
		}

		public int RemovePort(IDebugPort2 pPort)
		{
			return m_ports.Remove(pPort as MipsRemoteDebugPort) ? VSConstants.S_OK : Constants.E_PORTSUPPLIER_NO_PORT;
		}

		public void EnumConnectionPoints(out IEnumConnectionPoints ppEnum)
		{
			throw new NotImplementedException();
		}

		public void FindConnectionPoint(ref Guid riid, out IConnectionPoint ppCP)
		{
			throw new NotImplementedException();
		}

		public int GetDescription(enum_PORT_SUPPLIER_DESCRIPTION_FLAGS[] pdwFlags, out string pbstrText)
		{
			pbstrText = "The Mips Remote connection allows to debug programs on a computer that is running MipsRemoteDebugger";
			return VSConstants.S_OK;
		}
		public CustomQueryInterfaceResult GetInterface(ref Guid iid, out IntPtr ppv)
		{
			ppv = IntPtr.Zero;
			return CustomQueryInterfaceResult.NotHandled;
		}

		public int SetServer(IDebugCoreServer2 pServer)
		{
			m_server = (IDebugCoreServer3)pServer;
			return VSConstants.S_OK;
		}

		public int SetLocale(ushort wLangID)
		{
			return VSConstants.S_OK;
		}
	}
}
