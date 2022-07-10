// ElfDebugProperty.cpp : Implementation of CElfDebugProperty

#include "pch.h"
#include "ElfDebugProperty.h"
#include "ElfDebugStackFrame.h"

// CElfDebugProperty

HRESULT CElfDebugProperty::Init(ElfDie* pDie, IDebugDocumentContext2* pDocumentContext, IElfDebugStackFrame* pStackFrame)
{
	m_pDie = pDie;
	m_pDocumentContext = pDocumentContext;
	m_pStackFrame = pStackFrame;
	return S_OK;
}

std::wstring CElfDebugProperty::GetValue(DWORD radix)
{
	auto attr = m_pDie->GetAttribute(DW_AT_location);
	
	CComPtr<IMemoryOperation> pMemoryOp;
	HRESULT hr = m_pStackFrame->GetMemoryOperation(&pMemoryOp);
	if (FAILED(hr))
		throw std::exception();
	auto calculator = attr->GetCalculator(pMemoryOp);

	MipsRegisters registers = {};
	hr = m_pStackFrame->GetRegisters(&registers);
	if (FAILED(hr))
		throw std::exception();

	DWORD result = calculator->Calculate(registers);

	std::wstringstream ss;
	if (radix == 16)
		ss << std::hex << std::setfill(L'0') << std::setw(8) << L"0x" << result;
	else
		ss << std::dec << std::setfill(L'0') << std::setw(0) << result;
	return ss.str();
}
HRESULT CElfDebugProperty::GetPropertyInfo(
    /* [in] */ DEBUGPROP_INFO_FLAGS dwFields,
    /* [in] */ DWORD dwRadix,
    /* [in] */ DWORD dwTimeout,
    /* [length_is][size_is][full][in] */ __RPC__in_ecount_part_opt(dwArgCount, dwArgCount) IDebugReference2** rgpArgs,
    /* [in] */ DWORD dwArgCount,
    /* [out] */ __RPC__out DEBUG_PROPERTY_INFO* pPropertyInfo)
{
	if (!pPropertyInfo)
		return E_INVALIDARG;

	if (dwFields & enum_DEBUGPROP_INFO_FLAGS::DEBUGPROP_INFO_FULLNAME)
	{
		if (m_pDie)
		{
			pPropertyInfo->bstrName = SysAllocString(CA2W(m_pDie->GetName()));
			pPropertyInfo->dwFields |= enum_DEBUGPROP_INFO_FLAGS::DEBUGPROP_INFO_FULLNAME;
		}
	}
	if (dwFields & enum_DEBUGPROP_INFO_FLAGS::DEBUGPROP_INFO_NAME)
	{
		if (m_pDie)
		{
			pPropertyInfo->bstrName = SysAllocString(CA2W(m_pDie->GetName()));
			pPropertyInfo->dwFields |= enum_DEBUGPROP_INFO_FLAGS::DEBUGPROP_INFO_NAME;
		}
	}
	if (dwFields & enum_DEBUGPROP_INFO_FLAGS::DEBUGPROP_INFO_TYPE)
	{
		if (m_pDie)
		{
			try
			{
				pPropertyInfo->bstrType = SysAllocString(CA2W(m_pDie->GetType()->GetName()));
			}
			catch (std::exception e)
			{
				pPropertyInfo->bstrType = SysAllocString(L"function");
			}
			pPropertyInfo->dwFields |= enum_DEBUGPROP_INFO_FLAGS::DEBUGPROP_INFO_TYPE;
		}
	}
	if (dwFields & enum_DEBUGPROP_INFO_FLAGS::DEBUGPROP_INFO_VALUE)
	{
		if (m_pDie)
		{
			std::wstring sValue = L"";
			try
			{
				sValue = GetValue(dwRadix);
			}
			catch (...)
			{ }
			pPropertyInfo->bstrValue = SysAllocString(sValue.c_str());
			pPropertyInfo->dwFields |= enum_DEBUGPROP_INFO_FLAGS::DEBUGPROP_INFO_VALUE;
		}
	}
	if (dwFields & enum_DEBUGPROP_INFO_FLAGS::DEBUGPROP_INFO_ATTRIB)
	{
		if (m_pDie)
		{
			pPropertyInfo->dwAttrib |= DBG_ATTRIB_ACCESS_PUBLIC | DBG_ATTRIB_DATA;
			pPropertyInfo->dwFields |= enum_DEBUGPROP_INFO_FLAGS::DEBUGPROP_INFO_ATTRIB;
		}
	}
	if (dwFields & enum_DEBUGPROP_INFO_FLAGS::DEBUGPROP_INFO_PROP)
	{
		((IUnknown*)this)->QueryInterface(&pPropertyInfo->pProperty);
		pPropertyInfo->dwFields |= enum_DEBUGPROP_INFO_FLAGS::DEBUGPROP_INFO_PROP;
	}
	if (dwFields & enum_DEBUGPROP_INFO_FLAGS::DEBUGPROP_INFO_VALUE_AUTOEXPAND)
	{
		pPropertyInfo->dwFields |= enum_DEBUGPROP_INFO_FLAGS::DEBUGPROP_INFO_VALUE_AUTOEXPAND;
	}
	if (dwFields & enum_DEBUGPROP_INFO_FLAGS::DEBUGPROP_INFO_NOFUNCEVAL)
	{
		pPropertyInfo->dwFields |= enum_DEBUGPROP_INFO_FLAGS::DEBUGPROP_INFO_NOFUNCEVAL;
	}
	if (dwFields & enum_DEBUGPROP_INFO_FLAGS::DEBUGPROP_INFO_VALUE_RAW)
	{
		pPropertyInfo->dwFields |= enum_DEBUGPROP_INFO_FLAGS::DEBUGPROP_INFO_VALUE_RAW;
	}
	if (dwFields & enum_DEBUGPROP_INFO_FLAGS::DEBUGPROP_INFO_VALUE_NO_TOSTRING)
	{
		pPropertyInfo->dwFields |= enum_DEBUGPROP_INFO_FLAGS::DEBUGPROP_INFO_VALUE_NO_TOSTRING;
	}
	if (dwFields & enum_DEBUGPROP_INFO_FLAGS::DEBUGPROP_INFO_NO_NONPUBLIC_MEMBERS)
	{
		//pPropertyInfo->dwFields |= enum_DEBUGPROP_INFO_FLAGS::DEBUGPROP_INFO_NO_NONPUBLIC_MEMBERS;
	}
	return S_OK;
}

