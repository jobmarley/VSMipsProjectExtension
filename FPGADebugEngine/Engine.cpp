// Engine.cpp : Implementation of CEngine

#include "pch.h"
#include "Engine.h"
#include "FPGADebugProcess.h"
#include "FPGADebugThread2.h"
#include "FPGADebugProcessCreateEvent2.h"

// CEngine
// 
////////////////////////////////////////////////////////////////////////////// 
//IDebugEngine2 methods 
HRESULT CEngine::EnumPrograms(IEnumDebugPrograms2** ppEnum)
{
    return E_NOTIMPL;
}
HRESULT CEngine::SetException(EXCEPTION_INFO* pException)
{
    return E_NOTIMPL;
}
HRESULT CEngine::RemoveSetException(EXCEPTION_INFO* pException)
{
    return E_NOTIMPL;
}
HRESULT CEngine::RemoveAllSetExceptions(REFGUID guidType)
{
    return E_NOTIMPL;
}
HRESULT CEngine::GetEngineId(GUID* pguidEngine)
{
    *pguidEngine = GUID_MyDebugger_Engine;
    return S_OK;
}
HRESULT CEngine::DestroyProgram(IDebugProgram2* pProgram)
{
    return E_NOTIMPL;
}
HRESULT CEngine::SetLocale(WORD wLangID)
{
    return S_OK;
}
HRESULT CEngine::SetRegistryRoot(LPCOLESTR pszRegistryRoot)
{
    m_registryRoot = pszRegistryRoot;
    return S_OK;
}
HRESULT CEngine::SetMetric(LPCOLESTR pszMetric, VARIANT varValue)
{
    return E_NOTIMPL;
}
HRESULT CEngine::CauseBreak(void)
{
    return E_NOTIMPL;
}

HRESULT CEngine::Attach(
    IDebugProgram2** rgpPrograms,
    IDebugProgramNode2** rgpProgramNodes,
    DWORD celtPrograms,
    IDebugEventCallback2* pCallback,
    ATTACH_REASON dwReason)
{
    // TODO: IMPLEMENT ATTACH 
    return E_NOTIMPL;
}

HRESULT CEngine::ContinueFromSynchronousEvent(IDebugEvent2* pEvent)
{
    //TODO: IMPLEMENT CONTINUE 
    return E_NOTIMPL;
}

HRESULT CEngine::CreatePendingBreakpoint(
    IDebugBreakpointRequest2* pBPRequest,
    IDebugPendingBreakpoint2** ppPendingBP)
{
    //TODO: IMPLEMENT BREAKPOINT CREATION 
    return E_NOTIMPL;
}

////////////////////////////////////////////////////////////////////////////// 
//IDebugEngineLaunch2 methods 
HRESULT CEngine::LaunchSuspended(
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
    IDebugProcess2** ppProcess)
{
    m_pCallback = pCallback;

    CComPtr<IFPGADebugProcess> pProcess;
    CFPGADebugProcess::CreateInstance(&pProcess);
    pProcess->Init(pszExe);
    pProcess->QueryInterface(ppProcess);
    m_pProcess = pProcess;


    CComPtr<IFPGADebugProgram> pProgram;
    m_pProcess->GetProgram(&pProgram);

    CComPtr<IFPGADebugThread2> pThread;
    pProgram->GetThread(&pThread);


    CComPtr<IFPGADebugEvent2> pProcessCreateEvent;
    CFPGADebugProcessCreateEvent2::CreateInstance(&pProcessCreateEvent);
    pProcessCreateEvent->SetAttributes(EVENT_IMMEDIATE | EVENT_SYNCHRONOUS);
    m_pCallback->Event(this, m_pProcess, pProgram, nullptr/*pThread*/, pProcessCreateEvent, IID_IDebugProcessCreateEvent2, EVENT_IMMEDIATE | EVENT_SYNCHRONOUS);

    CComPtr<IFPGADebugEvent2> pProgramCreateEvent;
    CFPGADebugProcessCreateEvent2::CreateInstance(&pProgramCreateEvent);
    pProgramCreateEvent->SetAttributes(EVENT_IMMEDIATE | EVENT_SYNCHRONOUS);
    //m_pCallback->Event(this, m_pProcess, pProgram, pThread, pProgramCreateEvent, IID_IDebugProgramCreateEvent2, EVENT_IMMEDIATE | EVENT_SYNCHRONOUS);


    return S_OK;
    /*HANDLE hFile = NULL;
    try
    {
        hFile = CreateFileW(pszExe, GENERIC_READ, FILE_SHARE_READ, NULL, OPEN_EXISTING, 0, NULL);
        if (hFile == NULL || hFile == INVALID_HANDLE_VALUE)
            return E_INVALIDARG;

        return S_OK;
    }
    catch (...)
    {

    }
    return E_FAIL;*/
}
HRESULT CEngine::ResumeProcess(IDebugProcess2* pProcess)
{
    return S_OK;
}
HRESULT CEngine::CanTerminateProcess(IDebugProcess2* pProcess)
{
    if (pProcess == m_pProcess)
        return S_OK;
    return S_FALSE;
}
HRESULT CEngine::TerminateProcess(IDebugProcess2* pProcess)
{
    return E_NOTIMPL;
}