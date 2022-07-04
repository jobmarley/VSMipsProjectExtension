// ElfDebugStackFrame.cpp : Implementation of CElfDebugStackFrame

#include "pch.h"
#include "ElfDebugStackFrame.h"


// CElfDebugStackFrame


HRESULT CElfDebugStackFrame::Init(IDebugAddress* pAddress, IDebugThread2* pThread, ElfModule* pModule)
{
	m_pAddress = pAddress;
	m_pThread = pThread;
	m_pModule = pModule;

	HRESULT hr = pModule->GetContextFromAddress(m_pAddress, &m_pDocumentContext);
	return S_OK;
}
HRESULT CElfDebugStackFrame::GetCodeContext(
    /* [out] */ __RPC__deref_out_opt IDebugCodeContext2** ppCodeCxt)
{
    return E_NOTIMPL;
}

HRESULT CElfDebugStackFrame::GetDocumentContext(
    /* [out] */ __RPC__deref_out_opt IDebugDocumentContext2** ppCxt)
{
    return m_pDocumentContext.QueryInterface(ppCxt);
}

HRESULT CElfDebugStackFrame::GetName(
    /* [out] */ __RPC__deref_out_opt BSTR* pbstrName)
{
	DEBUG_ADDRESS ad = {};
	HRESULT hr = m_pAddress->GetAddress(&ad);
	if (FAILED(hr))
		return hr;
	DWORD address = ad.addr.addr.addrNative.unknown;

	std::wstringstream ss;
	ss << std::hex << std::setfill(L'0') << std::setw(8) << address;

	*pbstrName = SysAllocString(ss.str().c_str());
    return S_OK;
}

HRESULT CElfDebugStackFrame::GetInfo(
    /* [in] */ FRAMEINFO_FLAGS dwFieldSpec,
    /* [in] */ UINT nRadix,
    /* [out] */ __RPC__out FRAMEINFO* pFrameInfo)
{
	if (pFrameInfo == nullptr)
		return E_INVALIDARG;

	*pFrameInfo = {};
	if (dwFieldSpec & enum_FRAMEINFO_FLAGS::FIF_FUNCNAME)
	{
		DEBUG_ADDRESS ad = {};
		m_pAddress->GetAddress(&ad);
		DWORD address = ad.addr.addr.addrNative.unknown;

		if (m_pModule && (dwFieldSpec & enum_FRAMEINFO_FLAGS::FIF_FUNCNAME_MODULE))
		{
			std::wstringstream ss;
			std::filesystem::path p = m_pModule->GetFilepath();
			ss << p.filename().wstring() << L"!" << std::hex << std::setfill(L'0') << std::setw(8) << address;
			pFrameInfo->m_bstrFuncName = SysAllocString(ss.str().c_str());
			pFrameInfo->m_dwValidFields |= enum_FRAMEINFO_FLAGS::FIF_FUNCNAME;
		}
		else
		{
			std::wstringstream ss;
			ss << std::hex << std::setfill(L'0') << std::setw(8) << address;
			pFrameInfo->m_bstrFuncName = SysAllocString(ss.str().c_str());
			pFrameInfo->m_dwValidFields |= enum_FRAMEINFO_FLAGS::FIF_FUNCNAME;
		}
	}
	if (dwFieldSpec & enum_FRAMEINFO_FLAGS::FIF_RETURNTYPE)
	{
		pFrameInfo->m_bstrReturnType = SysAllocString(L"<return type>");
		pFrameInfo->m_dwValidFields |= enum_FRAMEINFO_FLAGS::FIF_RETURNTYPE;
	}
	if (dwFieldSpec & enum_FRAMEINFO_FLAGS::FIF_ARGS)
	{
		pFrameInfo->m_bstrArgs = SysAllocString(L"");
		pFrameInfo->m_dwValidFields |= enum_FRAMEINFO_FLAGS::FIF_ARGS;
	}
	if (dwFieldSpec & enum_FRAMEINFO_FLAGS::FIF_LANGUAGE)
	{
		GUID langGuid = {};
		GetLanguageInfo(&pFrameInfo->m_bstrLanguage, &langGuid);
		pFrameInfo->m_dwValidFields |= enum_FRAMEINFO_FLAGS::FIF_LANGUAGE;
	}
	if (dwFieldSpec & enum_FRAMEINFO_FLAGS::FIF_MODULE)
	{
		pFrameInfo->m_bstrModule = SysAllocString(CA2W(m_pModule->GetFilepath()));
		pFrameInfo->m_dwValidFields |= enum_FRAMEINFO_FLAGS::FIF_MODULE;
	}
	if (dwFieldSpec & enum_FRAMEINFO_FLAGS::FIF_DEBUG_MODULEP)
	{
		m_pModule->GetDebugModule()->QueryInterface(&pFrameInfo->m_pModule);
		pFrameInfo->m_dwValidFields |= enum_FRAMEINFO_FLAGS::FIF_DEBUG_MODULEP;
	}
	if (dwFieldSpec & enum_FRAMEINFO_FLAGS::FIF_STACKRANGE)
	{
		pFrameInfo->m_addrMin = 0;
		pFrameInfo->m_addrMax = 0x100;
		pFrameInfo->m_dwValidFields |= enum_FRAMEINFO_FLAGS::FIF_STACKRANGE;
	}
	if (dwFieldSpec & enum_FRAMEINFO_FLAGS::FIF_FRAME)
	{
		((IUnknown*)this)->QueryInterface(&pFrameInfo->m_pFrame);
		pFrameInfo->m_dwValidFields |= enum_FRAMEINFO_FLAGS::FIF_FRAME;
	}
	if (dwFieldSpec & enum_FRAMEINFO_FLAGS::FIF_DEBUGINFO)
	{
		pFrameInfo->m_fHasDebugInfo = TRUE;
		pFrameInfo->m_dwValidFields |= enum_FRAMEINFO_FLAGS::FIF_DEBUGINFO;
	}
	if (dwFieldSpec & enum_FRAMEINFO_FLAGS::FIF_STALECODE)
	{
		pFrameInfo->m_fStaleCode = 0;
		pFrameInfo->m_dwValidFields |= enum_FRAMEINFO_FLAGS::FIF_STALECODE;
	}
	return S_OK;
}

