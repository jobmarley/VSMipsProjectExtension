#include "pch.h"
#include "ElfModule.h"
#include "Utils.h"
#include "ElfDebugDocumentContext.h"
#include "ElfDebugStackFrame.h"
#include "ElfUtils.h"

uint32_t CalculateVirtualSize(const char* filepath)
{
    std::fstream f(filepath, std::ios::binary | std::ios::in);
    if (f.bad())
        throw std::exception();

    elf_header hdr = {};
    f.read(reinterpret_cast<char*>(&hdr), sizeof(hdr));
    if (f.bad())
        throw std::exception();
    if (hdr.magic == ELF_MAGIC)
    {
        uint32_t lower = 0xFFFFFFFF;
        uint32_t upper = 0;
        for (uint16_t i = 0; i < hdr.e_phnum; i++)
        {
            uint32_t ofs = hdr.e_phoff + i * hdr.e_phentsize;
            f.seekg(ofs, std::ios::beg);
            if (f.bad())
                throw std::exception();

            elf_program_header ph = {};
            f.read(reinterpret_cast<char*>(&ph), sizeof(ph));
            if (f.bad())
                throw std::exception();

            if (!(ph.p_type & PT_LOAD))
                continue;

            if (ph.p_memsz == 0)
                continue;

            lower = std::min(lower, ph.p_vaddr);
            upper = std::max(upper, ph.p_vaddr + ph.p_memsz);
        }
        return (upper >= lower) ? (upper - lower) : 0;
    }
    f.seekg(0, std::ios::end);
    return static_cast<uint32_t>(f.tellg());
}

ElfModule::ElfModule()
{
}
ElfModule::~ElfModule()
{
    if (m_dbg)
        dwarf_finish(m_dbg);
}

void ElfModule::LoadChildren(ElfDie* die)
{
    Dwarf_Die d = nullptr;
    Dwarf_Error err = nullptr;
    int result = dwarf_child(die->Die(), &d, &err);
    SafeThrowOnError(m_dbg, err);

    while (result == DW_DLV_OK)
    {
        ElfDie* child = nullptr;
        {
            auto s = std::make_unique<ElfDie>(m_dbg, d, die);
            child = s.get();
            die->GetChildrens().push_back(std::move(s));
        }

        if (child->GetTag() == DW_TAG_subprogram)
        {
            m_subprograms[static_cast<DWORD>(child->GetLowPc())] = child;
        }

        LoadChildren(child);
        
        result = dwarf_siblingof_b(m_dbg, d, true, &d, &err);
        SafeThrowOnError(m_dbg, err);
    }
}
void ElfModule::Load(IDebugModule2* pDebugModule, const char* filepath)
{
    m_pDebugModule = pDebugModule;
    m_filepath = filepath;

    m_virtualSize = CalculateVirtualSize(filepath);

    Dwarf_Error err = nullptr;
    int result = dwarf_init_path(filepath,
        nullptr,
        0,
        DW_GROUPNUMBER_ANY,
        nullptr,
        nullptr,
        &m_dbg,
        &err);
    SafeThrowOnError(m_dbg, err);

    if (result != DW_DLV_OK)
        throw std::exception();

    while (result == DW_DLV_OK)
    {
        Dwarf_Unsigned dw_cu_header_length = 0;
        Dwarf_Half dw_version_stamp = 0;
        Dwarf_Off dw_abbrev_offset = 0;
        Dwarf_Half dw_address_size = 0;
        Dwarf_Half dw_length_size = 0;
        Dwarf_Half dw_extension_size = 0;
        Dwarf_Sig8 dw_type_signature = {};
        Dwarf_Unsigned dw_typeoffset = 0;
        Dwarf_Unsigned dw_next_cu_header_offset = 0;
        Dwarf_Half dw_header_cu_type = 0;
        result = dwarf_next_cu_header_d(m_dbg,
            TRUE,
            &dw_cu_header_length,
            &dw_version_stamp,
            &dw_abbrev_offset,
            &dw_address_size,
            &dw_length_size,
            &dw_extension_size,
            &dw_type_signature,
            &dw_typeoffset,
            &dw_next_cu_header_offset,
            &dw_header_cu_type,
            &err);
        SafeThrowOnError(m_dbg, err);

        if (result != DW_DLV_OK)
            break;

        Dwarf_Die die = nullptr;
        result = dwarf_siblingof_b(m_dbg, die, TRUE, &die, &err);
        SafeThrowOnError(m_dbg, err);
        cu_info cui = {};
        cui.die = std::make_unique<ElfDie>(m_dbg, die, nullptr);
        cui.lineTable = std::make_unique<ElfLineTable>(m_dbg, die);

        LoadChildren(cui.die.get());

        Dwarf_Addr addr = cui.die->GetLowPc();
        m_compilationUnits[static_cast<DWORD>(addr)] = std::move(cui);
    }
}


ElfModule::cu_info* ElfModule::CUFromAddress(DWORD address)
{
	auto found = m_compilationUnits.upper_bound(address);

	// This gets the first bigger than, so if 0, miss
	if (found == m_compilationUnits.begin())
		return nullptr;

	--found;
	return &found->second;
}
HRESULT ElfModule::GetContextFromAddress(IDebugAddress* pAddress, IDebugDocumentContext2** ppDocContext)
{
    DEBUG_ADDRESS ad = {};
    HRESULT hr = pAddress->GetAddress(&ad);
    if (FAILED(hr))
        return hr;

    DWORD address = ad.addr.addr.addrNative.unknown;
    auto* cu_info = CUFromAddress(address);
    if (!cu_info)
        return E_FAIL;

    Dwarf_Line line = cu_info->lineTable->LineFromAddress(address);

    CComPtr<IElfDebugDocumentContext> pDocumentContext;
    hr = CElfDebugDocumentContext::CreateInstance(&pDocumentContext);
    if (FAILED(hr))
        return hr;

    hr = pDocumentContext->Init(this, pAddress, m_dbg, line, cu_info->die->GetLang());
    if (FAILED(hr))
        return hr;

    *ppDocContext = pDocumentContext.Detach();
    return S_OK;
}
DWORD ElfModule::VirtualSize()
{
    return m_virtualSize;
}

HRESULT ElfModule::GetStackFrame(IDebugAddress* pAddress, IDebugThread2* pThread, IDebugStackFrame2** ppStackFrame)
{
    CComPtr<IElfDebugStackFrame> pStackFrame;
    HRESULT hr = CElfDebugStackFrame::CreateInstance(&pStackFrame);
    if (FAILED(hr))
        return hr;

    hr = pStackFrame->Init(pAddress, pThread, this);
    if (FAILED(hr))
        return hr;

    return pStackFrame.QueryInterface(ppStackFrame);
}
ElfFunction ElfModule::GetFunction(IDebugAddress* pAddress)
{
    DEBUG_ADDRESS ad = {};
    HRESULT hr = pAddress->GetAddress(&ad);
    if (FAILED(hr))
        throw std::exception();

    DWORD address = ad.addr.addr.addrNative.unknown;

    auto found = m_subprograms.upper_bound(address);

    // This gets the first bigger than, so if 0, miss
    if (found == m_subprograms.begin())
        throw std::exception();

    --found;
    return ElfFunction(found->second);
}