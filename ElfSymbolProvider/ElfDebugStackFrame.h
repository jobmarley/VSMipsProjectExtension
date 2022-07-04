// ElfDebugStackFrame.h : Declaration of the CElfDebugStackFrame

#pragma once
#include "resource.h"       // main symbols
#include "ElfModule.h"


#include "ElfSymbolProvider_i.h"



#if defined(_WIN32_WCE) && !defined(_CE_DCOM) && !defined(_CE_ALLOW_SINGLE_THREADED_OBJECTS_IN_MTA)
#error "Single-threaded COM objects are not properly supported on Windows CE platform, such as the Windows Mobile platforms that do not include full DCOM support. Define _CE_ALLOW_SINGLE_THREADED_OBJECTS_IN_MTA to force ATL to support creating single-thread COM object's and allow use of it's single-threaded COM object implementations. The threading model in your rgs file was set to 'Free' as that is the only threading model supported in non DCOM Windows CE platforms."
#endif

using namespace ATL;

EXTERN_C const IID IID_IElfDebugStackFrame;

MIDL_INTERFACE("bbac5e6b-eb45-4690-9370-beac1e2e6a11")
IElfDebugStackFrame : public IDebugStackFrame2
{
public:
    virtual HRESULT Init(IDebugAddress* pAddress, IDebugThread2* pThread, ElfModule* pModule) = 0;
};

// CElfDebugStackFrame

class ATL_NO_VTABLE CElfDebugStackFrame :
	public CComObjectRootEx<CComSingleThreadModel>,
	public CComCoClass<CElfDebugStackFrame, &CLSID_ElfDebugStackFrame>,
	public IElfDebugStackFrame
{
    CComPtr<IDebugThread2> m_pThread;
    ElfModule* m_pModule = nullptr;
    CComPtr<IDebugAddress> m_pAddress;
    CComPtr<IDebugDocumentContext2> m_pDocumentContext;
    CComPtr<IDebugCodeContext2> m_pCodeContext;


    CComBSTR GetFunctionName();
public:
	CElfDebugStackFrame()
	{
	}

DECLARE_REGISTRY_RESOURCEID(114)


BEGIN_COM_MAP(CElfDebugStackFrame)
	COM_INTERFACE_ENTRY(IElfDebugStackFrame)
	COM_INTERFACE_ENTRY(IDebugStackFrame2)
END_COM_MAP()

STDMETHOD(Init)(IDebugAddress* pAddress, IDebugThread2* pThread, ElfModule* pModule);

STDMETHOD(GetCodeContext)(
    /* [out] */ __RPC__deref_out_opt IDebugCodeContext2** ppCodeCxt);

STDMETHOD(GetDocumentContext)(
    /* [out] */ __RPC__deref_out_opt IDebugDocumentContext2** ppCxt);

STDMETHOD(GetName)(
    /* [out] */ __RPC__deref_out_opt BSTR* pbstrName);

STDMETHOD(GetInfo)(
    /* [in] */ FRAMEINFO_FLAGS dwFieldSpec,
    /* [in] */ UINT nRadix,
    /* [out] */ __RPC__out FRAMEINFO* pFrameInfo);

STDMETHOD(GetPhysicalStackRange)(
    /* [out] */ __RPC__out UINT64* paddrMin,
    /* [out] */ __RPC__out UINT64* paddrMax);

STDMETHOD(GetExpressionContext)(
    /* [out] */ __RPC__deref_out_opt IDebugExpressionContext2** ppExprCxt);

STDMETHOD(GetLanguageInfo)(
    /* [full][out][in] */ __RPC__deref_opt_inout_opt BSTR* pbstrLanguage,
    /* [full][out][in] */ __RPC__inout_opt GUID* pguidLanguage);

STDMETHOD(GetDebugProperty)(
    /* [out] */ __RPC__deref_out_opt IDebugProperty2** ppProperty);

STDMETHOD(EnumProperties)(
    /* [in] */ DEBUGPROP_INFO_FLAGS dwFields,
    /* [in] */ UINT nRadix,
    /* [in] */ __RPC__in REFGUID guidFilter,
    /* [in] */ DWORD dwTimeout,
    /* [out] */ __RPC__out ULONG* pcelt,
    /* [out] */ __RPC__deref_out_opt IEnumDebugPropertyInfo2** ppEnum);

STDMETHOD(GetThread)(
    /* [out] */ __RPC__deref_out_opt IDebugThread2** ppThread);


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

OBJECT_ENTRY_AUTO(__uuidof(ElfDebugStackFrame), CElfDebugStackFrame)
