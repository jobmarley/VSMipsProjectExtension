// ElfDebugStackFrame.cpp : Implementation of CElfDebugStackFrame

#include "pch.h"
#include "ElfDebugStackFrame.h"
#include "ElfDebugAddress.h"
#include "CElfSymbolProvider.h"
// CElfDebugStackFrame


HRESULT CElfDebugStackFrame::GetFirstCodeContext(IDebugCodeContext2** ppCodeContext)
{
	if (!m_pDocumentContext)
		return E_FAIL;

	CComPtr<IEnumDebugCodeContexts2> pEnumerator;
	HRESULT hr = m_pDocumentContext->EnumCodeContexts(&pEnumerator);
	if (FAILED(hr))
		return hr;

	ULONG celtFetched = 0;
	hr = pEnumerator->Next(1, ppCodeContext, &celtFetched);
	if (FAILED(hr))
		return hr;
	if (celtFetched != 1)
		return E_FAIL;
	return S_OK;
}
HRESULT CElfDebugStackFrame::Init(IElfSymbolProvider* pSymbolProvider, IDebugAddress* pAddress, IDebugThread2* pThread, ElfModule* pModule, IMemoryOperation* pMemoryOp, MipsRegisters* pRegisters)
{
	m_pSymbolProvider = pSymbolProvider;
	m_pAddress = pAddress;
	m_pThread = pThread;
	m_pModule = pModule;
	m_pMemoryOp = pMemoryOp;
	m_registers = *pRegisters;

	// we can create a stackframe if we dont have debug info
	if (pModule)
	{
		pModule->GetContextFromAddress(m_pAddress, &m_pDocumentContext);
		GetFirstCodeContext(&m_pCodeContext);
	}

	return S_OK;
}
HRESULT CElfDebugStackFrame::GetPreviousStackFrame(IDebugStackFrame2** ppStackFrame)
{
	if (!ppStackFrame)
		return E_INVALIDARG;

	*ppStackFrame = nullptr;

	MipsRegisters previousRegisters = m_registers;

	// if we dont have debug info we cannot unwind the stackframe
	if (!m_pModule)
		return E_FAIL;

	try
	{
		m_pModule->UnwindRegisters(&previousRegisters, m_pMemoryOp);
	}
	catch (std::exception)
	{
		return E_FAIL;
	}

	// We need to acquire it through the symbol provider because it can be in another module
	CComPtr<IDebugAddress> pAddress;
	HRESULT hr = m_pSymbolProvider->GetAddressFromMemory(previousRegisters.values[0], &pAddress);
	if (FAILED(hr))
		return hr;

	CComPtr<IElfDebugStackFrame> pStackFrame;
	hr = CElfDebugStackFrame::CreateInstance(&pStackFrame);
	if (FAILED(hr))
		return hr;

	CComPtr<ISymbolProviderInternal> pSPI;
	hr = m_pSymbolProvider->QueryInterface(&pSPI);
	if (FAILED(hr))
		return hr;

	// non critical empty if not found
	ElfModule* pModule = nullptr;
	hr = pSPI->GetModuleFromDebugAddress(pAddress, &pModule);

	hr = pStackFrame->Init(m_pSymbolProvider, pAddress, m_pThread, pModule, m_pMemoryOp, &previousRegisters);
	if (FAILED(hr))
		return hr;

	return pStackFrame.QueryInterface(ppStackFrame);
}
HRESULT CElfDebugStackFrame::GetCodeContext(
    /* [out] */ __RPC__deref_out_opt IDebugCodeContext2** ppCodeCxt)
{
	if (!ppCodeCxt)
		return E_INVALIDARG;

	*ppCodeCxt = nullptr;
	if (!m_pCodeContext)
		return E_FAIL;
	return m_pCodeContext.QueryInterface(ppCodeCxt);
}

HRESULT CElfDebugStackFrame::GetDocumentContext(
    /* [out] */ __RPC__deref_out_opt IDebugDocumentContext2** ppCxt)
{
	if (!ppCxt)
		return E_INVALIDARG;

	*ppCxt = nullptr;
	if (!m_pDocumentContext)
		return E_FAIL;
    return m_pDocumentContext.QueryInterface(ppCxt);
}

