// ElfDebugCodeContext.cpp : Implementation of CElfDebugCodeContext

#include "pch.h"
#include "ElfDebugCodeContext.h"


// CElfDebugCodeContext


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
	return S_OK;
}

HRESULT CElfDebugCodeContext::GetInfo(
	/* [in] */ CONTEXT_INFO_FIELDS dwFields,
	/* [out] */ __RPC__out CONTEXT_INFO* pInfo)
{
	if (!pInfo)
		return E_INVALIDARG;

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
