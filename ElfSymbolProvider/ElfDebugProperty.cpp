/*
 Copyright (C) 2022 jobmarley

 This file is part of VSMipsProjectExtension.

 This program is free software; you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation; either version 3 of the License, or (at your option) any later version.

 This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

 You should have received a copy of the GNU General Public License along with Foobar. If not, see <https://www.gnu.org/licenses/>.
 */

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

bool IsChar(ElfDie* d)
{
	if (d->GetTag() == DW_TAG_base_type)
	{
		int64_t encoding = d->GetAttribute(DW_AT_encoding)->GetValue().AsInt64();
		return encoding == DW_ATE_signed_char || encoding == DW_ATE_unsigned_char;
	}
	if (d->GetTag() == DW_TAG_const_type)
		return IsChar(d->GetType());
}

std::wstring GetString(uint32_t offset, IMemoryOperation* pMemOp)
{
	uint32_t max = 0x40000000;
	char buffer[20] = { 0 };
	uint32_t count = std::min(max - offset, (uint32_t)20);
	DWORD readCount = 0;
	HRESULT hr = pMemOp->Read((BYTE*)buffer, offset, count, &readCount);
	if (FAILED(hr))
		throw std::exception();

	if (buffer[19] != '\0')
	{
		buffer[16] = '.';
		buffer[17] = '.';
		buffer[18] = '.';
		buffer[19] = '\0';
	}
	return CA2W(buffer).m_psz;
}
std::wstring FormatValue(uint32_t address, uint32_t radix, ElfDie* type, IMemoryOperation* pMemOp)
{
	switch (type->GetTag())
	{
	case DW_TAG_base_type:
	{
		int64_t byteSize = type->GetAttribute(DW_AT_byte_size)->GetValue().AsInt64();
		uint32_t value = 0;
		DWORD readCount = 0;
		HRESULT hr = pMemOp->Read((BYTE*)&value, address, byteSize, &readCount);
		if (FAILED(hr))
			throw std::exception();

		switch (type->GetAttribute(DW_AT_encoding)->GetValue().AsInt64())
		{
		case DW_ATE_boolean:
			return value != 0 ? L"true" : L"false";
		case DW_ATE_float:
		{
			std::wstringstream ss;
			ss << std::dec << std::setfill(L'0') << std::setw(0) << *reinterpret_cast<float*>(&value);
			return ss.str();
		}
		case DW_ATE_signed:
		{
			int64_t byteSize = type->GetAttribute(DW_AT_byte_size)->GetValue().AsInt64();
			std::wstringstream ss;
			if (radix == 16)
				ss << L"0x" << std::hex << std::setfill(L'0') << std::setw(byteSize * 2) << value;
			else
				ss << std::dec << std::setfill(L'0') << std::setw(0) << *reinterpret_cast<int32_t*>(&value);
			return ss.str();
		}
		case DW_ATE_unsigned:
		{
			std::wstringstream ss;
			if (radix == 16)
				ss << L"0x" << std::hex << std::setfill(L'0') << std::setw(byteSize * 2) << value;
			else
				ss << std::dec << std::setfill(L'0') << std::setw(0) << value;
			return ss.str();
		}
		case DW_ATE_signed_char:
		case DW_ATE_unsigned_char:
		{
			std::wstringstream ss;

			if (radix == 16)
				ss << L"0x" << std::hex << std::setfill(L'0') << std::setw(2) << value;
			else
				ss << std::dec << std::setfill(L'0') << std::setw(0) << value;
			ss << " '" << (char)value << "'";
			return ss.str();
		}
		default:
			return L"<unsupported value>";
		}
	}
	case DW_TAG_pointer_type:
	{
		uint32_t value = 0;
		DWORD readCount = 0;
		HRESULT hr = pMemOp->Read((BYTE*)&value, address, 4, &readCount);
		if (FAILED(hr))
			throw std::exception();

		std::wstringstream ss;
		ss << L"0x" << std::hex << std::setfill(L'0') << std::setw(8) << value;

		if (IsChar(type->GetType()) && pMemOp)
			ss << " \"" << GetString(value, pMemOp) << "\"";

		return ss.str();
	}
	case DW_TAG_array_type:
	{
		std::wstringstream ss;
		ss << L"0x" << std::hex << std::setfill(L'0') << std::setw(8) << address;

		if (IsChar(type->GetType()) && pMemOp)
			ss << " \"" << GetString(address, pMemOp) << "\"";

		return ss.str();
	}
	default:
		return L"<unsupported value>";
	}
}
std::wstring GetTypeName(ElfDie* d, uint32_t radix)
{
	if (d->GetTag() == DW_TAG_pointer_type)
		return GetTypeName(d->GetType(), radix) + L" *";
	if (d->GetTag() == DW_TAG_array_type)
	{
		auto subrange = std::ranges::find_if(d->GetChildrens(), [](std::unique_ptr<ElfDie>& p) { return p->GetTag() == DW_TAG_subrange_type; });
		std::wstringstream ss;
		ss << GetTypeName(d->GetType(), radix) << L"[";
		int64_t count = (*subrange)->GetAttribute(DW_AT_count)->GetValue().AsInt64();
		if (radix == 16)
			ss << L"0x" << std::hex << std::setfill(L'0') << std::setw(0) << count;
		else
			ss << std::dec << std::setfill(L'0') << std::setw(0) << count;
		ss << L"]";
		return ss.str();
	}
	if (d->GetTag() == DW_TAG_const_type)
		return L"const " + GetTypeName(d->GetType(), radix);
	return CA2W(d->GetName()).m_psz;
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

	DWORD address = calculator->Calculate(registers);
	return FormatValue(address, radix, m_pDie->GetType(), pMemoryOp);
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
				pPropertyInfo->bstrType = SysAllocString(GetTypeName(m_pDie->GetType(), dwRadix).c_str());
			}
			catch (std::exception e)
			{
				pPropertyInfo->bstrType = SysAllocString(L"<unsupported type>");
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