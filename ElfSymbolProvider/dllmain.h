// dllmain.h : Declaration of module class.

class CElfSymbolProviderModule : public ATL::CAtlDllModuleT< CElfSymbolProviderModule >
{
public :
	DECLARE_LIBID(LIBID_ElfSymbolProviderLib)
	DECLARE_REGISTRY_APPID_RESOURCEID(IDR_ELFSYMBOLPROVIDER, "{d58c5021-1e35-4bde-854c-80217cada2ae}")
};

extern class CElfSymbolProviderModule _AtlModule;
