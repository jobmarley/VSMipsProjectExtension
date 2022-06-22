// Engine.h : Declaration of the CEngine

#pragma once
#include "resource.h"       // main symbols



#include "FPGADebugEngine_i.h"

#include "FPGADebugProcess.h"
#include "FPGADebugEngineGUIDs.h"

#if defined(_WIN32_WCE) && !defined(_CE_DCOM) && !defined(_CE_ALLOW_SINGLE_THREADED_OBJECTS_IN_MTA)
#error "Single-threaded COM objects are not properly supported on Windows CE platform, such as the Windows Mobile platforms that do not include full DCOM support. Define _CE_ALLOW_SINGLE_THREADED_OBJECTS_IN_MTA to force ATL to support creating single-thread COM object's and allow use of it's single-threaded COM object implementations. The threading model in your rgs file was set to 'Free' as that is the only threading model supported in non DCOM Windows CE platforms."
#endif

using namespace ATL;

// {E13B43AC-F1B2-4636-A20A-364ACB053A04}
//DEFINE_GUID(CLSID_Engine,
//	0xe13b43ac, 0xf1b2, 0x4636, 0xa2, 0xa, 0x36, 0x4a, 0xcb, 0x5, 0x3a, 0x4);


// REGEX:
// STDMETHOD\((\w+)\)([^\;]+)\;
// HRESULT CFPGADebugProgram::$1$2\n{\n\treturn E_NOTIMPL;\n}

// CEngine

class ATL_NO_VTABLE CEngine :
	public CComObjectRootEx<CComSingleThreadModel>,
	public CComCoClass<CEngine, &CLSID_Engine>,
	public IDebugEngine2,
	public IDebugEngineLaunch2
{
	CComPtr<IDebugEventCallback2> m_pCallback;
	CComPtr<IFPGADebugProcess> m_pProcess;
	std::wstring m_registryRoot;
public:
	CEngine()
	{
	}

DECLARE_REGISTRY_RESOURCEID(106)


BEGIN_COM_MAP(CEngine)
	COM_INTERFACE_ENTRY(IDebugEngine2)
	COM_INTERFACE_ENTRY(IDebugEngineLaunch2)
END_COM_MAP()

	//////////////////////////////////////////////////////////// 
	//IDebugEngine2 
	STDMETHOD(EnumPrograms)(IEnumDebugPrograms2** ppEnum);
	STDMETHOD(Attach)(
		IDebugProgram2** rgpPrograms,
		IDebugProgramNode2** rgpProgramNodes,
		DWORD celtPrograms,
		IDebugEventCallback2* pCallback,
		ATTACH_REASON dwReason);
	STDMETHOD(CreatePendingBreakpoint)(
		IDebugBreakpointRequest2* pBPRequest,
		IDebugPendingBreakpoint2** ppPendingBP);
	STDMETHOD(SetException)(EXCEPTION_INFO* pException);
	STDMETHOD(RemoveSetException)(EXCEPTION_INFO* pException);
	STDMETHOD(RemoveAllSetExceptions)(REFGUID guidType);
	STDMETHOD(GetEngineId)(GUID* pguidEngine);
	STDMETHOD(DestroyProgram)(IDebugProgram2* pProgram);
	STDMETHOD(ContinueFromSynchronousEvent)(IDebugEvent2* pEvent);
	STDMETHOD(SetLocale)(WORD wLangID);
	STDMETHOD(SetRegistryRoot)(LPCOLESTR pszRegistryRoot);
	STDMETHOD(SetMetric)(LPCOLESTR pszMetric, VARIANT varValue);
	STDMETHOD(CauseBreak)(void);
	//////////////////////////////////////////////////////////// 
	//IDebugEngineLaunch2 
	STDMETHOD(LaunchSuspended)(
		LPCOLESTR pszServer,
		IDebugPort2* pPort,
		LPCOLESTR pszExe,
		LPCOLESTR pszArgs,
		LPCOLESTR pszDir,
		BSTR bstrEnv,
		LPCOLESTR pszOptions,
		LAUNCH_FLAGS dwLaunchFlags,
		DWORD hStdInput,
		DWORD hStdOutput,
		DWORD hStdError,
		IDebugEventCallback2* pCallback,
		IDebugProcess2** ppProcess);
	STDMETHOD(ResumeProcess)(IDebugProcess2* pProcess);
	STDMETHOD(CanTerminateProcess)(IDebugProcess2* pProcess);
	STDMETHOD(TerminateProcess)(IDebugProcess2* pProcess);

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

OBJECT_ENTRY_AUTO(CLSID_Engine, CEngine)
