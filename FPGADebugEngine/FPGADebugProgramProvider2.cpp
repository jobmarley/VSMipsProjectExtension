// FPGADebugProgramProvider2.cpp : Implementation of CFPGADebugProgramProvider2

#include "pch.h"
#include "FPGADebugProgramProvider2.h"


// CFPGADebugProgramProvider2

HRESULT CFPGADebugProgramProvider2::GetProviderProcessData(
    PROVIDER_FLAGS Flags,
    IDebugDefaultPort2* pPort,
    AD_PROCESS_ID processId,
    CONST_GUID_ARRAY EngineFilter,
    PROVIDER_PROCESS_DATA* pProcess)
{
    return E_NOTIMPL;
}
HRESULT CFPGADebugProgramProvider2::GetProviderProgramNode(
    PROVIDER_FLAGS Flags,
    IDebugDefaultPort2* pPort,
    AD_PROCESS_ID processId,
    REFGUID guidEngine,
    UINT64 programId,
    IDebugProgramNode2** ppProgramNode)
{
    return E_NOTIMPL;
}
HRESULT CFPGADebugProgramProvider2::WatchForProviderEvents(
    PROVIDER_FLAGS Flags,
    IDebugDefaultPort2* pPort,
    AD_PROCESS_ID processId,
    CONST_GUID_ARRAY EngineFilter,
    REFGUID guidLaunchingEngine,
    IDebugPortNotify2* pEventCallback)
{
    return S_OK;
}
HRESULT CFPGADebugProgramProvider2::SetLocale(WORD wLangID)
{
	return S_OK;
}