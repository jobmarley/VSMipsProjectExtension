/*
 Copyright (C) 2022 jobmarley

 This file is part of VSMipsProjectExtension.

 This program is free software; you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation; either version 3 of the License, or (at your option) any later version.

 This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

 You should have received a copy of the GNU General Public License along with Foobar. If not, see <https://www.gnu.org/licenses/>.
 */

// ElfDebugAddress.cpp : Implementation of CElfDebugAddress

#include "pch.h"
#include "ElfDebugAddress.h"


// CElfDebugAddress


HRESULT CElfDebugAddress::SetAddress(
	/* [out] */ __RPC__out DEBUG_ADDRESS* pAddress)
{
	m_address = *pAddress;
	return S_OK;
}
HRESULT CElfDebugAddress::GetAddress(
	/* [out] */ __RPC__out DEBUG_ADDRESS* pAddress)
{
	*pAddress = m_address;
	return S_OK;
}