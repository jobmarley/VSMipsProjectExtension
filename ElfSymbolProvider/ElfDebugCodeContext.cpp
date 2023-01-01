/*
 Copyright (C) 2022 jobmarley

 This file is part of VSMipsProjectExtension.

 This program is free software; you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation; either version 3 of the License, or (at your option) any later version.

 This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

 You should have received a copy of the GNU General Public License along with Foobar. If not, see <https://www.gnu.org/licenses/>.
 */

// ElfDebugCodeContext.cpp : Implementation of CElfDebugCodeContext

#include "pch.h"
#include "ElfDebugCodeContext.h"


// CElfDebugCodeContext


HRESULT CElfDebugCodeContext::Init(IElfSymbolProvider* pSymbolProvider, ElfModule* pModule, IDebugAddress* pAddress)
{
	m_pSymbolProvider = pSymbolProvider;
	m_pModule = pModule;
	m_pAddress = pAddress;
	m_addressValue = GetAddressValue(pAddress);

	try
	{
		m_function = pModule->GetFunction(pAddress);
	}
	catch (...)
	{

	}
	std::wstringstream ss;
	if (m_function)
		ss << m_function->GetName() << "!";
	ss << std::hex << std::setfill(L'0') << std::setw(8) << m_addressValue;
	m_name = ss.str();
	return S_OK;
}
HRESULT CElfDebugCodeContext::GetDocumentContext(
	/* [out] */ __RPC__deref_out_opt IDebugDocumentContext2** ppSrcCxt)
{
	return m_pSymbolProvider->GetContextFromAddress(m_pAddress, ppSrcCxt);
}

Dwarf_Unsigned GetLang(ElfModule* pModule, uint32_t address)
{
	if (pModule)
	{
		auto it = pModule->CUFromAddress(address);
		if (it)
			return it->die->GetLang();
	}
	return 0;
}
HRESULT CElfDebugCodeContext::GetLanguageInfo(
	/* [full][out][in] */ __RPC__deref_opt_inout_opt BSTR* pbstrLanguage,
	/* [full][out][in] */ __RPC__inout_opt GUID* pguidLanguage)
{
	if (pbstrLanguage == nullptr || pguidLanguage == nullptr)
		return E_INVALIDARG;

	Dwarf_Unsigned lang = GetLang(m_pModule, m_addressValue);

	switch (lang)
	{
	case DW_LANG_C89:
	case DW_LANG_C:
		*pbstrLanguage = SysAllocString(L"C");
		*pguidLanguage = guidCLang;
		break;
	case DW_LANG_C_plus_plus_14:
	case DW_LANG_C_plus_plus_11:
	case DW_LANG_C_plus_plus_03:
	case DW_LANG_C_plus_plus:
		*pbstrLanguage = SysAllocString(L"C++");
		*pguidLanguage = guidCPPLang;
		break;
	default:
		*pbstrLanguage = SysAllocString(L"Unknown");
		*pguidLanguage = {};
		return S_OK;
	}
	return S_OK;
}

HRESULT CElfDebugCodeContext::GetName(
	/* [out] */ __RPC__deref_out_opt BSTR* pbstrName)
{
	*pbstrName = SysAllocString(m_name.c_str());
	return S_OK;
}

HRESULT CElfDebugCodeContext::GetInfo(
	/* [in] */ CONTEXT_INFO_FIELDS dwFields,
	/* [out] */ __RPC__out CONTEXT_INFO* pInfo)
{
	if (!pInfo)
		return E_INVALIDARG;

	std::wstringstream ss;
	ss << std::hex << std::setfill(L'0') << std::setw(8) << m_addressValue;

	if (dwFields & enum_CONTEXT_INFO_FIELDS::CIF_ADDRESS)
	{
		// Dont know the difference with CIF_ADDRESSABSOLUTE. Virtual/non-virtual?
		pInfo->bstrAddress = SysAllocString(ss.str().c_str());
		pInfo->dwFields |= enum_CONTEXT_INFO_FIELDS::CIF_ADDRESS;
	}
	if (dwFields & enum_CONTEXT_INFO_FIELDS::CIF_ADDRESSABSOLUTE)
	{
		pInfo->bstrAddress = SysAllocString(ss.str().c_str());
		pInfo->dwFields |= enum_CONTEXT_INFO_FIELDS::CIF_ADDRESSABSOLUTE;
	}
	if (dwFields & enum_CONTEXT_INFO_FIELDS::CIF_ADDRESSOFFSET)
	{
		// Not sure what that is... the offset in the .text section?
		//pInfo->bstrAddress = SysAllocString(ss.str().c_str());
		//pInfo->dwFields |= enum_CONTEXT_INFO_FIELDS::CIF_ADDRESS;
	}
	if (dwFields & enum_CONTEXT_INFO_FIELDS::CIF_FUNCTION)
	{
		if (m_function)
			pInfo->bstrFunction = SysAllocString(CA2W(m_function->GetName()));
		else
			pInfo->bstrFunction = SysAllocString(L""); // not sure if I should return empty string here
		pInfo->dwFields |= enum_CONTEXT_INFO_FIELDS::CIF_FUNCTION;
	}
	if (dwFields & enum_CONTEXT_INFO_FIELDS::CIF_FUNCTIONOFFSET)
	{
		// Thats not good, we need to function span, not the line of code
		//m_pDocumentContext->GetSourceRange(&pInfo->posFunctionOffset, nullptr);
		//pInfo->dwFields |= enum_CONTEXT_INFO_FIELDS::CIF_FUNCTIONOFFSET;
	}
	if (dwFields & enum_CONTEXT_INFO_FIELDS::CIF_MODULEURL)
	{
		if (m_pModule)
			pInfo->bstrModuleUrl = SysAllocString(CA2W(m_pModule->GetFilepath()));
		else
			pInfo->bstrModuleUrl = SysAllocString(L"");
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
