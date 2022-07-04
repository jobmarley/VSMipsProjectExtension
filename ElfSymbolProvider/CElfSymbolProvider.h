// CElfSymbolProvider.h : Declaration of the CElfSymbolProvider

#pragma once
#include "resource.h"       // main symbols



#include "ElfSymbolProvider_i.h"

#include "ElfModule.h"

#if defined(_WIN32_WCE) && !defined(_CE_DCOM) && !defined(_CE_ALLOW_SINGLE_THREADED_OBJECTS_IN_MTA)
#error "Single-threaded COM objects are not properly supported on Windows CE platform, such as the Windows Mobile platforms that do not include full DCOM support. Define _CE_ALLOW_SINGLE_THREADED_OBJECTS_IN_MTA to force ATL to support creating single-thread COM object's and allow use of it's single-threaded COM object implementations. The threading model in your rgs file was set to 'Free' as that is the only threading model supported in non DCOM Windows CE platforms."
#endif

using namespace ATL;

// CElfSymbolProvider

class ATL_NO_VTABLE CElfSymbolProvider :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CElfSymbolProvider, &CLSID_ElfSymbolProvider>,
	public IElfSymbolProvider
{
    std::unordered_map<std::string, std::unique_ptr<ElfModule>> m_modules;
    // Map because of lowerbound
    std::map<DWORD, GUID> m_modulesByAddress;
    std::unordered_map<GUID, ElfModule*> m_modulesByGuid;
    std::mutex m_mutex;
public:
	CElfSymbolProvider()
	{
	}
    ~CElfSymbolProvider()
    {
    }

DECLARE_REGISTRY_RESOURCEID(107)


BEGIN_COM_MAP(CElfSymbolProvider)
	COM_INTERFACE_ENTRY(IElfSymbolProvider)
	COM_INTERFACE_ENTRY(IDebugSymbolProvider)
END_COM_MAP()

    STDMETHOD(GetModuleFromAddress)(DWORD address, GUID* pGuid);

    STDMETHOD(GetAddressFromMemory)(
        DWORD memAddr,
        IDebugAddress** ppAddress);

    STDMETHOD(LoadModule)(
        IDebugModule2* pDebugModule,
        LPCOLESTR pszFilepath,
        DWORD address);

    STDMETHOD(GetStackFrame)(IDebugAddress* pAddress, IDebugThread2* pThread, IDebugStackFrame2** ppStackFrame);

    STDMETHOD(Initialize)(
        /* [in] */ __RPC__in_opt IDebugEngineSymbolProviderServices* pServices);

    STDMETHOD(Uninitialize)(void);

    STDMETHOD(GetContainerField)(
        /* [in] */ __RPC__in_opt IDebugAddress* pAddress,
        /* [out] */ __RPC__deref_out_opt IDebugContainerField** ppContainerField);

    STDMETHOD(GetField)(
        /* [in] */ __RPC__in_opt IDebugAddress* pAddress,
        /* [in] */ __RPC__in_opt IDebugAddress* pAddressCur,
        /* [out] */ __RPC__deref_out_opt IDebugField** ppField);

    STDMETHOD(GetAddressesFromPosition)(
        /* [in] */ __RPC__in_opt IDebugDocumentPosition2* pDocPos,
        /* [in] */ BOOL fStatmentOnly,
        /* [out] */ __RPC__deref_out_opt IEnumDebugAddresses** ppEnumBegAddresses,
        /* [out] */ __RPC__deref_out_opt IEnumDebugAddresses** ppEnumEndAddresses);

    STDMETHOD(GetAddressesFromContext)(
        /* [in] */ __RPC__in_opt IDebugDocumentContext2* pDocContext,
        /* [in] */ BOOL fStatmentOnly,
        /* [out] */ __RPC__deref_out_opt IEnumDebugAddresses** ppEnumBegAddresses,
        /* [out] */ __RPC__deref_out_opt IEnumDebugAddresses** ppEnumEndAddresses);

    STDMETHOD(GetContextFromAddress)(
        /* [in] */ __RPC__in_opt IDebugAddress* pAddress,
        /* [out] */ __RPC__deref_out_opt IDebugDocumentContext2** ppDocContext);

    STDMETHOD(GetLanguage)(
        /* [in] */ __RPC__in_opt IDebugAddress* pAddress,
        /* [out] */ __RPC__out GUID* pguidLanguage,
        /* [out] */ __RPC__out GUID* pguidLanguageVendor);

    STDMETHOD(GetGlobalContainer)(
        /* [out] */ __RPC__deref_out_opt IDebugContainerField** pField);

    STDMETHOD(GetMethodFieldsByName)(
        /* [full][in] */ __RPC__in_opt LPCOLESTR pszFullName,
        /* [in] */ NAME_MATCH nameMatch,
        /* [out] */ __RPC__deref_out_opt IEnumDebugFields** ppEnum);

    STDMETHOD(GetClassTypeByName)(
        /* [full][in] */ __RPC__in_opt LPCOLESTR pszClassName,
        /* [in] */ NAME_MATCH nameMatch,
        /* [out] */ __RPC__deref_out_opt IDebugClassField** ppField);

    STDMETHOD(GetNamespacesUsedAtAddress)(
        /* [in] */ __RPC__in_opt IDebugAddress* pAddress,
        /* [out] */ __RPC__deref_out_opt IEnumDebugFields** ppEnum);

    STDMETHOD(GetTypeByName)(
        /* [full][in] */ __RPC__in_opt LPCOLESTR pszClassName,
        /* [in] */ NAME_MATCH nameMatch,
        /* [out] */ __RPC__deref_out_opt IDebugField** ppField);

    STDMETHOD(GetNextAddress)(
        /* [in] */ __RPC__in_opt IDebugAddress* pAddress,
        /* [in] */ BOOL fStatmentOnly,
        /* [out] */ __RPC__deref_out_opt IDebugAddress** ppAddress);


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

OBJECT_ENTRY_AUTO(__uuidof(ElfSymbolProvider), CElfSymbolProvider)
