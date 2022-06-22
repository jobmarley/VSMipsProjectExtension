// dllmain.h : Declaration of module class.

class CFPGADebugEngineModule : public ATL::CAtlDllModuleT< CFPGADebugEngineModule >
{
private:
	typedef CAtlDllModuleT<CFPGADebugEngineModule> base;
public :
	DECLARE_LIBID(LIBID_FPGADebugEngineLib)
	DECLARE_REGISTRY_APPID_RESOURCEID(IDR_FPGADEBUGENGINE, "{84f4a577-a2f8-49f0-aa2c-9526af8df3ee}")

	HRESULT RegisterServer(BOOL bRegTypeLib = 0, const CLSID* pCLSID = 0);
	HRESULT UnregisterServer(BOOL bUnRegTypeLib, const CLSID* pCLSID = 0);
};

extern class CFPGADebugEngineModule _AtlModule;
