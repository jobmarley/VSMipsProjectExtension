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
#include "SimpleEnumerator.h"

// CElfDebugProperty

HRESULT CElfDebugProperty::Init(BSTR name, ElfType type, DWORD address, IDebugDocumentContext2* pDocumentContext, IElfDebugStackFrame* pStackFrame)
{
	m_name = name;
	m_type = std::make_unique<ElfType>(type);
	m_address = address;
	m_pDocumentContext = pDocumentContext;
	m_pStackFrame = pStackFrame;
	return S_OK;
}

bool IsChar(ElfType t)
{
	if (t.GetDie()->GetTag() == DW_TAG_base_type)
	{
		int64_t encoding = t.GetEncoding();
		return encoding == DW_ATE_signed_char || encoding == DW_ATE_unsigned_char;
	}
	if (t.GetDie()->GetTag() == DW_TAG_const_type)
		return IsChar(t.GetReferencedType());
}

void FormatChar(std::wstringstream& ss, char c, uint32_t radix)
{
	if (isgraph(c))
	{
		ss << c;
	}
	else
	{
		if (radix == 16)
			ss << "\\" << "x" << std::hex << std::setfill(L'0') << std::setw(2) << (uint32_t)(uint8_t)c;
		else
			ss << "\\" << std::dec << (uint32_t)(uint8_t)c;
	}
}

std::wstring GetString(uint32_t offset, uint32_t radix, IMemoryOperation* pMemOp)
{
	if (offset == 0)
		return L"";

	uint32_t max = 0x40000000;
	std::array<char, 20> buffer = { 0 };
	uint32_t count = std::min(max - offset, (uint32_t)buffer.size());
	DWORD readCount = 0;
	HRESULT hr = pMemOp->Read((BYTE*)&buffer[0], offset, count, &readCount);
	if (FAILED(hr))
		throw std::exception();

	std::wstringstream ss;
	int len = strnlen_s(&buffer[0], buffer.size());
	if (len < buffer.size())
	{
		for (int i = 0; i < len; ++i)
			FormatChar(ss, buffer[i], radix);
	}
	else
	{
		for (int i = 0; i < buffer.size() - 3; ++i)
			FormatChar(ss, buffer[i], radix);
		ss << "...";
	}
	return ss.str();
}


std::wstring FormatValue(uint32_t address, uint32_t radix, ElfType type, IMemoryOperation* pMemOp)
{
	try
	{
		if (address == 0)
			return L"";

		switch (type.GetDie()->GetTag())
		{
		case DW_TAG_base_type:
		{
			int64_t byteSize = type.GetByteSize();
			uint32_t value = 0;
			DWORD readCount = 0;
			HRESULT hr = pMemOp->Read((BYTE*)&value, address, byteSize, &readCount);
			if (FAILED(hr))
				throw std::exception();

			switch (type.GetEncoding())
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
				ss << "'";
				FormatChar(ss, value, radix);
				ss << "'";
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

			if (value != 0)
			{
				if (IsChar(type.GetReferencedType()) && pMemOp)
					ss << " \"" << GetString(value, radix, pMemOp) << "\"";
				else
					ss << "{" << "}";
			}

			return ss.str();
		}
		case DW_TAG_array_type:
		{
			std::wstringstream ss;
			ss << L"0x" << std::hex << std::setfill(L'0') << std::setw(8) << address;

			if (IsChar(type.GetReferencedType()) && pMemOp)
				ss << " \"" << GetString(address, radix, pMemOp) << "\"";

			return ss.str();
		}
		case DW_TAG_const_type:
		{
			return FormatValue(address, radix, type.GetReferencedType(), pMemOp);
		}
		default:
			return L"<unsupported value>";
		}
	}
	catch (...)
	{

	}
	return L"<unsupported value>";
}
std::wstring GetTypeName(ElfType t, uint32_t radix)
{
	if (t.IsPointer())
		return GetTypeName(t.GetReferencedType(), radix) + L" *";
	if (t.IsArray())
	{
		std::wstringstream ss;
		ss << GetTypeName(t.GetReferencedType(), radix) << L"[";
		int64_t count = t.GetCount();
		if (radix == 16)
			ss << L"0x" << std::hex << std::setfill(L'0') << std::setw(8) << count;
		else
			ss << std::dec << std::setfill(L'0') << std::setw(0) << count;
		ss << L"]";
		return ss.str();
	}
	if (t.IsConst())
		return L"const " + GetTypeName(t.GetReferencedType(), radix);
	return CA2W(t.GetName().c_str()).m_psz;
}

