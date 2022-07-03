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