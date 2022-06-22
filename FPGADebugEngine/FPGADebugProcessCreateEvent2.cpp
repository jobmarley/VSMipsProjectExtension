// FPGADebugProcessCreateEvent2.cpp : Implementation of CFPGADebugProcessCreateEvent2

#include "pch.h"
#include "FPGADebugProcessCreateEvent2.h"


// CFPGADebugProcessCreateEvent2


HRESULT CFPGADebugProcessCreateEvent2::SetAttributes(DWORD dwAttrib)
{
	m_attributes = dwAttrib;
	return S_OK;
}
HRESULT CFPGADebugProcessCreateEvent2::GetAttributes(DWORD* pdwAttrib)
{
	*pdwAttrib = m_attributes;
	return S_OK;
}