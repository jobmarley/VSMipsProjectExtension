// ElfDebugDocumentContext.h : Declaration of the CElfDebugDocumentContext

#pragma once
#include "resource.h"       // main symbols
#include "ElfModule.h"

#include "ElfSymbolProvider_i.h"



#if defined(_WIN32_WCE) && !defined(_CE_DCOM) && !defined(_CE_ALLOW_SINGLE_THREADED_OBJECTS_IN_MTA)
#error "Single-threaded COM objects are not properly supported on Windows CE platform, such as the Windows Mobile platforms that do not include full DCOM support. Define _CE_ALLOW_SINGLE_THREADED_OBJECTS_IN_MTA to force ATL to support creating single-thread COM object's and allow use of it's single-threaded COM object implementations. The threading model in your rgs file was set to 'Free' as that is the only threading model supported in non DCOM Windows CE platforms."
#endif

using namespace ATL;

EXTERN_C const IID IID_IElfDebugDocumentContext;

MIDL_INTERFACE("A5DF09CD-EE52-44D7-A70F-9769AD41D0A1")
IElfDebugDocumentContext : public IDebugDocumentContext2
{
public:
	virtual HRESULT STDMETHODCALLTYPE Init(IElfSymbolProvider* pSymbolProvider, ElfModule* pModule, IDebugAddress* pAddress, Dwarf_Debug dbg, Dwarf_Line line, Dwarf_Unsigned lang) = 0;

};

// CElfDebugDocumentContext

class ATL_NO_VTABLE CElfDebugDocumentContext :
	public CComObjectRootEx<CComObjectThreadModel>,
	public CComCoClass<CElfDebugDocumentContext, &CLSID_ElfDebugDocumentContext>,
	public IElfDebugDocumentContext
{
	IElfSymbolProvider* m_pSymbolProvider = nullptr;
	Dwarf_Debug m_dbg = nullptr;
	Dwarf_Line m_line = nullptr;
	Dwarf_Unsigned m_lang = 0; // eg. DW_LANG_C

	ElfModule* m_pModule = nullptr;
public:
	CElfDebugDocumentContext()
	{
	}

DECLARE_REGISTRY_RESOURCEID(112)


BEGIN_COM_MAP(CElfDebugDocumentContext)
	COM_INTERFACE_ENTRY(IElfDebugDocumentContext)
	COM_INTERFACE_ENTRY(IDebugDocumentContext2)
END_COM_MAP()


	STDMETHOD(Init)(IElfSymbolProvider* pSymbolProvider, ElfModule* pModule, IDebugAddress* pAddress, Dwarf_Debug dbg, Dwarf_Line line, Dwarf_Unsigned lang);

	STDMETHOD(GetDocument)(
		/* [out] */ __RPC__deref_out_opt IDebugDocument2** ppDocument);

	STDMETHOD(GetName)(
		/* [in] */ GETNAME_TYPE gnType,
		/* [out] */ __RPC__deref_out_opt BSTR* pbstrFileName);

	STDMETHOD(EnumCodeContexts)(
		/* [out] */ __RPC__deref_out_opt IEnumDebugCodeContexts2** ppEnumCodeCxts);

	STDMETHOD(GetLanguageInfo)(
		/* [full][out][in] */ __RPC__deref_opt_inout_opt BSTR* pbstrLanguage,
		/* [full][out][in] */ __RPC__inout_opt GUID* pguidLanguage);

	STDMETHOD(GetStatementRange)(
		/* [full][out][in] */ __RPC__inout_opt TEXT_POSITION* pBegPosition,
		/* [full][out][in] */ __RPC__inout_opt TEXT_POSITION* pEndPosition);

	STDMETHOD(GetSourceRange)(
		/* [full][out][in] */ __RPC__inout_opt TEXT_POSITION* pBegPosition,
		/* [full][out][in] */ __RPC__inout_opt TEXT_POSITION* pEndPosition);

	STDMETHOD(Compare)(
		/* [in] */ DOCCONTEXT_COMPARE compare,
		/* [length_is][size_is][in] */ __RPC__in_ecount_part(dwDocContextSetLen, dwDocContextSetLen) IDebugDocumentContext2** rgpDocContextSet,
		/* [in] */ DWORD dwDocContextSetLen,
		/* [out] */ __RPC__out DWORD* pdwDocContext);

	STDMETHOD(Seek)(
		/* [in] */ int nCount,
		/* [out] */ __RPC__deref_out_opt IDebugDocumentContext2** ppDocContext);

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

OBJECT_ENTRY_AUTO(__uuidof(ElfDebugDocumentContext), CElfDebugDocumentContext)
