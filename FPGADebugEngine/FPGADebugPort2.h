// FPGADebugPort2.h : Declaration of the CFPGADebugPort2

#pragma once
#include "resource.h"       // main symbols

#include "FPGADebugEngine_i.h"
#include "FPGADebugProcess.h"


#if defined(_WIN32_WCE) && !defined(_CE_DCOM) && !defined(_CE_ALLOW_SINGLE_THREADED_OBJECTS_IN_MTA)
#error "Single-threaded COM objects are not properly supported on Windows CE platform, such as the Windows Mobile platforms that do not include full DCOM support. Define _CE_ALLOW_SINGLE_THREADED_OBJECTS_IN_MTA to force ATL to support creating single-thread COM object's and allow use of it's single-threaded COM object implementations. The threading model in your rgs file was set to 'Free' as that is the only threading model supported in non DCOM Windows CE platforms."
#endif

using namespace ATL;

// {01CFA1E2-CF44-4EE0-9CD5-77937364F290}
DEFINE_GUID(IID_IFPGADebugPort2,
	0x1cfa1e2, 0xcf44, 0x4ee0, 0x9c, 0xd5, 0x77, 0x93, 0x73, 0x64, 0xf2, 0x90);


MIDL_INTERFACE("407565BD-58D8-4150-BDAC-6A14D02FDC7E")
IFPGADebugPort2
	: IDebugPort2
{
	virtual HRESULT Init(IDebugPortSupplier2* pPortSupplier, IDebugPortRequest2* pRequest, const WCHAR* name) = 0;
};


// CFPGADebugPort2

class ATL_NO_VTABLE CFPGADebugPort2 :
	public CComObjectRootEx<CComSingleThreadModel>,
	public CComCoClass<CFPGADebugPort2, &CLSID_FPGADebugPort2>,
	public IFPGADebugPort2,
	public IDebugPortEx2,
	public IDebugPortNotify2,
	public IConnectionPointContainer
{
	std::wstring m_name;
	GUID m_portGuid = { 0 };
	IDebugPortSupplier2* m_pPortSupplier = nullptr;
	std::vector<CComPtr<IFPGADebugProcess>> m_processes;
	CComPtr<IDebugPortRequest2> m_pRequest;
public:
	CFPGADebugPort2()
	{
	}

DECLARE_REGISTRY_RESOURCEID(119)


BEGIN_COM_MAP(CFPGADebugPort2)
	COM_INTERFACE_ENTRY(IFPGADebugPort2)
	COM_INTERFACE_ENTRY(IDebugPort2)
	COM_INTERFACE_ENTRY(IDebugPortEx2)
	COM_INTERFACE_ENTRY(IDebugPortNotify2)
	COM_INTERFACE_ENTRY(IConnectionPointContainer)
END_COM_MAP()

	STDMETHOD(Init)(IDebugPortSupplier2* pPortSupplier, IDebugPortRequest2* pRequest, const WCHAR* name);

	// IDebugPort2
	STDMETHOD(GetPortName)(BSTR* pbstrName);
	STDMETHOD(GetPortId)(GUID* pguidPort);
	STDMETHOD(GetPortRequest)(IDebugPortRequest2** ppRequest);
	STDMETHOD(GetPortSupplier)(IDebugPortSupplier2** ppSupplier);
	STDMETHOD(GetProcess)(AD_PROCESS_ID ProcessId, IDebugProcess2** ppProcess);
	STDMETHOD(EnumProcesses)(IEnumDebugProcesses2** ppEnum);

	// IDebugPortEx2
	STDMETHOD(LaunchSuspended)(
		LPCOLESTR pszExe,
		LPCOLESTR pszArgs,
		LPCOLESTR pszDir,
		BSTR bstrEnv,
		DWORD hStdInput,
		DWORD hStdOutput,
		DWORD hStdError,
		IDebugProcess2** ppPortProcess);
	STDMETHOD(ResumeProcess)(
		IDebugProcess2* pPortProcess);
	STDMETHOD(CanTerminateProcess)(
		IDebugProcess2* pPortProcess);
	STDMETHOD(TerminateProcess)(
		IDebugProcess2* pPortProcess);
	STDMETHOD(GetPortProcessId)(
		DWORD* pdwProcessId);
	STDMETHOD(GetProgram)(
		IDebugProgramNode2* pProgramNode,
		IDebugProgram2** ppProgram);

	// IDebugPortNotify2
	STDMETHOD(AddProgramNode)(
		IDebugProgramNode2* pProgramNode);
	STDMETHOD(RemoveProgramNode)(
		IDebugProgramNode2* pProgramNode);

	// IConnectionPointContainer
	STDMETHOD(EnumConnectionPoints)(
		IEnumConnectionPoints** ppEnum);
	STDMETHOD(FindConnectionPoint)(
		REFIID riid,
		IConnectionPoint** ppCP);

	DECLARE_PROTECT_FINAL_CONSTRUCT()

	HRESULT FinalConstruct()
	{
		return S_OK;
	}

	void FinalRelease()
	{
	}

public:



};

OBJECT_ENTRY_AUTO(__uuidof(FPGADebugPort2), CFPGADebugPort2)
