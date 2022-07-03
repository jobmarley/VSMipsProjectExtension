#include "pch.h"
#include "ElfModule.h"
#include "Utils.h"
#include "ElfDebugDocumentContext.h"
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
    if (m_lineContext)
        dwarf_srclines_dealloc_b(m_lineContext);
}

void ElfModule::Load(const char* filepath)
{
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

    Dwarf_Die die = nullptr;
    result = dwarf_siblingof_b(m_dbg, die, TRUE, &die, &err);
    SafeThrowOnError(m_dbg, err);
    m_cuDie = std::make_unique<ElfDie>(m_dbg, die);

    Dwarf_Unsigned version = 0;
    Dwarf_Small table_count = 0;
    result = dwarf_srclines_b(die, &version, &table_count, &m_lineContext, &err);
    SafeThrowOnError(m_dbg, err);

    result = dwarf_srclines_from_linecontext(m_lineContext, &m_lines, &m_lineCount, &err);
    SafeThrowOnError(m_dbg, err);

    m_lang = m_cuDie->GetLang();
}


// Return the line corresponding to that address, or nullptr if not found
Dwarf_Line ElfModule::LineFromAddress(DWORD address)
{
    Dwarf_Line* start = &m_lines[0];
    Dwarf_Line* end = &m_lines[m_lineCount];
    auto get_address = [](Dwarf_Line l)
    {
        Dwarf_Addr addr = 0;
        Dwarf_Error err = nullptr;
        int result = dwarf_lineaddr(l, &addr, &err);
        return addr;
    };
    Dwarf_Line* found = std::upper_bound(start, end, address, [&get_address](DWORD v, Dwarf_Line l) { return v < get_address(l); });

    // This gets the first bigger than, so if 0, miss
    if (found == start)
        return nullptr;

    --found;
    if (get_address(*found) == address)
        return *found;
    return nullptr;
}
HRESULT ElfModule::GetContextFromAddress(DEBUG_ADDRESS* pAddress, IDebugDocumentContext2** ppDocContext)
{
    Dwarf_Line line = LineFromAddress(pAddress->addr.addr.addrNative.unknown);

    CComPtr<IElfDebugDocumentContext> pDocumentContext;
    HRESULT hr = CElfDebugDocumentContext::CreateInstance(&pDocumentContext);
    if (FAILED(hr))
        return hr;

    hr = pDocumentContext->Init(m_dbg, line, m_lang);
    if (FAILED(hr))
        return hr;

    *ppDocContext = pDocumentContext.Detach();
    return S_OK;
}
DWORD ElfModule::VirtualSize()
{
    return m_virtualSize;
}