// FPGADebugProgram.cpp : Implementation of CFPGADebugProgram

#include "pch.h"
#include "FPGADebugProgram.h"
#include "FPGADebugProcess.h"
#include "FPGADebugThread2.h"

// CFPGADebugProgram

HRESULT CFPGADebugProgram::Init(IFPGADebugProcess* process, const WCHAR* name)
{
	m_pProcess = process;
	m_name = name;
	CoCreateGuid(&m_programGuid);

	CFPGADebugThread2::CreateInstance(&m_pThread);
	m_pThread->Init(L"main thread", this);
	return S_OK;
}
HRESULT CFPGADebugProgram::GetThread(IFPGADebugThread2** ppThread)
{
	return m_pThread->QueryInterface(ppThread);
}

//////////////////////////////////////////////////////////// 
//IDebugProgram2
HRESULT CFPGADebugProgram::EnumThreads(IEnumDebugThreads2** ppEnum)
{
	return E_NOTIMPL;
}
HRESULT CFPGADebugProgram::GetName(BSTR* pbstrName)
{
	*pbstrName = SysAllocString(m_name.c_str());
	return S_OK;
}
HRESULT CFPGADebugProgram::GetProcess(IDebugProcess2** ppProcess)
{
	if (m_pProcess)
		m_pProcess->AddRef();
	*ppProcess = m_pProcess;
	return S_OK;
}
HRESULT CFPGADebugProgram::Terminate(void)
{
	return E_NOTIMPL;
}
HRESULT CFPGADebugProgram::Attach(IDebugEventCallback2* pCallback)
{
	return E_NOTIMPL;
}
HRESULT CFPGADebugProgram::CanDetach(void)
{
	return E_NOTIMPL;
}
HRESULT CFPGADebugProgram::Detach(void)
{
	return S_OK;
}
HRESULT CFPGADebugProgram::GetProgramId(GUID* pguidProgramId)
{
	*pguidProgramId = m_programGuid;
	return S_OK;
}
HRESULT CFPGADebugProgram::GetDebugProperty(IDebugProperty2** ppProperty)
{
	return E_NOTIMPL;
}
HRESULT CFPGADebugProgram::Execute(void)
{
	return E_NOTIMPL;
}
HRESULT CFPGADebugProgram::Continue(IDebugThread2* pThread)
{
	return E_NOTIMPL;
}
HRESULT CFPGADebugProgram::Step(IDebugThread2* pThread, STEPKIND sk, STEPUNIT step)
{
	return E_NOTIMPL;
}
HRESULT CFPGADebugProgram::CauseBreak(void)
{
	return E_NOTIMPL;
}
HRESULT CFPGADebugProgram::GetEngineInfo(BSTR* pbstrEngine, GUID* pguidEngine)
{
	return E_NOTIMPL;
}
HRESULT CFPGADebugProgram::EnumCodeContexts(IDebugDocumentPosition2* pDocPos, IEnumDebugCodeContexts2** ppEnum)
{
	return E_NOTIMPL;
}
HRESULT CFPGADebugProgram::GetMemoryBytes(IDebugMemoryBytes2** ppMemoryBytes)
{
	return E_NOTIMPL;
}
HRESULT CFPGADebugProgram::GetDisassemblyStream(
    DISASSEMBLY_STREAM_SCOPE dwScope,
    IDebugCodeContext2* pCodeContext,
    IDebugDisassemblyStream2** ppDisassemblyStream)
{
	return E_NOTIMPL;
}
HRESULT CFPGADebugProgram::EnumModules(IEnumDebugModules2** ppEnum)
{
	return E_NOTIMPL;
}
HRESULT CFPGADebugProgram::GetENCUpdate(IDebugENCUpdate** ppUpdate)
{
	return E_NOTIMPL;
}
HRESULT CFPGADebugProgram::EnumCodePaths(
    LPCOLESTR pszHint,
    IDebugCodeContext2* pStart,
    IDebugStackFrame2* pFrame,
    BOOL fSource,
    IEnumCodePaths2** ppEnum,
    IDebugCodeContext2** ppSafety)
{
	return E_NOTIMPL;
}
HRESULT CFPGADebugProgram::WriteDump(DUMPTYPE DumpType, LPCOLESTR pszDumpUrl)
{
	return E_NOTIMPL;
}