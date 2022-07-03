// ElfDebugDocumentContext.cpp : Implementation of CElfDebugDocumentContext

#include "pch.h"
#include "ElfDebugDocumentContext.h"
#include "Utils.h"

// CElfDebugDocumentContext

HRESULT CElfDebugDocumentContext::Init(Dwarf_Debug dbg, Dwarf_Line line, Dwarf_Unsigned lang)
{
	m_dbg = dbg;
	m_line = line;
	m_lang = lang;
	return S_OK;
}
HRESULT CElfDebugDocumentContext::GetDocument(
	/* [out] */ __RPC__deref_out_opt IDebugDocument2** ppDocument)
{
	*ppDocument = nullptr;
	return E_NOTIMPL;
}

HRESULT CElfDebugDocumentContext::GetName(
	/* [in] */ GETNAME_TYPE gnType,
	/* [out] */ __RPC__deref_out_opt BSTR* pbstrFileName)
{
	char* filename = nullptr;
	Dwarf_Error err = nullptr;
	int result = dwarf_linesrc(m_line, &filename, &err);
	SafeThrowOnError(m_dbg, err);
	CA2W filenameW(filename);
	*pbstrFileName = SysAllocString(filenameW);
	return S_OK;
}

HRESULT CElfDebugDocumentContext::EnumCodeContexts(
	/* [out] */ __RPC__deref_out_opt IEnumDebugCodeContexts2** ppEnumCodeCxts)
{
	return S_OK;
}

HRESULT CElfDebugDocumentContext::GetLanguageInfo(
	/* [full][out][in] */ __RPC__deref_opt_inout_opt BSTR* pbstrLanguage,
	/* [full][out][in] */ __RPC__inout_opt GUID* pguidLanguage)
{
	if (pbstrLanguage == nullptr || pguidLanguage == nullptr)
		return E_INVALIDARG;

	switch (m_lang)
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

HRESULT CElfDebugDocumentContext::GetStatementRange(
	/* [full][out][in] */ __RPC__inout_opt TEXT_POSITION* pBegPosition,
	/* [full][out][in] */ __RPC__inout_opt TEXT_POSITION* pEndPosition)
{
	Dwarf_Unsigned no = 0;
	Dwarf_Error err = nullptr;
	int result = dwarf_lineno(m_line, &no, &err);
	SafeThrowOnError(m_dbg, err);
	--no; // vs expect zero based I guess
	Dwarf_Unsigned col = 0;
	result = dwarf_lineoff_b(m_line, &col, &err);
	SafeThrowOnError(m_dbg, err);

	if (pBegPosition)
	{
		pBegPosition->dwLine = static_cast<DWORD>(no);
		pBegPosition->dwColumn = static_cast<DWORD>(col);
	}
	if (pEndPosition)
	{
		pEndPosition->dwLine = static_cast<DWORD>(no);
		pEndPosition->dwColumn = static_cast<DWORD>(col);
	}

	return S_OK;
}

HRESULT CElfDebugDocumentContext::GetSourceRange(
	/* [full][out][in] */ __RPC__inout_opt TEXT_POSITION* pBegPosition,
	/* [full][out][in] */ __RPC__inout_opt TEXT_POSITION* pEndPosition)
{
	return GetStatementRange(pBegPosition, pEndPosition);
}

HRESULT CElfDebugDocumentContext::Compare(
	/* [in] */ DOCCONTEXT_COMPARE compare,
	/* [length_is][size_is][in] */ __RPC__in_ecount_part(dwDocContextSetLen, dwDocContextSetLen) IDebugDocumentContext2** rgpDocContextSet,
	/* [in] */ DWORD dwDocContextSetLen,
	/* [out] */ __RPC__out DWORD* pdwDocContext)
{
	return S_OK;
}

HRESULT CElfDebugDocumentContext::Seek(
	/* [in] */ int nCount,
	/* [out] */ __RPC__deref_out_opt IDebugDocumentContext2** ppDocContext)
{
	return E_NOTIMPL;
}