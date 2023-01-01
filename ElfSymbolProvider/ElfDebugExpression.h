// ElfDebugExpression.h : Declaration of the CElfDebugExpression

#pragma once
#include "resource.h"       // main symbols



#include "ElfSymbolProvider_i.h"

#include "ElfDie.h"



#if defined(_WIN32_WCE) && !defined(_CE_DCOM) && !defined(_CE_ALLOW_SINGLE_THREADED_OBJECTS_IN_MTA)
#error "Single-threaded COM objects are not properly supported on Windows CE platform, such as the Windows Mobile platforms that do not include full DCOM support. Define _CE_ALLOW_SINGLE_THREADED_OBJECTS_IN_MTA to force ATL to support creating single-thread COM object's and allow use of it's single-threaded COM object implementations. The threading model in your rgs file was set to 'Free' as that is the only threading model supported in non DCOM Windows CE platforms."
#endif

using namespace ATL;

struct IElfDebugStackFrame;

EXTERN_C const IID IID_IElfDebugExpression;

MIDL_INTERFACE("f0e7e2fc-eb52-4d97-90bb-492b4d07a253")
IElfDebugExpression : public IDebugExpression2
{
public:
	virtual HRESULT Init(IElfSymbolProvider* pSymbolProvider, IElfDebugStackFrame* pStackFrame, LPCOLESTR code, ElfDie* pDie, IDebugDocumentContext2* pDocumentContext) = 0;
};

// CElfDebugExpression

class ATL_NO_VTABLE CElfDebugExpression :
	public CComObjectRootEx<CComObjectThreadModel>,
	public CComCoClass<CElfDebugExpression, &CLSID_ElfDebugExpression>,
	public IElfDebugExpression
{
	CComPtr<IMipsDEEventCallback> m_pDEEventCallback;
	std::future<void> m_future;
	CComBSTR m_code;
	ElfDie* m_die = nullptr;
	CComPtr<IDebugProperty2> m_pResult;
	CComPtr<IDebugDocumentContext2> m_pDocumentContext;
	IElfDebugStackFrame* m_pStackFrame = nullptr;
public:
	CElfDebugExpression()
	{
	}

DECLARE_REGISTRY_RESOURCEID(116)


BEGIN_COM_MAP(CElfDebugExpression)
	COM_INTERFACE_ENTRY(IElfDebugExpression)
	COM_INTERFACE_ENTRY(IDebugExpression2)
END_COM_MAP()

STDMETHOD(Init)(IElfSymbolProvider* pSymbolProvider, IElfDebugStackFrame* pStackFrame, LPCOLESTR code, ElfDie* pDie, IDebugDocumentContext2* pDocumentContext);

STDMETHOD(EvaluateAsync)(
	/* [in] */ EVALFLAGS dwFlags,
	/* [in] */ __RPC__in_opt IDebugEventCallback2* pExprCallback);

STDMETHOD(Abort)(void);

STDMETHOD(EvaluateSync)(
	/* [in] */ EVALFLAGS dwFlags,
	/* [in] */ DWORD dwTimeout,
	/* [in] */ __RPC__in_opt IDebugEventCallback2* pExprCallback,
	/* [out] */ __RPC__deref_out_opt IDebugProperty2** ppResult);


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

OBJECT_ENTRY_AUTO(__uuidof(ElfDebugExpression), CElfDebugExpression)
