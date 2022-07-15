/*
 Copyright (C) 2022 jobmarley

 This file is part of VSMipsProjectExtension.

 This program is free software; you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation; either version 3 of the License, or (at your option) any later version.

 This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

 You should have received a copy of the GNU General Public License along with Foobar. If not, see <https://www.gnu.org/licenses/>.
 */

#include "pch.h"
#include "ElfModule.h"
#include "Utils.h"
#include "ElfDebugDocumentContext.h"
#include "ElfDebugStackFrame.h"
#include "ElfUtils.h"
#include "CElfSymbolProvider.h"

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

ElfModule::ElfModule(IElfSymbolProvider* pSymbolProvider)
{
    m_pSymbolProvider = pSymbolProvider;
}
ElfModule::~ElfModule()
{
    m_compilationUnits.clear();

    if (m_cieTable || m_fdeTable)
        dwarf_dealloc_fde_cie_list(m_dbg, m_cieTable, m_cieCount, m_fdeTable, m_fdeCount);

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
            auto s = std::make_unique<ElfDie>(m_dbg, d, this, die);
            child = s.get();
            die->GetChildrens().push_back(std::move(s));
        }

        m_diesFromOfs[child->GetDwarfOfs()] = child;

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
        cui.die = std::make_unique<ElfDie>(m_dbg, die, this, nullptr);
        cui.lineTable = std::make_unique<ElfLineTable>(m_dbg, die);

        m_diesFromOfs[cui.die->GetDwarfOfs()] = cui.die.get();

        LoadChildren(cui.die.get());

        Dwarf_Addr addr = cui.die->GetLowPc();
        m_compilationUnits[static_cast<DWORD>(addr)] = std::move(cui);
    }

    LoadFrames();
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

HRESULT ElfModule::GetStackFrame(
    IDebugAddress* pAddress,
    IDebugThread2* pThread,
    IMemoryOperation* pMemoryOp,
    IRegisterOperation* pRegisterOp,
    IDebugStackFrame2** ppStackFrame)
{
    CComPtr<IElfDebugStackFrame> pStackFrame;
    HRESULT hr = CElfDebugStackFrame::CreateInstance(&pStackFrame);
    if (FAILED(hr))
        return hr;

    MipsRegisters registers = {};
    for (DWORD i = 0; i < 32; ++i)
        pRegisterOp->Read(i, &registers.values[i]);

    hr = pStackFrame->Init(m_pSymbolProvider, pAddress, pThread, this, pMemoryOp, &registers);
    if (FAILED(hr))
        return hr;

    return pStackFrame.QueryInterface(ppStackFrame);
}
ElfDie* ElfModule::GetFunction(IDebugAddress* pAddress)
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
    return found->second;
}
void ElfModule::LoadFrames()
{
    Dwarf_Error err = nullptr;
    int result = dwarf_get_fde_list(m_dbg, &m_cieTable, &m_cieCount, &m_fdeTable, &m_fdeCount, &err);
    SafeThrowOnError(m_dbg, err);
}


auto GetFdeBounds(Dwarf_Debug dbg, Dwarf_Fde fde)
{
    Dwarf_Addr dw_low_pc = 0;
    Dwarf_Unsigned dw_func_length = 0;
    Dwarf_Small* dw_fde_bytes = nullptr;
    Dwarf_Unsigned dw_fde_byte_length = 0;
    Dwarf_Off dw_cie_offset = 0;
    Dwarf_Signed dw_cie_index = 0;
    Dwarf_Off dw_fde_offset = 0;
    Dwarf_Error dw_error = nullptr;
    int result = dwarf_get_fde_range(fde,
        &dw_low_pc,
        &dw_func_length,
        &dw_fde_bytes,
        &dw_fde_byte_length,
        &dw_cie_offset,
        &dw_cie_index,
        &dw_fde_offset,
        &dw_error);
    SafeThrowOnError(dbg, dw_error);
    return std::make_pair(dw_low_pc, dw_low_pc + dw_func_length);
}
Dwarf_Fde ElfModule::FdeFromAddress(DWORD address)
{
    auto start = m_fdeTable;
    auto end = &m_fdeTable[m_fdeCount];
    auto found = std::upper_bound(start, end, address,
        [this](DWORD address, Dwarf_Fde fde)
        {
            return address < GetFdeBounds(m_dbg, fde).first;
        });

    // This gets the first bigger than, so if 0, miss
    if (found == end)
        return nullptr;

    --found;

    auto bounds = GetFdeBounds(m_dbg, *found);
    if (bounds.first <= address && address <= bounds.second - 4)
        return *found;
    return nullptr;
}
void ElfModule::GetRegisters(DWORD address)
{
    Dwarf_Fde fde = FdeFromAddress(address);
    if (!fde)
        throw std::exception();

	Dwarf_Regtable3 table = {};
    std::vector<Dwarf_Regtable_Entry3_s> entries(64);
    table.rt3_reg_table_size = static_cast<Dwarf_Half>(entries.size());
    table.rt3_rules = entries.data();

	Dwarf_Addr ofs = 0;
	Dwarf_Error err = nullptr;
	int result = dwarf_get_fde_info_for_all_regs3(fde, address, &table, &ofs, &err);
    int qzdqzd = 0;
}