HRESULT CElfDebugStackFrame::GetName(
    /* [out] */ __RPC__deref_out_opt BSTR* pbstrName)
{
	if (!pbstrName)
		return E_INVALIDARG;

	FRAMEINFO fi = {};
	HRESULT hr = GetInfo(enum_FRAMEINFO_FLAGS::FIF_FUNCNAME |
		enum_FRAMEINFO_FLAGS::FIF_FUNCNAME_ARGS |
		enum_FRAMEINFO_FLAGS::FIF_FUNCNAME_LINES |
		enum_FRAMEINFO_FLAGS::FIF_FUNCNAME_MODULE, 16, &fi);
	if (FAILED(hr))
		return hr;
	CComBSTR funcName;
	funcName.Attach(fi.m_bstrFuncName);

	*pbstrName = funcName.Copy();
    return S_OK;
}

CComBSTR CElfDebugStackFrame::GetFunctionName()
{
	if (m_pCodeContext)
	{
		CONTEXT_INFO info = {};
		HRESULT hr = m_pCodeContext->GetInfo(enum_CONTEXT_INFO_FIELDS::CIF_FUNCTION, &info);
		if (SUCCEEDED(hr))
		{
			CComBSTR s;
			s.Attach(info.bstrFunction);
			return s;
		}
	}
	return nullptr;
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
		CComBSTR funcName = GetFunctionName();

		DEBUG_ADDRESS ad = {};
		m_pAddress->GetAddress(&ad);
		DWORD address = ad.addr.addr.addrNative.unknown;

		std::wstringstream ss;

		if (m_pModule && (dwFieldSpec & enum_FRAMEINFO_FLAGS::FIF_FUNCNAME_MODULE))
		{
			std::filesystem::path p = m_pModule->GetFilepath();
			ss << p.filename().wstring() << L"!";
		}
		if (funcName)
			ss << (LPWSTR)funcName;
		else
			ss << std::hex << std::setfill(L'0') << std::setw(8) << address;

		if (dwFieldSpec & enum_FRAMEINFO_FLAGS::FIF_FUNCNAME_ARGS)
			ss << "()";

		if (dwFieldSpec & enum_FRAMEINFO_FLAGS::FIF_FUNCNAME_LINES && m_pDocumentContext)
		{
			TEXT_POSITION begin = {};
			TEXT_POSITION end = {};
			HRESULT hr = m_pDocumentContext->GetSourceRange(&begin, &end);
			if (SUCCEEDED(hr))
				ss << " Line " << begin.dwLine + 1;
		}

		pFrameInfo->m_bstrFuncName = SysAllocString(ss.str().c_str());
		pFrameInfo->m_dwValidFields |= enum_FRAMEINFO_FLAGS::FIF_FUNCNAME;
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
	if (m_pModule && (dwFieldSpec & enum_FRAMEINFO_FLAGS::FIF_DEBUG_MODULEP))
	{
		m_pModule->GetDebugModule()->QueryInterface(&pFrameInfo->m_pModule);
		pFrameInfo->m_dwValidFields |= enum_FRAMEINFO_FLAGS::FIF_DEBUG_MODULEP;
	}
	if (dwFieldSpec & enum_FRAMEINFO_FLAGS::FIF_STACKRANGE)
	{
		GetPhysicalStackRange(&pFrameInfo->m_addrMin, &pFrameInfo->m_addrMax);
		pFrameInfo->m_dwValidFields |= enum_FRAMEINFO_FLAGS::FIF_STACKRANGE;
	}
	if (dwFieldSpec & enum_FRAMEINFO_FLAGS::FIF_FRAME)
	{
		((IUnknown*)this)->QueryInterface(&pFrameInfo->m_pFrame);
		pFrameInfo->m_dwValidFields |= enum_FRAMEINFO_FLAGS::FIF_FRAME;
	}
	if (dwFieldSpec & enum_FRAMEINFO_FLAGS::FIF_DEBUGINFO)
	{
		pFrameInfo->m_fHasDebugInfo = m_pDocumentContext ? TRUE : FALSE;
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
	// Not sure about that
	// Does this include prologue? depend what its used for I guess
	if (paddrMin)
		*paddrMin = m_registers.values[29]; // sp
	if (paddrMax)
		*paddrMax = m_registers.values[30]; // fp
    return S_OK;
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