HRESULT CElfDebugStackFrame::GetPhysicalStackRange(
    /* [out] */ __RPC__out UINT64* paddrMin,
    /* [out] */ __RPC__out UINT64* paddrMax)
{
    return E_NOTIMPL;
}

HRESULT CElfDebugStackFrame::GetExpressionContext(
    /* [out] */ __RPC__deref_out_opt IDebugExpressionContext2** ppExprCxt)
{
    return E_NOTIMPL;
}

HRESULT CElfDebugStackFrame::GetLanguageInfo(
    /* [full][out][in] */ __RPC__deref_opt_inout_opt BSTR* pbstrLanguage,
    /* [full][out][in] */ __RPC__inout_opt GUID* pguidLanguage)
{
	if (!pbstrLanguage || !pguidLanguage)
		return E_INVALIDARG;

	if (m_pDocumentContext)
		return m_pDocumentContext->GetLanguageInfo(pbstrLanguage, pguidLanguage);

	*pguidLanguage = {};
	*pbstrLanguage = SysAllocString(L"Unknown");
	return S_OK;
}

HRESULT CElfDebugStackFrame::GetDebugProperty(
	/* [out] */ __RPC__deref_out_opt IDebugProperty2** ppProperty)
{
	return E_NOTIMPL;
}

HRESULT CElfDebugStackFrame::EnumProperties(
    /* [in] */ DEBUGPROP_INFO_FLAGS dwFields,
    /* [in] */ UINT nRadix,
    /* [in] */ __RPC__in REFGUID guidFilter,
    /* [in] */ DWORD dwTimeout,
    /* [out] */ __RPC__out ULONG* pcelt,
    /* [out] */ __RPC__deref_out_opt IEnumDebugPropertyInfo2** ppEnum)
{
    return E_NOTIMPL;
}

HRESULT CElfDebugStackFrame::GetThread(
    /* [out] */ __RPC__deref_out_opt IDebugThread2** ppThread)
{
	return m_pThread.QueryInterface(ppThread);
}