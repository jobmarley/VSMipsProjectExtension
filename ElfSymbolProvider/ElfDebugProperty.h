// ElfDebugProperty.h : Declaration of the CElfDebugProperty

#pragma once
#include "resource.h"       // main symbols
#include "ElfDie.h"

#include "ElfSymbolProvider_i.h"



#if defined(_WIN32_WCE) && !defined(_CE_DCOM) && !defined(_CE_ALLOW_SINGLE_THREADED_OBJECTS_IN_MTA)
#error "Single-threaded COM objects are not properly supported on Windows CE platform, such as the Windows Mobile platforms that do not include full DCOM support. Define _CE_ALLOW_SINGLE_THREADED_OBJECTS_IN_MTA to force ATL to support creating single-thread COM object's and allow use of it's single-threaded COM object implementations. The threading model in your rgs file was set to 'Free' as that is the only threading model supported in non DCOM Windows CE platforms."
#endif

using namespace ATL;

EXTERN_C const IID IID_IElfDebugExpression;

struct IElfDebugStackFrame;

MIDL_INTERFACE("f0e7e2fc-eb52-4d97-90bb-492b4d07a253")
IElfDebugProperty : public IDebugProperty2
{
public:
	virtual HRESULT Init(BSTR name, ElfType type, DWORD address, IDebugDocumentContext2* pDocumentContext, IElfDebugStackFrame* pStackFrame) = 0;
};

// CElfDebugProperty
// This represent a variable to be displayed in eg. the local window in the debugger
// It has an address, a type and a name
class ATL_NO_VTABLE CElfDebugProperty :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CElfDebugProperty, &CLSID_ElfDebugProperty>,
	public IElfDebugProperty
{
    uint32_t m_address = 0;
    std::unique_ptr<ElfType> m_type;
    CComPtr<IDebugDocumentContext2> m_pDocumentContext;
    IElfDebugStackFrame* m_pStackFrame = nullptr;
    std::vector<CComPtr<IElfDebugProperty>> m_children;
    bool m_childrenEnumerated = false;
    CComBSTR m_name;

    std::wstring GetValue(DWORD radix);
public:
	CElfDebugProperty()
	{
	}

DECLARE_REGISTRY_RESOURCEID(117)


BEGIN_COM_MAP(CElfDebugProperty)
    COM_INTERFACE_ENTRY(IElfDebugProperty)
    COM_INTERFACE_ENTRY(IDebugProperty2)
END_COM_MAP()


STDMETHOD(Init)(BSTR name, ElfType type, DWORD address, IDebugDocumentContext2* pDocumentContext, IElfDebugStackFrame* pStackFrame);

STDMETHOD(GetPropertyInfo)(
    /* [in] */ DEBUGPROP_INFO_FLAGS dwFields,
    /* [in] */ DWORD dwRadix,
    /* [in] */ DWORD dwTimeout,
    /* [length_is][size_is][full][in] */ __RPC__in_ecount_part_opt(dwArgCount, dwArgCount) IDebugReference2** rgpArgs,
    /* [in] */ DWORD dwArgCount,
    /* [out] */ __RPC__out DEBUG_PROPERTY_INFO* pPropertyInfo);

STDMETHOD(SetValueAsString)(
    /* [in] */ __RPC__in LPCOLESTR pszValue,
    /* [in] */ DWORD dwRadix,
    /* [in] */ DWORD dwTimeout);

STDMETHOD(SetValueAsReference)(
    /* [length_is][size_is][full][in] */ __RPC__in_ecount_part_opt(dwArgCount, dwArgCount) IDebugReference2** rgpArgs,
    /* [in] */ DWORD dwArgCount,
    /* [in] */ __RPC__in_opt IDebugReference2* pValue,
    /* [in] */ DWORD dwTimeout);

STDMETHOD(EnumChildren)(
    /* [in] */ DEBUGPROP_INFO_FLAGS dwFields,
    /* [in] */ DWORD dwRadix,
    /* [in] */ __RPC__in REFGUID guidFilter,
    /* [in] */ DBG_ATTRIB_FLAGS dwAttribFilter,
    /* [full][in] */ __RPC__in_opt LPCOLESTR pszNameFilter,
    /* [in] */ DWORD dwTimeout,
    /* [out] */ __RPC__deref_out_opt IEnumDebugPropertyInfo2** ppEnum);

STDMETHOD(GetParent)(
    /* [out] */ __RPC__deref_out_opt IDebugProperty2** ppParent);

STDMETHOD(GetDerivedMostProperty)(
    /* [out] */ __RPC__deref_out_opt IDebugProperty2** ppDerivedMost);

STDMETHOD(GetMemoryBytes)(
    /* [out] */ __RPC__deref_out_opt IDebugMemoryBytes2** ppMemoryBytes);

STDMETHOD(GetMemoryContext)(
    /* [out] */ __RPC__deref_out_opt IDebugMemoryContext2** ppMemory);

STDMETHOD(GetSize)(
    /* [out] */ __RPC__out DWORD* pdwSize);

STDMETHOD(GetReference)(
    /* [out] */ __RPC__deref_out_opt IDebugReference2** ppReference);

STDMETHOD(GetExtendedInfo)(
    /* [in] */ __RPC__in REFGUID guidExtendedInfo,
    /* [out] */ __RPC__out VARIANT* pExtendedInfo);

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

OBJECT_ENTRY_AUTO(__uuidof(ElfDebugProperty), CElfDebugProperty)
