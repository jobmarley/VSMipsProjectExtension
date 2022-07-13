/*
 Copyright (C) 2022 jobmarley

 This file is part of VSMipsProjectExtension.

 This program is free software; you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation; either version 3 of the License, or (at your option) any later version.

 This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

 You should have received a copy of the GNU General Public License along with Foobar. If not, see <https://www.gnu.org/licenses/>.
 */

// ElfDebugExpressionContext.cpp : Implementation of CElfDebugExpressionContext

#include "pch.h"
#include "ElfDebugExpressionContext.h"
#include "ElfDebugExpression.h"
#include "ElfDebugStackFrame.h"

// CElfDebugExpressionContext

HRESULT CElfDebugExpressionContext::Init(IElfSymbolProvider* pSymbolProvider, IElfDebugStackFrame* pStackFrame, ElfDie* pDie, IDebugDocumentContext2* pDocumentContext)
{
	m_pSymbolProvider = pSymbolProvider;
	m_pStackFrame = pStackFrame;
	m_pDie = pDie;
	m_pDocumentContext = pDocumentContext;
	return S_OK;
}
HRESULT CElfDebugExpressionContext::GetName(
	/* [out] */ __RPC__deref_out_opt BSTR* pbstrName)
{
	if (!pbstrName)
		return E_INVALIDARG;

	*pbstrName = SysAllocString(L"");
	return S_OK;
}

HRESULT CElfDebugExpressionContext::ParseText(
	/* [in] */ __RPC__in LPCOLESTR pszCode,
	/* [in] */ PARSEFLAGS dwFlags,
	/* [in] */ UINT nRadix,
	/* [out] */ __RPC__deref_out_opt IDebugExpression2** ppExpr,
	/* [out] */ __RPC__deref_out_opt BSTR* pbstrError,
	/* [out] */ __RPC__out UINT* pichError)
{
	CComPtr<IElfDebugExpression> pDebugExpression;
	HRESULT hr = CElfDebugExpression::CreateInstance(&pDebugExpression);
	if (FAILED(hr))
		return hr;

	pDebugExpression->Init(m_pSymbolProvider, m_pStackFrame, pszCode, m_pDie, m_pDocumentContext);

	*pbstrError = nullptr;
	*pichError = -1;
	return pDebugExpression.QueryInterface(ppExpr);
}