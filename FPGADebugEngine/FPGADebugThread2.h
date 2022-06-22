// FPGADebugThread2.h : Declaration of the CFPGADebugThread2

#pragma once
#include "resource.h"       // main symbols



#include "FPGADebugEngine_i.h"



#if defined(_WIN32_WCE) && !defined(_CE_DCOM) && !defined(_CE_ALLOW_SINGLE_THREADED_OBJECTS_IN_MTA)
#error "Single-threaded COM objects are not properly supported on Windows CE platform, such as the Windows Mobile platforms that do not include full DCOM support. Define _CE_ALLOW_SINGLE_THREADED_OBJECTS_IN_MTA to force ATL to support creating single-thread COM object's and allow use of it's single-threaded COM object implementations. The threading model in your rgs file was set to 'Free' as that is the only threading model supported in non DCOM Windows CE platforms."
#endif

using namespace ATL;


struct IFPGADebugProgram;

// CFPGADebugThread2

// {00FD57F9-9584-45E9-A104-C6913DB7CBD5}
DEFINE_GUID(IID_IFPGADebugThread2,
	0xfd57f9, 0x9584, 0x45e9, 0xa1, 0x4, 0xc6, 0x91, 0x3d, 0xb7, 0xcb, 0xd5);

MIDL_INTERFACE("00FD57F9-9584-45E9-A104-C6913DB7CBD5")
IFPGADebugThread2
	: IDebugThread2
{
	virtual HRESULT Init(const WCHAR* name, IFPGADebugProgram* pProgram) = 0;
};

class ATL_NO_VTABLE CFPGADebugThread2 :
	public CComObjectRootEx<CComSingleThreadModel>,
	public CComCoClass<CFPGADebugThread2, &CLSID_FPGADebugThread2>,
	public IFPGADebugThread2
{
	IFPGADebugProgram* m_pProgram = nullptr;
	std::wstring m_name;
	DWORD m_threadID = 0;
public:
	CFPGADebugThread2()
	{
	}

DECLARE_REGISTRY_RESOURCEID(115)


BEGIN_COM_MAP(CFPGADebugThread2)
	COM_INTERFACE_ENTRY(IFPGADebugThread2)
	COM_INTERFACE_ENTRY(IDebugThread2)
END_COM_MAP()

	STDMETHOD(Init)(const WCHAR* name, IFPGADebugProgram* pProgram);

	//////////////////////////////////////////////////////////// 
	//IDebugThread2 
	STDMETHOD(EnumFrameInfo)(FRAMEINFO_FLAGS dwFieldSpec, UINT nRadix, IEnumDebugFrameInfo2** ppEnum);
	STDMETHOD(GetName)(BSTR* pbstrName);
	STDMETHOD(SetThreadName)(LPCOLESTR pszName);
	STDMETHOD(GetProgram)(IDebugProgram2** ppProgram);
	STDMETHOD(CanSetNextStatement)(IDebugStackFrame2* pStackFrame, IDebugCodeContext2* pCodeContext);
	STDMETHOD(SetNextStatement)(IDebugStackFrame2* pStackFrame, IDebugCodeContext2* pCodeContext);
	STDMETHOD(GetThreadId)(DWORD* pdwThreadId);
	STDMETHOD(Suspend)(DWORD* pdwSuspendCount);
	STDMETHOD(Resume)(DWORD* pdwSuspendCount);
	STDMETHOD(GetThreadProperties)(THREADPROPERTY_FIELDS dwFields, THREADPROPERTIES* ptp);
	STDMETHOD(GetLogicalThread)(IDebugStackFrame2* pStackFrame, IDebugLogicalThread2** ppLogicalThread);


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

OBJECT_ENTRY_AUTO(__uuidof(FPGADebugThread2), CFPGADebugThread2)
