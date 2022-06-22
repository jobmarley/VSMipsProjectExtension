// FPGADebugPort2.cpp : Implementation of CFPGADebugPort2

#include "pch.h"
#include "FPGADebugPort2.h"
#include "FPGAEnumerator.h"

// CFPGADebugPort2

HRESULT CFPGADebugPort2::Init(IDebugPortSupplier2* pPortSupplier, IDebugPortRequest2* pRequest, const WCHAR* name)
{
	m_pPortSupplier = pPortSupplier;
	m_name = name;
	m_pRequest = pRequest;
	CoCreateGuid(&m_portGuid);
	return S_OK;
}

HRESULT CFPGADebugPort2::GetPortName(BSTR* pbstrName)
{
	*pbstrName = SysAllocString(m_name.c_str());
	return S_OK;
}
HRESULT CFPGADebugPort2::GetPortId(GUID* pguidPort)
{
	*pguidPort = m_portGuid;
	return S_OK;
}
HRESULT CFPGADebugPort2::GetPortRequest(IDebugPortRequest2** ppRequest)
{
	if (m_pRequest)
		return m_pRequest->QueryInterface(ppRequest);

	*ppRequest = nullptr;
	return E_PORT_NO_REQUEST;
}
HRESULT CFPGADebugPort2::GetPortSupplier(IDebugPortSupplier2** ppSupplier)
{
	return m_pPortSupplier->QueryInterface(ppSupplier);
}
HRESULT CFPGADebugPort2::GetProcess(AD_PROCESS_ID ProcessId, IDebugProcess2** ppProcess)
{
	for (auto& it : m_processes)
	{
		AD_PROCESS_ID pid = { 0 };
		HRESULT hr = it->GetPhysicalProcessId(&pid);
		if (FAILED(hr))
			continue;
		if (memcmp(&pid, &ProcessId, sizeof(pid)) == 0)
			return it->QueryInterface(ppProcess);
	}
	return E_FAIL;
}
HRESULT CFPGADebugPort2::EnumProcesses(IEnumDebugProcesses2** ppEnum)
{
	CComPtr<CFPGAEnumerator<IEnumDebugProcesses2>> pEnumerator;
	HRESULT hr = CFPGAEnumerator<IEnumDebugProcesses2>::CreateInstance(&pEnumerator);
	if (FAILED(hr))
		return hr;

	pEnumerator->Add(m_processes);

	return pEnumerator.QueryInterface(ppEnum);
}

// IDebugPortEx2
HRESULT CFPGADebugPort2::LaunchSuspended(
	LPCOLESTR pszExe,
	LPCOLESTR pszArgs,
	LPCOLESTR pszDir,
	BSTR bstrEnv,
	DWORD hStdInput,
	DWORD hStdOutput,
	DWORD hStdError,
	IDebugProcess2** ppPortProcess)
{
	 return E_NOTIMPL;
}
HRESULT CFPGADebugPort2::ResumeProcess(
	IDebugProcess2* pPortProcess)
{
	 return E_NOTIMPL;
}
HRESULT CFPGADebugPort2::CanTerminateProcess(
	IDebugProcess2* pPortProcess)
{
	 return E_NOTIMPL;
}
HRESULT CFPGADebugPort2::TerminateProcess(
	IDebugProcess2* pPortProcess)
{
	 return E_NOTIMPL;
}
HRESULT CFPGADebugPort2::GetPortProcessId(
	DWORD* pdwProcessId)
{
	 return E_NOTIMPL;
}
HRESULT CFPGADebugPort2::GetProgram(
	IDebugProgramNode2* pProgramNode,
	IDebugProgram2** ppProgram)
{
	 return E_NOTIMPL;
}

// IDebugPortNotify2
HRESULT CFPGADebugPort2::AddProgramNode(
	IDebugProgramNode2* pProgramNode)
{
	 return E_NOTIMPL;
}
HRESULT CFPGADebugPort2::RemoveProgramNode(
	IDebugProgramNode2* pProgramNode)
{
	 return E_NOTIMPL;
}

// IConnectionPointContainer
HRESULT CFPGADebugPort2::EnumConnectionPoints(
	IEnumConnectionPoints** ppEnum)
{
	 return E_NOTIMPL;
}
HRESULT CFPGADebugPort2::FindConnectionPoint(
	REFIID riid,
	IConnectionPoint** ppCP)
{
	 return E_NOTIMPL;
}