void ElfModule::UnwindRegisters(MipsRegisters* pRegisters, IMemoryOperation* pMemoryOp)
{
    DWORD address = pRegisters->values[0];
    Dwarf_Fde fde = FdeFromAddress(address);
    if (!fde)
        throw std::exception();

    Dwarf_Cie cie = nullptr;
    Dwarf_Error err = nullptr;
    int result = dwarf_get_cie_of_fde(fde, &cie, &err);
    SafeThrowOnError(m_dbg, err);
    if (result != DW_DLV_OK)
        throw std::exception();

    Dwarf_Unsigned dw_bytes_in_cie = 0;
    Dwarf_Small dw_version = 0;
    char* dw_augmenter = nullptr;
    Dwarf_Unsigned dw_code_alignment_factor = 0;
    Dwarf_Signed dw_data_alignment_factor = 0;
    Dwarf_Half dw_return_address_register_rule = 0;
    Dwarf_Small* dw_initial_instructions = nullptr;
    Dwarf_Unsigned dw_initial_instructions_length = 0;
    Dwarf_Half dw_offset_size = 0;
    result = dwarf_get_cie_info_b(cie,
        &dw_bytes_in_cie,
        &dw_version,
        &dw_augmenter,
        &dw_code_alignment_factor,
        &dw_data_alignment_factor,
        &dw_return_address_register_rule,
        &dw_initial_instructions,
        &dw_initial_instructions_length,
        &dw_offset_size,
        &err);
    SafeThrowOnError(m_dbg, err);
    if (result != DW_DLV_OK)
        throw std::exception();

    Dwarf_Regtable3 table = {};
    std::vector<Dwarf_Regtable_Entry3_s> entries(32);
    table.rt3_reg_table_size = static_cast<Dwarf_Half>(entries.size());
    table.rt3_rules = entries.data();

    DWORD pc = pRegisters->values[0];

    Dwarf_Addr ofs = 0;
    err = nullptr;
    result = dwarf_get_fde_info_for_all_regs3(fde, pc, &table, &ofs, &err);
    if (result != DW_DLV_OK)
        throw std::exception();

    MipsRegisters previousFrameRegs = {};
    Dwarf_Unsigned cfa = UnwindRegister(table.rt3_cfa_rule,
        0,
        0, // register 0 is used as pc since its always 0 anyway
        dw_data_alignment_factor,
        pMemoryOp,
        pRegisters);
    for (int i = 1; i < entries.size(); ++i)
    {
        previousFrameRegs.values[i] = static_cast<DWORD>(UnwindRegister(entries[i], i, cfa, dw_data_alignment_factor, pMemoryOp, pRegisters));
    }

    // pc is set at return address register value
    previousFrameRegs.values[0] = previousFrameRegs.values[31];
    *pRegisters = previousFrameRegs;
}

Dwarf_Unsigned ElfModule::UnwindRegister(
    Dwarf_Regtable_Entry3_s& entry,
    int registerIndex,
    Dwarf_Unsigned cfa,
    Dwarf_Signed alignmentFactory,
    IMemoryOperation* pMemoryOp,
    MipsRegisters* pRegisters)
{
    if (entry.dw_regnum == DW_FRAME_UNDEFINED_VAL)
        return 0;
    else if (entry.dw_regnum == DW_FRAME_SAME_VAL)
        return pRegisters->values[registerIndex];
    /*else if (entry.dw_regnum == DW_FRAME_CFA_COL)
    {

    }
    else
    {

    }*/

    switch (entry.dw_value_type)
    {
    case DW_EXPR_OFFSET:
        if (entry.dw_offset_relevant == 0)
        {
            // Not sure about this one
            if (entry.dw_regnum < 0 || entry.dw_regnum > 31)
                throw std::exception();

            return pRegisters->values[entry.dw_regnum];
        }
        else
        {
            int32_t offset = static_cast<int32_t>(entry.dw_offset);
            DWORD memoryOffset = 0;
            if (entry.dw_regnum == DW_FRAME_CFA_COL)
            {
                memoryOffset = static_cast<int32_t>(static_cast<int64_t>(cfa) + offset);

                DWORD value = 0;
                DWORD readCount = 0;
                HRESULT hr = pMemoryOp->Read(reinterpret_cast<BYTE*>(&value), memoryOffset, 4, &readCount);
                if (FAILED(hr) || readCount != 4)
                    throw std::exception();
                return value;
            }
            else
            {
                return static_cast<uint32_t>(static_cast<int64_t>(pRegisters->values[entry.dw_regnum]) + offset);
            }
        }
        break;
    case DW_EXPR_VAL_OFFSET:
        throw std::exception();
        break;
    case DW_EXPR_EXPRESSION:
        throw std::exception();
        break;
    case DW_EXPR_VAL_EXPRESSION:
        throw std::exception();
        break;
    default:
        throw std::exception();
    }
}
// Get die from dwarf offset
ElfDie* ElfModule::GetDieFromOffset(Dwarf_Off ofs)
{
    auto f = m_diesFromOfs.find(ofs);
    if (f == m_diesFromOfs.end())
        return nullptr;

    return f->second;
}