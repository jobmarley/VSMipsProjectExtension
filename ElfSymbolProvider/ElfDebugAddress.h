// ElfDebugAddress.h : Declaration of the CElfDebugAddress

#pragma once
#include "resource.h"       // main symbols



#include "ElfSymbolProvider_i.h"



#if defined(_WIN32_WCE) && !defined(_CE_DCOM) && !defined(_CE_ALLOW_SINGLE_THREADED_OBJECTS_IN_MTA)
#error "Single-threaded COM objects are not properly supported on Windows CE platform, such as the Windows Mobile platforms that do not include full DCOM support. Define _CE_ALLOW_SINGLE_THREADED_OBJECTS_IN_MTA to force ATL to support creating single-thread COM object's and allow use of it's single-threaded COM object implementations. The threading model in your rgs file was set to 'Free' as that is the only threading model supported in non DCOM Windows CE platforms."
#endif

using namespace ATL;


EXTERN_C const IID IID_IElfDebugAddress;

MIDL_INTERFACE("7fe412c4-4202-4173-a8fd-d74d2007db5e")
IElfDebugAddress : public IDebugAddress
{
public:
	virtual HRESULT STDMETHODCALLTYPE SetAddress(
		DEBUG_ADDRESS * pAddress) = 0;

	virtual HRESULT STDMETHODCALLTYPE GetAddress(
		DEBUG_ADDRESS* pAddress) = 0;

};

// CElfDebugAddress

class ATL_NO_VTABLE CElfDebugAddress :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CElfDebugAddress, &CLSID_ElfDebugAddress>,
	public IElfDebugAddress
{
	DEBUG_ADDRESS m_address = {};
public:
	CElfDebugAddress()
	{
	}

DECLARE_REGISTRY_RESOURCEID(111)


BEGIN_COM_MAP(CElfDebugAddress)
	COM_INTERFACE_ENTRY(IElfDebugAddress)
	COM_INTERFACE_ENTRY(IDebugAddress)
END_COM_MAP()


STDMETHOD(SetAddress)(
	/* [out] */ __RPC__out DEBUG_ADDRESS* pAddress);
STDMETHOD(GetAddress)(
	/* [out] */ __RPC__out DEBUG_ADDRESS* pAddress);

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

OBJECT_ENTRY_AUTO(__uuidof(ElfDebugAddress), CElfDebugAddress)
