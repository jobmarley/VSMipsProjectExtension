// FPGADebugEngine.cpp : Implementation of DLL Exports.


#include "pch.h"
#include "framework.h"
#include "resource.h"
#include "FPGADebugEngine_i.h"
#include "dllmain.h"

#include "FPGADebugEngine.h"

#include "FPGADebugEngineGUIDs.h"

using namespace ATL;

// Used to determine whether the DLL can be unloaded by OLE.
_Use_decl_annotations_
STDAPI DllCanUnloadNow(void)
{
	return _AtlModule.DllCanUnloadNow();
}

// Returns a class factory to create an object of the requested type.
_Use_decl_annotations_
STDAPI DllGetClassObject(_In_ REFCLSID rclsid, _In_ REFIID riid, _Outptr_ LPVOID* ppv)
{
	return _AtlModule.DllGetClassObject(rclsid, riid, ppv);
}

// DllRegisterServer - Adds entries to the system registry.
_Use_decl_annotations_
STDAPI DllRegisterServer(void)
{
	// registers object, typelib and all interfaces in typelib
	HRESULT hr = _AtlModule.DllRegisterServer();
	return hr;
}

// DllUnregisterServer - Removes entries from the system registry.
_Use_decl_annotations_
STDAPI DllUnregisterServer(void)
{
	HRESULT hr = _AtlModule.DllUnregisterServer();
	return hr;
}

// DllInstall - Adds/Removes entries to the system registry per user per machine.
STDAPI DllInstall(BOOL bInstall, _In_opt_  LPCWSTR pszCmdLine)
{
	HRESULT hr = E_FAIL;
	static const wchar_t szUserSwitch[] = L"user";

	if (pszCmdLine != nullptr)
	{
		if (_wcsnicmp(pszCmdLine, szUserSwitch, _countof(szUserSwitch)) == 0)
		{
			ATL::AtlSetPerUserRegistration(true);
		}
	}

	if (bInstall)
	{
		hr = DllRegisterServer();
		if (FAILED(hr))
		{
			DllUnregisterServer();
		}
	}
	else
	{
		hr = DllUnregisterServer();
	}

	return hr;
}

// This registers the debugger for normal version of VS
// For experimental version, VS uses an application hive stored in C:\Users\<user>\AppData\Local\Microsoft\VisualStudio\<version>\privateregistry.bin
// where <version> is something like 17.0_3d6bb8efExp
// We need to open that file in regedit and add the keys by hand. Otherwise the debugger is not found
// COM components are correctly found though
static const WCHAR strRegistrationRoot[] = L"Software\\Microsoft\\VisualStudio\\17.0";

// Check https://docs.microsoft.com/en-us/visualstudio/extensibility/debugger/reference/sdk-helpers-for-debugging?view=vs-2022
// for description of metrics
HRESULT CFPGADebugEngineModule::RegisterServer(BOOL bRegTypeLib, const CLSID* pCLSID)
{
	SetMetric(metrictypeEngine, GUID_MyDebugger_Engine,
		metricName, L"MyDebugger",
		false, strRegistrationRoot);
	SetMetric(metrictypeEngine, GUID_MyDebugger_Engine,
		metricCLSID, CLSID_Engine,
		false, strRegistrationRoot);
	SetMetric(metrictypeEngine, GUID_MyDebugger_Engine,
		metricProgramProvider, CLSID_FPGADebugProgramProvider2,
		false, strRegistrationRoot);

	SetMetric(metrictypePortSupplier, GUID_MyDebugger_PortSupplier,
		metricName, L"MyDebuggerPortSupplier",
		false, strRegistrationRoot);
	SetMetric(metrictypePortSupplier, GUID_MyDebugger_PortSupplier,
		metricCLSID, CLSID_FPGADebugPortSupplier2,
		false, strRegistrationRoot);

	return base::RegisterServer(bRegTypeLib, pCLSID);
}
HRESULT CFPGADebugEngineModule::UnregisterServer(BOOL bUnRegTypeLib, const CLSID* pCLSID)
{
	RemoveMetric(metrictypeEngine, GUID_MyDebugger_Engine, metricName, strRegistrationRoot);
	RemoveMetric(metrictypeEngine, GUID_MyDebugger_Engine, metricCLSID, strRegistrationRoot);
	RemoveMetric(metrictypeEngine, GUID_MyDebugger_Engine, metricProgramProvider, strRegistrationRoot);

	RemoveMetric(metrictypeEngine, GUID_MyDebugger_PortSupplier, metricName, strRegistrationRoot);
	RemoveMetric(metrictypeEngine, GUID_MyDebugger_PortSupplier, metricCLSID, strRegistrationRoot);

	return base::UnregisterServer(bUnRegTypeLib, pCLSID);
}
