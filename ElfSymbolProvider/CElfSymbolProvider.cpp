/*
 Copyright (C) 2022 jobmarley

 This file is part of VSMipsProjectExtension.

 This program is free software; you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation; either version 3 of the License, or (at your option) any later version.

 This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

 You should have received a copy of the GNU General Public License along with Foobar. If not, see <https://www.gnu.org/licenses/>.
 */

// CElfSymbolProvider.cpp : Implementation of CElfSymbolProvider

#include "pch.h"
#include "CElfSymbolProvider.h"
#include "ElfDebugDocumentContext.h"
#include "ElfDebugAddress.h"
#include "ElfDebugStackFrame.h"
#include "SimpleEnumerator.h"
#include "ElfDebugCodeContext.h"

// CElfSymbolProvider

HRESULT CElfSymbolProvider::GetModuleFromDebugAddress(
    IDebugAddress* pAddress,
    ElfModule** ppModule)
{
    *ppModule = nullptr;

    DEBUG_ADDRESS da = {};
    HRESULT hr = pAddress->GetAddress(&da);
    if (FAILED(hr))
        return E_FAIL;

    return GetModuleFromGUID(da.guidModule, ppModule);
}

HRESULT CElfSymbolProvider::GetModuleFromGUID(
    const GUID& guid,
    ElfModule** ppModule)
{
    *ppModule = nullptr;
    auto found = m_modulesByGuid.find(guid);
    if (found == m_modulesByGuid.end())
        return E_FAIL;

    *ppModule = found->second;
    return S_OK;
}

HRESULT CElfSymbolProvider::GetDEEventCallback(
    IMipsDEEventCallback** ppDEEventCallback)
{
    *ppDEEventCallback = m_pDEEventCallback;
    return S_OK;
}
HRESULT CElfSymbolProvider::GetModuleFromAddress(DWORD address, GUID* pGuid)
{
    if (!pGuid)
        return E_INVALIDARG;

    *pGuid = {};
    {
        std::lock_guard<std::mutex> guard(m_mutex);
        auto found = std::upper_bound(
            m_modulesByAddress.begin(),
            m_modulesByAddress.end(),
            address, [](DWORD v, const std::pair<DWORD, GUID>& p) { return v < p.first; });

        // The result is the first element bigger, so we want the one just before
        if (found != m_modulesByAddress.begin())
        {
            --found;
            auto pModule = m_modulesByGuid[found->second];
            // Check if the address is in the modules boundaries
            DWORD base = found->first;
            if (base <= address && address <= base + pModule->VirtualSize() - 4)
            {
                *pGuid = found->second;
                return S_OK;
            }
        }
    }

    return E_FAIL;
}
HRESULT CElfSymbolProvider::GetAddressFromMemory(
    DWORD memAddr,
    IDebugAddress** ppAddress)
{
    CComPtr<IElfDebugAddress> pAddress;
    HRESULT hr = CElfDebugAddress::CreateInstance(&pAddress);
    if (FAILED(hr))
        return hr;

    DEBUG_ADDRESS da = {};
    da.addr.dwKind = enum_ADDRESS_KIND::ADDRESS_KIND_NATIVE;
    da.addr.addr.addrNative.unknown = memAddr;

    // dont care about error, empty if not found
    hr = GetModuleFromAddress(memAddr, &da.guidModule);

    hr = pAddress->SetAddress(&da);
    if (FAILED(hr))
        return hr;

    return pAddress.QueryInterface(ppAddress);
}


