// FPGADebugProgram.h : Declaration of the CFPGADebugProgram

#pragma once
#include "resource.h"       // main symbols



#include "FPGADebugEngine_i.h"
#include "FPGADebugThread2.h"


#if defined(_WIN32_WCE) && !defined(_CE_DCOM) && !defined(_CE_ALLOW_SINGLE_THREADED_OBJECTS_IN_MTA)
#error "Single-threaded COM objects are not properly supported on Windows CE platform, such as the Windows Mobile platforms that do not include full DCOM support. Define _CE_ALLOW_SINGLE_THREADED_OBJECTS_IN_MTA to force ATL to support creating single-thread COM object's and allow use of it's single-threaded COM object implementations. The threading model in your rgs file was set to 'Free' as that is the only threading model supported in non DCOM Windows CE platforms."
#endif

using namespace ATL;

struct IFPGADebugProcess;
struct IFPGADebugThread2;

// {33F1BB53-5952-4B5F-B97F-631888D0215C}
//DEFINE_GUID(<< name >> ,
//    0x33f1bb53, 0x5952, 0x4b5f, 0xb9, 0x7f, 0x63, 0x18, 0x88, 0xd0, 0x21, 0x5c);

MIDL_INTERFACE("33F1BB53-5952-4B5F-B97F-631888D0215C")
IFPGADebugProgram
    : IDebugProgram2
{
    virtual HRESULT Init(IFPGADebugProcess* process, const WCHAR * name) = 0;
    virtual HRESULT GetThread(IFPGADebugThread2** ppThread) = 0;
};


// CFPGADebugProgram

class ATL_NO_VTABLE CFPGADebugProgram :
	public CComObjectRootEx<CComSingleThreadModel>,
	public CComCoClass<CFPGADebugProgram, &CLSID_FPGADebugProgram>,
	public IFPGADebugProgram
{
    IFPGADebugProcess* m_pProcess = nullptr;
    std::wstring m_name;
    ATL::CComPtr<IFPGADebugThread2> m_pThread;
    GUID m_programGuid = { 0 };
public:
	CFPGADebugProgram()
	{
	}

DECLARE_REGISTRY_RESOURCEID(112)


BEGIN_COM_MAP(CFPGADebugProgram)
    COM_INTERFACE_ENTRY(IFPGADebugProgram)
    COM_INTERFACE_ENTRY(IDebugProgram2)
END_COM_MAP()

    STDMETHOD(Init)(IFPGADebugProcess* process, const WCHAR* filepath);
    STDMETHOD(GetThread)(IFPGADebugThread2** ppThread);

    //////////////////////////////////////////////////////////// 
    //IDebugProgram2
    STDMETHOD(EnumThreads)(IEnumDebugThreads2** ppEnum);
    STDMETHOD(GetName)(BSTR* pbstrName);
    STDMETHOD(GetProcess)(IDebugProcess2** ppProcess);
    STDMETHOD(Terminate)(void);
    STDMETHOD(Attach)(IDebugEventCallback2* pCallback);
    STDMETHOD(CanDetach)(void);
    STDMETHOD(Detach)(void);
    STDMETHOD(GetProgramId)(GUID* pguidProgramId);
    STDMETHOD(GetDebugProperty)(IDebugProperty2** ppProperty);
    STDMETHOD(Execute)(void);
    STDMETHOD(Continue)(IDebugThread2* pThread);
    STDMETHOD(Step)(IDebugThread2* pThread, STEPKIND sk, STEPUNIT step);
    STDMETHOD(CauseBreak)(void);
    STDMETHOD(GetEngineInfo)(BSTR* pbstrEngine, GUID* pguidEngine);
    STDMETHOD(EnumCodeContexts)(IDebugDocumentPosition2* pDocPos, IEnumDebugCodeContexts2** ppEnum);
    STDMETHOD(GetMemoryBytes)(IDebugMemoryBytes2** ppMemoryBytes);
    STDMETHOD(GetDisassemblyStream)(
        DISASSEMBLY_STREAM_SCOPE dwScope,
        IDebugCodeContext2* pCodeContext,
        IDebugDisassemblyStream2** ppDisassemblyStream);
    STDMETHOD(EnumModules)(IEnumDebugModules2** ppEnum);
    STDMETHOD(GetENCUpdate)(IDebugENCUpdate** ppUpdate);
    STDMETHOD(EnumCodePaths)(
        LPCOLESTR pszHint,
        IDebugCodeContext2* pStart,
        IDebugStackFrame2* pFrame,
        BOOL fSource,
        IEnumCodePaths2** ppEnum,
        IDebugCodeContext2** ppSafety);
    STDMETHOD(WriteDump)(DUMPTYPE DumpType, LPCOLESTR pszDumpUrl);


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

OBJECT_ENTRY_AUTO(__uuidof(FPGADebugProgram), CFPGADebugProgram)
