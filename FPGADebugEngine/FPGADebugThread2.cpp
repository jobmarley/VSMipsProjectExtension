// FPGADebugThread2.cpp : Implementation of CFPGADebugThread2

#include "pch.h"
#include "FPGADebugThread2.h"
#include "FPGADebugProgram.h"

DWORD g_threadId = 1;

// CFPGADebugThread2
HRESULT CFPGADebugThread2::Init(const WCHAR* name, IFPGADebugProgram* pProgram)
{
	m_name = name;
	m_pProgram = pProgram;
	m_threadID = InterlockedIncrement(&g_threadId);
	return S_OK;
}

HRESULT CFPGADebugThread2::EnumFrameInfo(FRAMEINFO_FLAGS dwFieldSpec, UINT nRadix, IEnumDebugFrameInfo2** ppEnum)
{
	return E_NOTIMPL;
}
HRESULT CFPGADebugThread2::GetName(BSTR* pbstrName)
{
	*pbstrName = SysAllocString(m_name.c_str());
	return S_OK;
}
HRESULT CFPGADebugThread2::SetThreadName(LPCOLESTR pszName)
{
	m_name = pszName;
	return S_OK;
}
HRESULT CFPGADebugThread2::GetProgram(IDebugProgram2** ppProgram)
{
	return m_pProgram->QueryInterface(ppProgram);
}
HRESULT CFPGADebugThread2::CanSetNextStatement(IDebugStackFrame2* pStackFrame, IDebugCodeContext2* pCodeContext)
{
	return S_FALSE;
}
HRESULT CFPGADebugThread2::SetNextStatement(IDebugStackFrame2* pStackFrame, IDebugCodeContext2* pCodeContext)
{
	return E_NOTIMPL;
}
HRESULT CFPGADebugThread2::GetThreadId(DWORD* pdwThreadId)
{
	*pdwThreadId = m_threadID;
	return S_OK;
}
HRESULT CFPGADebugThread2::Suspend(DWORD* pdwSuspendCount)
{
	return E_NOTIMPL;
}
HRESULT CFPGADebugThread2::Resume(DWORD* pdwSuspendCount)
{
	return E_NOTIMPL;
}
HRESULT CFPGADebugThread2::GetThreadProperties(THREADPROPERTY_FIELDS dwFields, THREADPROPERTIES* ptp)
{
	return E_NOTIMPL;
}
HRESULT CFPGADebugThread2::GetLogicalThread(IDebugStackFrame2* pStackFrame, IDebugLogicalThread2** ppLogicalThread)
{
	return E_NOTIMPL;
}