HRESULT CElfDebugProperty::SetValueAsString(
    /* [in] */ __RPC__in LPCOLESTR pszValue,
    /* [in] */ DWORD dwRadix,
    /* [in] */ DWORD dwTimeout)
{
	return E_NOTIMPL;
}

HRESULT CElfDebugProperty::SetValueAsReference(
    /* [length_is][size_is][full][in] */ __RPC__in_ecount_part_opt(dwArgCount, dwArgCount) IDebugReference2** rgpArgs,
    /* [in] */ DWORD dwArgCount,
    /* [in] */ __RPC__in_opt IDebugReference2* pValue,
    /* [in] */ DWORD dwTimeout)
{
	return E_NOTIMPL;
}

HRESULT CElfDebugProperty::EnumChildren(
    /* [in] */ DEBUGPROP_INFO_FLAGS dwFields,
    /* [in] */ DWORD dwRadix,
    /* [in] */ __RPC__in REFGUID guidFilter,
    /* [in] */ DBG_ATTRIB_FLAGS dwAttribFilter,
    /* [full][in] */ __RPC__in_opt LPCOLESTR pszNameFilter,
    /* [in] */ DWORD dwTimeout,
    /* [out] */ __RPC__deref_out_opt IEnumDebugPropertyInfo2** ppEnum)
{
	return E_NOTIMPL;
}

HRESULT CElfDebugProperty::GetParent(
    /* [out] */ __RPC__deref_out_opt IDebugProperty2** ppParent)
{
	return E_NOTIMPL;
}

HRESULT CElfDebugProperty::GetDerivedMostProperty(
    /* [out] */ __RPC__deref_out_opt IDebugProperty2** ppDerivedMost)
{
	return E_NOTIMPL;
}

HRESULT CElfDebugProperty::GetMemoryBytes(
    /* [out] */ __RPC__deref_out_opt IDebugMemoryBytes2** ppMemoryBytes)
{
	return E_NOTIMPL;
}

HRESULT CElfDebugProperty::GetMemoryContext(
    /* [out] */ __RPC__deref_out_opt IDebugMemoryContext2** ppMemory)
{
	return E_NOTIMPL;
}

HRESULT CElfDebugProperty::GetSize(
    /* [out] */ __RPC__out DWORD* pdwSize)
{
	return E_NOTIMPL;
}

HRESULT CElfDebugProperty::GetReference(
    /* [out] */ __RPC__deref_out_opt IDebugReference2** ppReference)
{
	return E_NOTIMPL;
}

HRESULT CElfDebugProperty::GetExtendedInfo(
    /* [in] */ __RPC__in REFGUID guidExtendedInfo,
    /* [out] */ __RPC__out VARIANT* pExtendedInfo)
{
	if (guidCodeContext == guidExtendedInfo)
	{
		CComVariant v;
		v = m_pDocumentContext;
		v.Detach(pExtendedInfo);
		return S_OK;
	}
	return S_GETEXTENDEDINFO_NO_EXTENDEDINFO;
}