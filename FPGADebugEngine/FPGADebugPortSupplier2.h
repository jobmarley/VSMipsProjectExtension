// FPGADebugPortSupplier2.h : Declaration of the CFPGADebugPortSupplier2

#pragma once
#include "resource.h"       // main symbols



#include "FPGADebugEngine_i.h"

#include <unordered_map>
#include <string_view>

#if defined(_WIN32_WCE) && !defined(_CE_DCOM) && !defined(_CE_ALLOW_SINGLE_THREADED_OBJECTS_IN_MTA)
#error "Single-threaded COM objects are not properly supported on Windows CE platform, such as the Windows Mobile platforms that do not include full DCOM support. Define _CE_ALLOW_SINGLE_THREADED_OBJECTS_IN_MTA to force ATL to support creating single-thread COM object's and allow use of it's single-threaded COM object implementations. The threading model in your rgs file was set to 'Free' as that is the only threading model supported in non DCOM Windows CE platforms."
#endif

using namespace ATL;

// CFPGADebugPortSupplier2

namespace std
{
	template<>
	struct hash<GUID>
	{
		size_t operator()(const GUID& g) const
		{
			std::hash<std::string_view> h;
			std::string_view s((const char*)&g, sizeof(GUID));
			return h(s);
		}
	};
}

class ATL_NO_VTABLE CFPGADebugPortSupplier2 :
	public CComObjectRootEx<CComSingleThreadModel>,
	public CComCoClass<CFPGADebugPortSupplier2, &CLSID_FPGADebugPortSupplier2>,
	public IDebugPortSupplier2
{
	std::unordered_map<GUID, ATL::CComPtr<IDebugPort2>> m_ports;
public:
	CFPGADebugPortSupplier2()
	{
	}

DECLARE_REGISTRY_RESOURCEID(117)


BEGIN_COM_MAP(CFPGADebugPortSupplier2)
	COM_INTERFACE_ENTRY(IDebugPortSupplier2)
END_COM_MAP()

	STDMETHOD(GetPortSupplierName)(BSTR* pbstrName);
	STDMETHOD(GetPortSupplierId)(GUID* pguidPortSupplier);
	STDMETHOD(GetPort)(REFGUID guidPort, IDebugPort2** ppPort);
	STDMETHOD(EnumPorts)(IEnumDebugPorts2** ppEnum);
	STDMETHOD(CanAddPort)(void);
	STDMETHOD(AddPort)(IDebugPortRequest2* pRequest, IDebugPort2** ppPort);
	STDMETHOD(RemovePort)(IDebugPort2* pPort);

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

OBJECT_ENTRY_AUTO(__uuidof(FPGADebugPortSupplier2), CFPGADebugPortSupplier2)