std::wstring CElfDebugProperty::GetValue(DWORD radix)
{
	CComPtr<IMemoryOperation> pMemoryOp;
	HRESULT hr = m_pStackFrame->GetMemoryOperation(&pMemoryOp);
	if (FAILED(hr))
		throw std::exception();
	return FormatValue(m_address, radix, *m_type, pMemoryOp);
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
		pPropertyInfo->bstrName = SysAllocString(m_name);
		pPropertyInfo->dwFields |= enum_DEBUGPROP_INFO_FLAGS::DEBUGPROP_INFO_FULLNAME;
	}
	if (dwFields & enum_DEBUGPROP_INFO_FLAGS::DEBUGPROP_INFO_NAME)
	{
		pPropertyInfo->bstrName = SysAllocString(m_name);
		pPropertyInfo->dwFields |= enum_DEBUGPROP_INFO_FLAGS::DEBUGPROP_INFO_NAME;
	}
	if (dwFields & enum_DEBUGPROP_INFO_FLAGS::DEBUGPROP_INFO_TYPE)
	{
		try
		{
			pPropertyInfo->bstrType = SysAllocString(GetTypeName(*m_type, dwRadix).c_str());
		}
		catch (std::exception e)
		{
			pPropertyInfo->bstrType = SysAllocString(L"<unsupported type>");
		}
		pPropertyInfo->dwFields |= enum_DEBUGPROP_INFO_FLAGS::DEBUGPROP_INFO_TYPE;
	}
	if (dwFields & enum_DEBUGPROP_INFO_FLAGS::DEBUGPROP_INFO_VALUE)
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
	if (dwFields & enum_DEBUGPROP_INFO_FLAGS::DEBUGPROP_INFO_ATTRIB)
	{
		pPropertyInfo->dwAttrib |= DBG_ATTRIB_ACCESS_PUBLIC | DBG_ATTRIB_DATA;
		if (m_type->IsPointer() || (m_type->IsArray() && m_type->GetCount() > 0))
			pPropertyInfo->dwAttrib |= DBG_ATTRIB_OBJ_IS_EXPANDABLE;
		pPropertyInfo->dwFields |= enum_DEBUGPROP_INFO_FLAGS::DEBUGPROP_INFO_ATTRIB;
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
	if (!m_childrenEnumerated)
	{
		if (m_type->IsPointer())
		{
			CComPtr<IElfDebugProperty> pProperty;
			HRESULT hr = CElfDebugProperty::CreateInstance(&pProperty);
			if (FAILED(hr))
				throw std::exception();

			CComPtr<IMemoryOperation> pMemoryOp;
			hr = m_pStackFrame->GetMemoryOperation(&pMemoryOp);
			if (FAILED(hr))
				throw std::exception();
			DWORD ptrValue = 0;
			DWORD readCount = 0;
			hr = pMemoryOp->Read((BYTE*)&ptrValue, m_address, 4, &readCount);
			if (FAILED(hr))
				throw std::exception();

			hr = pProperty->Init(CComBSTR(), m_type->GetReferencedType(), ptrValue, m_pDocumentContext, m_pStackFrame);
			if (FAILED(hr))
				throw std::exception();

			m_children.push_back(pProperty);
		}
		else if (m_type->IsArray())
		{
			for (int i = 0; i < m_type->GetCount(); ++i)
			{
				CComPtr<IElfDebugProperty> pProperty;
				HRESULT hr = CElfDebugProperty::CreateInstance(&pProperty);
				if (FAILED(hr))
					throw std::exception();

				std::wstringstream ss;
				ss << L"[";
				if (dwRadix == 16)
					ss << L"0x" << std::hex << std::setfill(L'0') << std::setw(8) << i;
				else
					ss << std::dec << std::setfill(L'0') << std::setw(0) << i; 
				ss << L"]";
				hr = pProperty->Init(CComBSTR(ss.str().c_str()), m_type->GetReferencedType(), m_address + i, m_pDocumentContext, m_pStackFrame);
				if (FAILED(hr))
					throw std::exception();

				m_children.push_back(pProperty);
			}
		}
		m_childrenEnumerated = true;
	}

	std::vector<DEBUG_PROPERTY_INFO> infos(m_children.size());
	for (int i = 0; i < m_children.size(); ++i)
	{
		if (FAILED(m_children[i]->GetPropertyInfo(dwFields, dwRadix, dwTimeout, nullptr, 0, &infos[i])))
			throw std::exception();
	}
	return SimpleEnumerator<IEnumDebugPropertyInfo2>::Create(infos, ppEnum);
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