HRESULT CElfSymbolProvider::LoadModule(
    IDebugModule2* pDebugModule,
    LPCOLESTR pszFilepath,
    DWORD address)
{
    try
    {
        CW2A szFilepathA(pszFilepath);
        if (m_modules.find(szFilepathA.m_psz) != m_modules.end())
            return E_FAIL;

        auto pModule = std::make_unique<ElfModule>(this);
        pModule->Load(pDebugModule, szFilepathA.m_psz);

        GUID guid = {};
        HRESULT hr = CoCreateGuid(&guid);
        if (FAILED(hr))
            return hr;

        {
            std::lock_guard<std::mutex> guard(m_mutex);

            // Check again in lock for thread safety
            if (m_modules.find(szFilepathA.m_psz) != m_modules.end())
                return E_FAIL;

            m_modulesByAddress[address] = guid;
            m_modulesByGuid[guid] = pModule.get();
            m_modules[szFilepathA.m_psz] = std::move(pModule);
        }
        return S_OK;
    }
    catch (std::exception)
    {
        return E_FAIL;
    }
}
HRESULT CElfSymbolProvider::GetStackFrame(
    IDebugAddress* pAddress,
    IDebugThread2* pThread,
    IMemoryOperation* pMemoryOp,
    IRegisterOperation* pRegisterOp,
    IDebugStackFrame2** ppStackFrame)
{
    if (!ppStackFrame)
        return E_INVALIDARG;

    DEBUG_ADDRESS ad = {};
    HRESULT hr = pAddress->GetAddress(&ad);
    if (FAILED(hr))
        return hr;

    auto found = m_modulesByGuid.find(ad.guidModule);
    if (found == m_modulesByGuid.end())
        return E_FAIL;

    ElfModule* pModule = found->second;
    return pModule->GetStackFrame(pAddress, pThread, pMemoryOp, pRegisterOp, ppStackFrame);
}

HRESULT CElfSymbolProvider::GetPreviousStackFrame(IDebugStackFrame2* pStackFrame, IDebugStackFrame2** ppStackFrame)
{
    CComPtr<IElfDebugStackFrame> pElfStackFrame;
    HRESULT hr = pStackFrame->QueryInterface(&pElfStackFrame);
    if (FAILED(hr))
        return hr;

    return pElfStackFrame->GetPreviousStackFrame(ppStackFrame);
}
HRESULT CElfSymbolProvider::GetCodeContextFromAddress(IDebugAddress* pAddress, IDebugCodeContext2** ppCodeContext)
{
    CComPtr<IElfDebugCodeContext> pContext;
    HRESULT hr = CElfDebugCodeContext::CreateInstance(&pContext);
    if (FAILED(hr))
        return hr;

    ElfModule* pModule = nullptr;
    GetModuleFromDebugAddress(pAddress, &pModule);
    pContext->Init(this, pModule, pAddress);
    return pContext.QueryInterface(ppCodeContext);
}
HRESULT CElfSymbolProvider::SetEventCallback(IMipsDEEventCallback* pDEEventCallback)
{
    m_pDEEventCallback = pDEEventCallback;
    return S_OK;
}
HRESULT CElfSymbolProvider::UnloadAll()
{
    // destroy everything, in case of circular references we would still close the elf files and such
    m_modulesByGuid.clear();
    m_modulesByAddress.clear();
    m_modules.clear();
    m_pDEEventCallback.Release();
    return S_OK;
}
HRESULT CElfSymbolProvider::Initialize(
    /* [in] */ __RPC__in_opt IDebugEngineSymbolProviderServices* pServices)
{
	return S_OK;
}

HRESULT CElfSymbolProvider::Uninitialize(void)
{
	return S_OK;
}

HRESULT CElfSymbolProvider::GetContainerField(
    /* [in] */ __RPC__in_opt IDebugAddress* pAddress,
    /* [out] */ __RPC__deref_out_opt IDebugContainerField** ppContainerField)
{
	return S_OK;
}

HRESULT CElfSymbolProvider::GetField(
    /* [in] */ __RPC__in_opt IDebugAddress* pAddress,
    /* [in] */ __RPC__in_opt IDebugAddress* pAddressCur,
    /* [out] */ __RPC__deref_out_opt IDebugField** ppField)
{
	return S_OK;
}

