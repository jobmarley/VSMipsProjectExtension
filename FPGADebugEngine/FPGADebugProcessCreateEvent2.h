// FPGADebugProcessCreateEvent2.h : Declaration of the CFPGADebugProcessCreateEvent2

#pragma once
#include "resource.h"       // main symbols



#include "FPGADebugEngine_i.h"
#include "FPGADebugEvent2.h"


#if defined(_WIN32_WCE) && !defined(_CE_DCOM) && !defined(_CE_ALLOW_SINGLE_THREADED_OBJECTS_IN_MTA)
#error "Single-threaded COM objects are not properly supported on Windows CE platform, such as the Windows Mobile platforms that do not include full DCOM support. Define _CE_ALLOW_SINGLE_THREADED_OBJECTS_IN_MTA to force ATL to support creating single-thread COM object's and allow use of it's single-threaded COM object implementations. The threading model in your rgs file was set to 'Free' as that is the only threading model supported in non DCOM Windows CE platforms."
#endif

using namespace ATL;


// CFPGADebugProcessCreateEvent2



class ATL_NO_VTABLE CFPGADebugProcessCreateEvent2 :
	public CComObjectRootEx<CComSingleThreadModel>,
	public CComCoClass<CFPGADebugProcessCreateEvent2, &CLSID_FPGADebugProcessCreateEvent2>,
	public IDebugProcessCreateEvent2,
	public IFPGADebugEvent2
{
	DWORD m_attributes = 0;
public:
	CFPGADebugProcessCreateEvent2()
	{
	}

DECLARE_REGISTRY_RESOURCEID(114)


BEGIN_COM_MAP(CFPGADebugProcessCreateEvent2)
	COM_INTERFACE_ENTRY(IDebugProcessCreateEvent2)
	COM_INTERFACE_ENTRY(IFPGADebugEvent2)
	COM_INTERFACE_ENTRY(IDebugEvent2)
END_COM_MAP()

	STDMETHOD(SetAttributes)(DWORD dwAttrib);
	STDMETHOD(GetAttributes)(DWORD* pdwAttrib);

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

OBJECT_ENTRY_AUTO(__uuidof(FPGADebugProcessCreateEvent2), CFPGADebugProcessCreateEvent2)
