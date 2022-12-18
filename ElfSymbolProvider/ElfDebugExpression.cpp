/*
 Copyright (C) 2022 jobmarley

 This file is part of VSMipsProjectExtension.

 This program is free software; you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation; either version 3 of the License, or (at your option) any later version.

 This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

 You should have received a copy of the GNU General Public License along with Foobar. If not, see <https://www.gnu.org/licenses/>.
 */

// ElfDebugExpression.cpp : Implementation of CElfDebugExpression

#include "pch.h"
#include "ElfDebugExpression.h"
#include "CElfSymbolProvider.h"
#include "ElfDebugProperty.h"

// CElfDebugExpression

HRESULT CElfDebugExpression::Init(IElfSymbolProvider* pSymbolProvider, IElfDebugStackFrame* pStackFrame, LPCOLESTR code, ElfDie* pDie, IDebugDocumentContext2* pDocumentContext)
{
	CComPtr<ISymbolProviderInternal> pSPI;
	HRESULT hr = pSymbolProvider->QueryInterface(&pSPI);
	if (FAILED(hr))
		return hr;
	hr = pSPI->GetDEEventCallback(&m_pDEEventCallback);
	if (FAILED(hr))
		return hr;

	m_code = code;
	m_die = pDie;
	m_pDocumentContext = pDocumentContext;
	m_pStackFrame = pStackFrame;
	return S_OK;
}

HRESULT CElfDebugExpression::EvaluateAsync(
	/* [in] */ EVALFLAGS dwFlags,
	/* [in] */ __RPC__in_opt IDebugEventCallback2* pExprCallback)
{
	m_future = std::async([&, this]()
		{
			EvaluateSync(dwFlags, INFINITE, nullptr, &m_pResult);
		});
	return S_OK;
}

HRESULT CElfDebugExpression::Abort(void)
{
	return S_OK;
}

struct zobob
{
	CComPtr<IDebugThread2> pThread;
	CComPtr<IDebugProperty2> pDebugProperty;
	CComPtr<IDebugExpression2> pDebugExpression;
	CComPtr<IMipsDEEventCallback> pCallback;
};
HRESULT CElfDebugExpression::EvaluateSync(
	/* [in] */ EVALFLAGS dwFlags,
	/* [in] */ DWORD dwTimeout,
	/* [in] */ __RPC__in_opt IDebugEventCallback2* pExprCallback,
	/* [out] */ __RPC__deref_out_opt IDebugProperty2** ppResult)
{
	*ppResult = nullptr;

	ElfDie* die = nullptr;
	std::string codeA = CW2A(m_code).m_psz;

	if (m_die->GetName() == codeA)
		die = m_die;
	else
	{
		for (auto& it : m_die->GetChildrens() | std::views::filter([](auto& x) { return x->GetTag() == DW_TAG_variable; })
			| std::views::filter([&](auto& x) { return x->GetName() == codeA; }))
		{
			die = it.get();
			break;
		}
	}
	return E_FAIL;
	//if (die == nullptr)
	//	return E_FAIL;


	/*CComPtr<IElfDebugProperty> pDebugProperty;
	HRESULT hr = CElfDebugProperty::CreateInstance(&pDebugProperty);
	if (FAILED(hr))
		return hr;

	hr = pDebugProperty->Init(CA2W(die->GetName()), die->GetType(), m_pDocumentContext, m_pStackFrame);
	if (FAILED(hr))
		return hr;*/

	CComPtr<IComThreadingInfo> pThreadingInfo;
	HRESULT hr = CoGetObjectContext(IID_IComThreadingInfo, (LPVOID*)&pThreadingInfo);
	APTTYPE apttype;
	hr = pThreadingInfo->GetCurrentApartmentType(&apttype);
	/*CComPtr<IDebugExpression2> gogo;
	((IUnknown*)this)->QueryInterface(&gogo);
	std::async([t = m_pThread, self = gogo, prop = pDebugProperty, callback = m_pDEEventCallback](){
		CoInitializeEx(NULL, COINIT_MULTITHREADED);
		try
		{
			callback->SendEECompleteEvent(t, self, prop);
		}
		catch (...)
		{

		}
		callback->SendEECompleteEvent(t, self, prop);
		int qzqd = 0;
		});*/

	//m_pDEEventCallback->SendEECompleteEvent(m_pThread, this, pDebugProperty);
	// 
	//zobob* pzobob = new zobob;
	//pzobob->pThread = m_pThread;
	//pzobob->pDebugExpression = this;
	//pzobob->pDebugProperty = pDebugProperty;
	//pzobob->pCallback = m_pDEEventCallback;
	//
	//std::async(std::launch::async, [=]()
	//	{
	//		try
	//		{
	//			////HRESULT hr = CoInitializeEx(NULL, COINIT_APARTMENTTHREADED);
	//			CComPtr<IComThreadingInfo> pThreadingInfo;
	//			HRESULT hr = CoGetObjectContext(IID_IComThreadingInfo, (LPVOID*)&pThreadingInfo);
	//			APTTYPE apttype;
	//			hr = pThreadingInfo->GetCurrentApartmentType(&apttype);

	//			pzobob->pCallback->SendEECompleteEvent(pzobob->pThread, pzobob->pDebugExpression, pzobob->pDebugProperty);
	//			delete pzobob;
	//			return;
	//		}
	//		catch (...)
	//		{

	//		}
	//		CoInitialize(NULL);
	//		pzobob->pCallback->SendEECompleteEvent(pzobob->pThread, pzobob->pDebugExpression, pzobob->pDebugProperty);
	//		int zqdq = 0;
	//		delete pzobob;
	//	});
	//m_pResult = pDebugProperty;
	//return pDebugProperty.QueryInterface(&ppResult);
}