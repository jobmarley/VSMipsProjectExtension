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