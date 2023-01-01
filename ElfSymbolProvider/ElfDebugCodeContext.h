// ElfDebugCodeContext.h : Declaration of the CElfDebugCodeContext

#pragma once
#include "resource.h"       // main symbols
#include "ElfDebugDocumentContext.h"
#include "ElfDie.h"
#include "ElfModule.h"

#include "ElfSymbolProvider_i.h"



#if defined(_WIN32_WCE) && !defined(_CE_DCOM) && !defined(_CE_ALLOW_SINGLE_THREADED_OBJECTS_IN_MTA)
#error "Single-threaded COM objects are not properly supported on Windows CE platform, such as the Windows Mobile platforms that do not include full DCOM support. Define _CE_ALLOW_SINGLE_THREADED_OBJECTS_IN_MTA to force ATL to support creating single-thread COM object's and allow use of it's single-threaded COM object implementations. The threading model in your rgs file was set to 'Free' as that is the only threading model supported in non DCOM Windows CE platforms."
#endif

using namespace ATL;

EXTERN_C const IID IID_IElfDebugCodeContext;

MIDL_INTERFACE("388d8dc0-e693-494e-b7e1-d71e00b8b8db")
IElfDebugCodeContext : public IDebugCodeContext2
{
public:
	virtual HRESULT Init(IElfSymbolProvider* pSymbolProvider, ElfModule* pModule, IDebugAddress* pAddress) = 0;
};

// CElfDebugCodeContext

// This represent debug info about an instruction
// If we dont have debug info about that location we shouldnt have much in there
class ATL_NO_VTABLE CElfDebugCodeContext :
	public CComObjectRootEx<CComObjectThreadModel>,
	public CComCoClass<CElfDebugCodeContext, &CLSID_ElfDebugCodeContext>,
	public IElfDebugCodeContext
{
	ElfModule* m_pModule = nullptr;
	ElfDie* m_function = nullptr;
	std::wstring m_name;
	IElfSymbolProvider* m_pSymbolProvider = nullptr;

	CComPtr<IDebugAddress> m_pAddress;
	uint32_t m_addressValue = 0;
public:
	CElfDebugCodeContext()
	{
	}

DECLARE_REGISTRY_RESOURCEID(113)


BEGIN_COM_MAP(CElfDebugCodeContext)
	COM_INTERFACE_ENTRY(IElfDebugCodeContext)
	COM_INTERFACE_ENTRY(IDebugCodeContext2)
	COM_INTERFACE_ENTRY(IDebugMemoryContext2)
END_COM_MAP()

STDMETHOD(Init)(IElfSymbolProvider* pSymbolProvider, ElfModule* pModule, IDebugAddress* pAddress);

STDMETHOD(GetDocumentContext)(
	/* [out] */ __RPC__deref_out_opt IDebugDocumentContext2** ppSrcCxt);

STDMETHOD(GetLanguageInfo)(
	/* [full][out][in] */ __RPC__deref_opt_inout_opt BSTR* pbstrLanguage,
	/* [full][out][in] */ __RPC__inout_opt GUID* pguidLanguage);


STDMETHOD(GetName)(
	/* [out] */ __RPC__deref_out_opt BSTR* pbstrName);

STDMETHOD(GetInfo)(
	/* [in] */ CONTEXT_INFO_FIELDS dwFields,
	/* [out] */ __RPC__out CONTEXT_INFO* pInfo);

STDMETHOD(Add)(
	/* [in] */ UINT64 dwCount,
	/* [out] */ __RPC__deref_out_opt IDebugMemoryContext2** ppMemCxt);

STDMETHOD(Subtract)(
	/* [in] */ UINT64 dwCount,
	/* [out] */ __RPC__deref_out_opt IDebugMemoryContext2** ppMemCxt);

STDMETHOD(Compare)(
	/* [in] */ CONTEXT_COMPARE compare,
	/* [length_is][size_is][in] */ __RPC__in_ecount_part(dwMemoryContextSetLen, dwMemoryContextSetLen) IDebugMemoryContext2** rgpMemoryContextSet,
	/* [in] */ DWORD dwMemoryContextSetLen,
	/* [out] */ __RPC__out DWORD* pdwMemoryContext);

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

OBJECT_ENTRY_AUTO(__uuidof(ElfDebugCodeContext), CElfDebugCodeContext)
