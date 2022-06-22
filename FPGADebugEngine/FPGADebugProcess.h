// FPGADebugProcess.h : Declaration of the CFPGADebugProcess

#pragma once
#include "resource.h"       // main symbols



#include "FPGADebugEngine_i.h"
#include "FPGADebugProgram.h"


#if defined(_WIN32_WCE) && !defined(_CE_DCOM) && !defined(_CE_ALLOW_SINGLE_THREADED_OBJECTS_IN_MTA)
#error "Single-threaded COM objects are not properly supported on Windows CE platform, such as the Windows Mobile platforms that do not include full DCOM support. Define _CE_ALLOW_SINGLE_THREADED_OBJECTS_IN_MTA to force ATL to support creating single-thread COM object's and allow use of it's single-threaded COM object implementations. The threading model in your rgs file was set to 'Free' as that is the only threading model supported in non DCOM Windows CE platforms."
#endif

using namespace ATL;


// CFPGADebugProcess

// {407565BD-58D8-4150-BDAC-6A14D02FDC7E}
DEFINE_GUID(IID_IFPGADebugProcess,
	0x407565bd, 0x58d8, 0x4150, 0xbd, 0xac, 0x6a, 0x14, 0xd0, 0x2f, 0xdc, 0x7e);

MIDL_INTERFACE("407565BD-58D8-4150-BDAC-6A14D02FDC7E")
IFPGADebugProcess
	: IDebugProcess2
{
	virtual HRESULT Init(const WCHAR* filepath) = 0;
	virtual HRESULT GetProgram(IFPGADebugProgram** ppProgram) = 0;
};

class ATL_NO_VTABLE CFPGADebugProcess :
	public CComObjectRootEx<CComSingleThreadModel>,
	public CComCoClass<CFPGADebugProcess, &CLSID_FPGADebugProcess>,
	public IFPGADebugProcess
{
	CComPtr<IDebugEventCallback2> m_pEventCallback;
	fs::path m_filepath;
	FILETIME m_creationTime = { 0 };
	bool m_running = false;

	std::vector<ATL::CComPtr<IFPGADebugProgram>> m_programs;

	GUID m_physicalProcessGuid = { 0 };
	GUID m_processGuid = { 0 };
public:
	CFPGADebugProcess()
	{
	}

DECLARE_REGISTRY_RESOURCEID(110)


BEGIN_COM_MAP(CFPGADebugProcess)
	COM_INTERFACE_ENTRY(IFPGADebugProcess)
	COM_INTERFACE_ENTRY(IDebugProcess2)
END_COM_MAP()

STDMETHOD(Init)(const WCHAR* filepath);
STDMETHOD(GetProgram)(IFPGADebugProgram** ppProgram);

	//////////////////////////////////////////////////////////// 
	//IDebugProcess2 
	STDMETHOD(GetInfo)(PROCESS_INFO_FIELDS Fields, PROCESS_INFO* pProcessInfo);
	STDMETHOD(EnumPrograms)(IEnumDebugPrograms2** ppEnum);
	STDMETHOD(GetName)(GETNAME_TYPE gnType, BSTR* pbstrName);
	STDMETHOD(GetServer)(IDebugCoreServer2** ppServer);
	STDMETHOD(Terminate)(void);
	STDMETHOD(Attach)(IDebugEventCallback2* pCallback,
		GUID* rgguidSpecificEngines,
		DWORD celtSpecificEngines,
		HRESULT* rghrEngineAttach);
	STDMETHOD(CanDetach)(void);
	STDMETHOD(Detach)(void);
	STDMETHOD(GetPhysicalProcessId)(AD_PROCESS_ID* pProcessId);
	STDMETHOD(GetProcessId)(GUID* pguidProcessId);
	STDMETHOD(GetAttachedSessionName)(BSTR* pbstrSessionName);
	STDMETHOD(EnumThreads)(IEnumDebugThreads2** ppEnum);
	STDMETHOD(CauseBreak)(void);
	STDMETHOD(GetPort)(IDebugPort2** ppPort);

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

OBJECT_ENTRY_AUTO(__uuidof(FPGADebugProcess), CFPGADebugProcess)
