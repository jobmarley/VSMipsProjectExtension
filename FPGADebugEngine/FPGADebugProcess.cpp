// FPGADebugProcess.cpp : Implementation of CFPGADebugProcess

#include "pch.h"
#include "FPGADebugProcess.h"
#include "FPGAEnumerator.h"

// CFPGADebugProcess

HRESULT CFPGADebugProcess::Init(const WCHAR* filepath)
{
    m_filepath = filepath;
    GetSystemTimeAsFileTime(&m_creationTime);

    ATL::CComPtr<IFPGADebugProgram> pProgram;
    HRESULT hr = CFPGADebugProgram::CreateInstance(&pProgram);

    pProgram->Init(this, m_filepath.stem().wstring().c_str());
    m_programs.push_back(pProgram);

    CoCreateGuid(&m_physicalProcessGuid);
    CoCreateGuid(&m_processGuid);
    return S_OK;
}
HRESULT CFPGADebugProcess::GetProgram(IFPGADebugProgram** ppProgram)
{
    m_programs[0]->QueryInterface(ppProgram);
    return S_OK;
}


//////////////////////////////////////////////////////////// 
//IDebugProcess2 
HRESULT CFPGADebugProcess::GetInfo(
    PROCESS_INFO_FIELDS Fields,
    PROCESS_INFO* pProcessInfo)
{
    pProcessInfo->Fields = 0;
    if (Fields & PIF_FILE_NAME)
    {
        GetName(GN_FILENAME, &pProcessInfo->bstrFileName);
        pProcessInfo->Fields |= PIF_FILE_NAME;
    }
    if (Fields & PIF_BASE_NAME)
    {
        GetName(GN_BASENAME, &pProcessInfo->bstrBaseName);
        pProcessInfo->Fields |= PIF_BASE_NAME;
    }
    if (Fields & PIF_TITLE)
    {
        GetName(GN_TITLE, &pProcessInfo->bstrTitle);
        pProcessInfo->Fields |= PIF_TITLE;
    }
    if (Fields & PIF_PROCESS_ID)
    {
        GetPhysicalProcessId(&pProcessInfo->ProcessId);
        pProcessInfo->Fields |= PIF_PROCESS_ID;
    }
    //if (Fields & PIF_SESSION_ID)
    if (Fields & PIF_ATTACHED_SESSION_NAME)
    {
        GetAttachedSessionName(&pProcessInfo->bstrAttachedSessionName);
        pProcessInfo->Fields |= PIF_ATTACHED_SESSION_NAME;
    }
    if (Fields & PIF_CREATION_TIME)
    {
        pProcessInfo->CreationTime = m_creationTime;
        pProcessInfo->Fields |= PIF_CREATION_TIME;
    }
    if (Fields & PIF_FLAGS)
    {
        pProcessInfo->Flags = 0;
        if (m_pEventCallback)
            pProcessInfo->Flags |= PIFLAG_DEBUGGER_ATTACHED;

        if (m_running)
            pProcessInfo->Flags |= PIFLAG_PROCESS_RUNNING;
        else
            pProcessInfo->Flags |= PIFLAG_PROCESS_STOPPED;


        pProcessInfo->Fields |= PIF_FLAGS;
    }
    return E_NOTIMPL;
}

HRESULT CFPGADebugProcess::EnumPrograms(
    IEnumDebugPrograms2** ppEnum)
{
    CComPtr<CFPGAEnumerator<IEnumDebugPrograms2>> pEnumerator;
    HRESULT hr = CFPGAEnumerator<IEnumDebugPrograms2>::CreateInstance(&pEnumerator);
    if (FAILED(hr))
        return hr;

    pEnumerator->Add(m_programs);

    return pEnumerator.QueryInterface(ppEnum);
}

HRESULT CFPGADebugProcess::GetName(
    GETNAME_TYPE gnType,
    BSTR* pbstrName)
{
    switch (gnType)
    {
    case GN_NAME:
        *pbstrName = SysAllocString(m_filepath.stem().wstring().c_str());
        return S_OK;
    case GN_FILENAME:
        *pbstrName = SysAllocString(m_filepath.wstring().c_str());
        return S_OK;
    case GN_BASENAME:
        *pbstrName = SysAllocString(m_filepath.filename().wstring().c_str());
        return S_OK;
    case GN_MONIKERNAME:
        break;
    case GN_URL:
        break;
    case GN_TITLE:
        break;
    case GN_STARTPAGEURL:
        break;
    default:
        break;
    }
    return E_FAIL;
}

HRESULT CFPGADebugProcess::GetServer(
    IDebugCoreServer2** ppServer)
{
    return E_NOTIMPL;
}

HRESULT CFPGADebugProcess::Terminate(void)
{
    return S_OK;
}

HRESULT CFPGADebugProcess::Attach(
    IDebugEventCallback2* pCallback,
    GUID* rgguidSpecificEngines,
    DWORD celtSpecificEngines,
    HRESULT* rghrEngineAttach)
{
    return E_NOTIMPL;
}

HRESULT CFPGADebugProcess::CanDetach(void)
{
    return E_NOTIMPL;
}

HRESULT CFPGADebugProcess::Detach(void)
{
    return E_NOTIMPL;
}

HRESULT CFPGADebugProcess::GetPhysicalProcessId(AD_PROCESS_ID* pProcessId)
{
    pProcessId->ProcessIdType = AD_PROCESS_ID_GUID;
    pProcessId->ProcessId.guidProcessId = m_physicalProcessGuid;
    return S_OK;
}

HRESULT CFPGADebugProcess::GetProcessId(GUID* pguidProcessId)
{
    *pguidProcessId = m_processGuid;
    return S_OK;
}

HRESULT CFPGADebugProcess::GetAttachedSessionName(BSTR* pbstrSessionName)
{
    // deprecated
    return E_NOTIMPL;
}

HRESULT CFPGADebugProcess::EnumThreads(IEnumDebugThreads2** ppEnum)
{
    return E_NOTIMPL;
}

HRESULT CFPGADebugProcess::CauseBreak(void)
{
    return E_NOTIMPL;
}

HRESULT CFPGADebugProcess::GetPort(IDebugPort2** ppPort)
{
    return E_NOTIMPL;
}