HRESULT CElfSymbolProvider::GetAddressesFromPosition(
    /* [in] */ __RPC__in_opt IDebugDocumentPosition2* pDocPos,
    /* [in] */ BOOL fStatmentOnly,
    /* [out] */ __RPC__deref_out_opt IEnumDebugAddresses** ppEnumBegAddresses,
    /* [out] */ __RPC__deref_out_opt IEnumDebugAddresses** ppEnumEndAddresses)
{
    CComBSTR filename;
    HRESULT hr = pDocPos->GetFileName(&filename);
    if (FAILED(hr))
        return E_FAIL;

    TEXT_POSITION beg = {};
    TEXT_POSITION end = {};
    hr = pDocPos->GetRange(&beg, &end);
    if (FAILED(hr))
        return E_FAIL;

    for (auto& it : m_modules)
    {
        std::vector<std::pair<DWORD, DWORD>> v;
        try
        {
            v = it.second->GetAddressesFromLine(CW2A(filename).m_psz, beg.dwLine + 1);
        }
        catch (...)
        {
            continue;
        }
        try
        {
            auto create_address = [this](DWORD d)
            {
                CComPtr<IDebugAddress> o;
                HRESULT hr = GetAddressFromMemory(d, &o);
                if (FAILED(hr))
                    throw std::exception();
                return o;
            };
            CComPtr<IEnumDebugAddresses> enumBegAddresses;
            CComPtr<IEnumDebugAddresses> enumEndAddresses;
            SimpleEnumerator<IEnumDebugAddresses>::Create(v | std::views::transform([&create_address](std::pair<DWORD, DWORD> p) { return create_address(p.first); }), &enumBegAddresses);
            SimpleEnumerator<IEnumDebugAddresses>::Create(v | std::views::transform([&create_address](std::pair<DWORD, DWORD> p) { return create_address(p.second); }), &enumEndAddresses);
            *ppEnumBegAddresses = enumBegAddresses.Detach();
            *ppEnumEndAddresses = enumEndAddresses.Detach();
            return S_OK;
        }
        catch (...)
        {
        }
    }
	return E_FAIL;
}

HRESULT CElfSymbolProvider::GetAddressesFromContext(
    /* [in] */ __RPC__in_opt IDebugDocumentContext2* pDocContext,
    /* [in] */ BOOL fStatmentOnly,
    /* [out] */ __RPC__deref_out_opt IEnumDebugAddresses** ppEnumBegAddresses,
    /* [out] */ __RPC__deref_out_opt IEnumDebugAddresses** ppEnumEndAddresses)
{
	return S_OK;
}

HRESULT CElfSymbolProvider::GetContextFromAddress(
    /* [in] */ __RPC__in_opt IDebugAddress* pAddress,
    /* [out] */ __RPC__deref_out_opt IDebugDocumentContext2** ppDocContext)
{
    // Identify module from address
    // Get line from libdwarf
    // create debug document
    DEBUG_ADDRESS da = {};
    HRESULT hr = pAddress->GetAddress(&da);
    if (FAILED(hr))
        return hr;

    {
        std::lock_guard<std::mutex> guard(m_mutex);
        auto found = m_modulesByGuid.find(da.guidModule);
        if (found == m_modulesByGuid.end())
            return E_FAIL;

        ElfModule* pModule = found->second;
        return pModule->GetContextFromAddress(pAddress, ppDocContext);
    }
}

HRESULT CElfSymbolProvider::GetLanguage(
    /* [in] */ __RPC__in_opt IDebugAddress* pAddress,
    /* [out] */ __RPC__out GUID* pguidLanguage,
    /* [out] */ __RPC__out GUID* pguidLanguageVendor)
{
	return S_OK;
}

HRESULT CElfSymbolProvider::GetGlobalContainer(
    /* [out] */ __RPC__deref_out_opt IDebugContainerField** pField)
{
	return S_OK;
}

HRESULT CElfSymbolProvider::GetMethodFieldsByName(
    /* [full][in] */ __RPC__in_opt LPCOLESTR pszFullName,
    /* [in] */ NAME_MATCH nameMatch,
    /* [out] */ __RPC__deref_out_opt IEnumDebugFields** ppEnum)
{
	return S_OK;
}

HRESULT CElfSymbolProvider::GetClassTypeByName(
    /* [full][in] */ __RPC__in_opt LPCOLESTR pszClassName,
    /* [in] */ NAME_MATCH nameMatch,
    /* [out] */ __RPC__deref_out_opt IDebugClassField** ppField)
{
	return S_OK;
}

HRESULT CElfSymbolProvider::GetNamespacesUsedAtAddress(
    /* [in] */ __RPC__in_opt IDebugAddress* pAddress,
    /* [out] */ __RPC__deref_out_opt IEnumDebugFields** ppEnum)
{
	return S_OK;
}

HRESULT CElfSymbolProvider::GetTypeByName(
    /* [full][in] */ __RPC__in_opt LPCOLESTR pszClassName,
    /* [in] */ NAME_MATCH nameMatch,
    /* [out] */ __RPC__deref_out_opt IDebugField** ppField)
{
	return S_OK;
}

HRESULT CElfSymbolProvider::GetNextAddress(
    /* [in] */ __RPC__in_opt IDebugAddress* pAddress,
    /* [in] */ BOOL fStatmentOnly,
    /* [out] */ __RPC__deref_out_opt IDebugAddress** ppAddress)
{
	return S_OK;
}