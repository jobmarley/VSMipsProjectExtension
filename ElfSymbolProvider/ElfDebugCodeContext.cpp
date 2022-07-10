// ElfDebugCodeContext.cpp : Implementation of CElfDebugCodeContext

#include "pch.h"
#include "ElfDebugCodeContext.h"


// CElfDebugCodeContext


HRESULT CElfDebugCodeContext::Init(ElfModule* pModule, IDebugAddress* pAddress, IDebugDocumentContext2* pDocumentContext)
{
	m_pModule = pModule;
	m_pAddress = pAddress;
	m_pDocumentContext = pDocumentContext;

	m_die = pModule->GetFunction(pAddress);
	return S_OK;
}
HRESULT CElfDebugCodeContext::GetDocumentContext(
	/* [out] */ __RPC__deref_out_opt IDebugDocumentContext2** ppSrcCxt)
{
	return m_pDocumentContext.QueryInterface(ppSrcCxt);
}

HRESULT CElfDebugCodeContext::GetLanguageInfo(
	/* [full][out][in] */ __RPC__deref_opt_inout_opt BSTR* pbstrLanguage,
	/* [full][out][in] */ __RPC__inout_opt GUID* pguidLanguage)
{
	return m_pDocumentContext->GetLanguageInfo(pbstrLanguage, pguidLanguage);
}

HRESULT CElfDebugCodeContext::GetName(
	/* [out] */ __RPC__deref_out_opt BSTR* pbstrName)
{
	*pbstrName = SysAllocString(CA2W(m_die->GetName()));
	return S_OK;
}

HRESULT CElfDebugCodeContext::GetInfo(
	/* [in] */ CONTEXT_INFO_FIELDS dwFields,
	/* [out] */ __RPC__out CONTEXT_INFO* pInfo)
{
	if (!pInfo)
		return E_INVALIDARG;

	DEBUG_ADDRESS da = {};
	m_pAddress->GetAddress(&da);

	std::wstringstream ss;
	ss << std::hex << std::setfill(L'0') << std::setw(8) << da.addr.addr.addrNative.unknown;

	if (dwFields & enum_CONTEXT_INFO_FIELDS::CIF_ADDRESS)
	{
		pInfo->bstrAddress = SysAllocString(ss.str().c_str());
		pInfo->dwFields |= enum_CONTEXT_INFO_FIELDS::CIF_ADDRESS;
	}
	if (dwFields & enum_CONTEXT_INFO_FIELDS::CIF_ADDRESSABSOLUTE)
	{
		pInfo->bstrAddress = SysAllocString(ss.str().c_str());
		pInfo->dwFields |= enum_CONTEXT_INFO_FIELDS::CIF_ADDRESS;
	}
	if (dwFields & enum_CONTEXT_INFO_FIELDS::CIF_ADDRESSOFFSET)
	{
		pInfo->bstrAddress = SysAllocString(ss.str().c_str());
		pInfo->dwFields |= enum_CONTEXT_INFO_FIELDS::CIF_ADDRESS;
	}
	if (dwFields & enum_CONTEXT_INFO_FIELDS::CIF_FUNCTION)
	{
		 pInfo->bstrFunction = SysAllocString(CA2W(m_die->GetName()));
		 pInfo->dwFields |= enum_CONTEXT_INFO_FIELDS::CIF_FUNCTION;
	}
	if (dwFields & enum_CONTEXT_INFO_FIELDS::CIF_FUNCTIONOFFSET)
	{
		m_pDocumentContext->GetSourceRange(&pInfo->posFunctionOffset, nullptr);
		pInfo->dwFields |= enum_CONTEXT_INFO_FIELDS::CIF_FUNCTIONOFFSET;
	}
	if (dwFields & enum_CONTEXT_INFO_FIELDS::CIF_MODULEURL)
	{
		pInfo->bstrModuleUrl = SysAllocString(CA2W(m_pModule->GetFilepath()));
		pInfo->dwFields |= enum_CONTEXT_INFO_FIELDS::CIF_MODULEURL;
	}

	return S_OK;
}

HRESULT CElfDebugCodeContext::Add(
	/* [in] */ UINT64 dwCount,
	/* [out] */ __RPC__deref_out_opt IDebugMemoryContext2** ppMemCxt)
{
	return E_NOTIMPL;
}

HRESULT CElfDebugCodeContext::Subtract(
	/* [in] */ UINT64 dwCount,
	/* [out] */ __RPC__deref_out_opt IDebugMemoryContext2** ppMemCxt)
{
	return E_NOTIMPL;
}

HRESULT CElfDebugCodeContext::Compare(
	/* [in] */ CONTEXT_COMPARE compare,
	/* [length_is][size_is][in] */ __RPC__in_ecount_part(dwMemoryContextSetLen, dwMemoryContextSetLen) IDebugMemoryContext2** rgpMemoryContextSet,
	/* [in] */ DWORD dwMemoryContextSetLen,
	/* [out] */ __RPC__out DWORD* pdwMemoryContext)
{
	return E_NOTIMPL;
}
