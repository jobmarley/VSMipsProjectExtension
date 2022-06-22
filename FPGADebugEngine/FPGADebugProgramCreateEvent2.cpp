// FPGADebugProgramCreateEvent2.cpp : Implementation of CFPGADebugProgramCreateEvent2

#include "pch.h"
#include "FPGADebugProgramCreateEvent2.h"


// CFPGADebugProgramCreateEvent2

HRESULT CFPGADebugProgramCreateEvent2::SetAttributes(DWORD dwAttrib)
{
	m_attributes = dwAttrib;
	return S_OK;
}
HRESULT CFPGADebugProgramCreateEvent2::GetAttributes(DWORD* pdwAttrib)
{
	*pdwAttrib = m_attributes;
	return S_OK;
}