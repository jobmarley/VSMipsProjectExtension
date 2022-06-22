// FPGADebugProgramProvider2.h : Declaration of the CFPGADebugProgramProvider2

#pragma once
#include "resource.h"       // main symbols



#include "FPGADebugEngine_i.h"



#if defined(_WIN32_WCE) && !defined(_CE_DCOM) && !defined(_CE_ALLOW_SINGLE_THREADED_OBJECTS_IN_MTA)
#error "Single-threaded COM objects are not properly supported on Windows CE platform, such as the Windows Mobile platforms that do not include full DCOM support. Define _CE_ALLOW_SINGLE_THREADED_OBJECTS_IN_MTA to force ATL to support creating single-thread COM object's and allow use of it's single-threaded COM object implementations. The threading model in your rgs file was set to 'Free' as that is the only threading model supported in non DCOM Windows CE platforms."
#endif

using namespace ATL;


// CFPGADebugProgramProvider2

class ATL_NO_VTABLE CFPGADebugProgramProvider2 :
	public CComObjectRootEx<CComSingleThreadModel>,
	public CComCoClass<CFPGADebugProgramProvider2, &CLSID_FPGADebugProgramProvider2>,
	public IDebugProgramProvider2
{
public:
	CFPGADebugProgramProvider2()
	{
	}

DECLARE_REGISTRY_RESOURCEID(118)


BEGIN_COM_MAP(CFPGADebugProgramProvider2)
	COM_INTERFACE_ENTRY(IDebugProgramProvider2)
END_COM_MAP()

    STDMETHOD(GetProviderProcessData)(
        PROVIDER_FLAGS Flags,
        IDebugDefaultPort2* pPort,
        AD_PROCESS_ID processId,
        CONST_GUID_ARRAY EngineFilter,
        PROVIDER_PROCESS_DATA* pProcess);
    STDMETHOD(GetProviderProgramNode)(
        PROVIDER_FLAGS Flags,
        IDebugDefaultPort2* pPort,
        AD_PROCESS_ID processId,
        REFGUID guidEngine,
        UINT64 programId,
        IDebugProgramNode2** ppProgramNode);
    STDMETHOD(WatchForProviderEvents)(
        PROVIDER_FLAGS Flags,
        IDebugDefaultPort2* pPort,
        AD_PROCESS_ID processId,
        CONST_GUID_ARRAY EngineFilter,
        REFGUID guidLaunchingEngine,
        IDebugPortNotify2* pEventCallback);
    STDMETHOD(SetLocale)(WORD wLangID);

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

OBJECT_ENTRY_AUTO(__uuidof(FPGADebugProgramProvider2), CFPGADebugProgramProvider2)
