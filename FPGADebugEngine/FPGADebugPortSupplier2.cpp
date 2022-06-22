// FPGADebugPortSupplier2.cpp : Implementation of CFPGADebugPortSupplier2

#include "pch.h"
#include "FPGADebugPortSupplier2.h"
#include "FPGADebugEngineGUIDs.h"

#include "FPGAEnumerator.h"
#include "FPGADebugPort2.h"
// CFPGADebugPortSupplier2


HRESULT CFPGADebugPortSupplier2::GetPortSupplierName(BSTR* pbstrName)
{
	*pbstrName = SysAllocString(L"MyDebuggerPortSupplier");
	return S_OK;
}
HRESULT CFPGADebugPortSupplier2::GetPortSupplierId(GUID* pguidPortSupplier)
{
	*pguidPortSupplier = GUID_MyDebugger_PortSupplier;
	return S_OK;
}
HRESULT CFPGADebugPortSupplier2::GetPort(REFGUID guidPort, IDebugPort2** ppPort)
{
	auto it = m_ports.find(guidPort);
	if (it == m_ports.end())
		return E_PORTSUPPLIER_NO_PORT;
	
	return it->second->QueryInterface(ppPort);
}
HRESULT CFPGADebugPortSupplier2::EnumPorts(IEnumDebugPorts2** ppEnum)
{
	CComPtr<CFPGAEnumerator<IEnumDebugPorts2>> pEnumerator;
	HRESULT hr = CFPGAEnumerator<IEnumDebugPorts2>::CreateInstance(&pEnumerator);
	if (FAILED(hr))
		return hr;

	return pEnumerator.QueryInterface(ppEnum);
}
HRESULT CFPGADebugPortSupplier2::CanAddPort(void)
{
	return S_OK;
}
HRESULT CFPGADebugPortSupplier2::AddPort(IDebugPortRequest2* pRequest, IDebugPort2** ppPort)
{
	CComPtr<IFPGADebugPort2> pDebugPort;
	HRESULT hr = CFPGADebugPort2::CreateInstance(&pDebugPort);
	if (FAILED(hr))
		return hr;
	
	CComBSTR name;
	hr = pRequest->GetPortName(&name);
	if (FAILED(hr))
		return hr;
	hr = pDebugPort->Init(this, pRequest, name);
	if (FAILED(hr))
		return hr;

	*ppPort = pDebugPort.Detach();
	return S_OK;
}
HRESULT CFPGADebugPortSupplier2::RemovePort(IDebugPort2* pPort)
{
	for (auto it : m_ports)
	{
		if (it.second == pPort)
		{
			m_ports.erase(it.first);
			return S_OK;
		}
	}
	return E_PORTSUPPLIER_NO_PORT;
}