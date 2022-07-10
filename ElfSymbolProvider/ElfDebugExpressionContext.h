// ElfDebugExpressionContext.h : Declaration of the CElfDebugExpressionContext

#pragma once
#include "resource.h"       // main symbols



#include "ElfSymbolProvider_i.h"

#include "ElfDie.h"

#if defined(_WIN32_WCE) && !defined(_CE_DCOM) && !defined(_CE_ALLOW_SINGLE_THREADED_OBJECTS_IN_MTA)
#error "Single-threaded COM objects are not properly supported on Windows CE platform, such as the Windows Mobile platforms that do not include full DCOM support. Define _CE_ALLOW_SINGLE_THREADED_OBJECTS_IN_MTA to force ATL to support creating single-thread COM object's and allow use of it's single-threaded COM object implementations. The threading model in your rgs file was set to 'Free' as that is the only threading model supported in non DCOM Windows CE platforms."
#endif

using namespace ATL;


EXTERN_C const IID IID_IElfDebugExpressionContext;

struct IElfDebugStackFrame;

MIDL_INTERFACE("adca9a5b-5c0d-4b3f-9a86-928422c6953d")
IElfDebugExpressionContext : public IDebugExpressionContext2
{
public:
	virtual HRESULT Init(IElfSymbolProvider* pSymbolProvider, IElfDebugStackFrame* pStackFrame, ElfDie* pDie, IDebugDocumentContext2* pDocumentContext) = 0;
};

// CElfDebugExpressionContext

class ATL_NO_VTABLE CElfDebugExpressionContext :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CElfDebugExpressionContext, &CLSID_ElfDebugExpressionContext>,
	public IElfDebugExpressionContext
{
	CComPtr<IElfSymbolProvider> m_pSymbolProvider;
	ElfDie* m_pDie = nullptr;
	CComPtr<IDebugDocumentContext2> m_pDocumentContext;
	CComPtr<IElfDebugStackFrame> m_pStackFrame;
public:
	CElfDebugExpressionContext()
	{
	}

DECLARE_REGISTRY_RESOURCEID(115)


BEGIN_COM_MAP(CElfDebugExpressionContext)
	COM_INTERFACE_ENTRY(IElfDebugExpressionContext)
	COM_INTERFACE_ENTRY(IDebugExpressionContext2)
END_COM_MAP()

STDMETHOD(Init)(IElfSymbolProvider* pSymbolProvider, IElfDebugStackFrame* pStackFrame, ElfDie* pDie, IDebugDocumentContext2* pDocumentContext);

STDMETHOD(GetName)(
	/* [out] */ __RPC__deref_out_opt BSTR* pbstrName);

STDMETHOD(ParseText)(
	/* [in] */ __RPC__in LPCOLESTR pszCode,
	/* [in] */ PARSEFLAGS dwFlags,
	/* [in] */ UINT nRadix,
	/* [out] */ __RPC__deref_out_opt IDebugExpression2** ppExpr,
	/* [out] */ __RPC__deref_out_opt BSTR* pbstrError,
	/* [out] */ __RPC__out UINT* pichError);


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

OBJECT_ENTRY_AUTO(__uuidof(ElfDebugExpressionContext), CElfDebugExpressionContext)
