#pragma once
#include "framework.h"
#include "ElfDie.h"
#include "ElfSymbolProvider_i.h"

using namespace ATL;

class ElfLineTable
{
    // ElfLineTable() = delete;
    ElfLineTable& operator=(const ElfLineTable&) = delete;
    Dwarf_Debug m_dbg = nullptr;
    Dwarf_Line_Context m_context = nullptr;
    Dwarf_Line* m_lines = nullptr;
    Dwarf_Signed m_lineCount = 0;
public:
    ElfLineTable(Dwarf_Debug dbg, Dwarf_Die die)
    {
        m_dbg = dbg;

        Dwarf_Error err = nullptr;
        Dwarf_Unsigned version = 0;
        Dwarf_Small table_count = 0;
        int result = dwarf_srclines_b(die, &version, &table_count, &m_context, &err);
        SafeThrowOnError(m_dbg, err);

        result = dwarf_srclines_from_linecontext(m_context, &m_lines, &m_lineCount, &err);
        SafeThrowOnError(m_dbg, err);
    }
    ~ElfLineTable()
    {
        if (m_context)
            dwarf_srclines_dealloc_b(m_context);
    }
    // Return the line corresponding to that address, or nullptr if not found
    Dwarf_Line LineFromAddress(DWORD address)
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
};

struct MipsRegisters
{
    DWORD values[32];
};

struct IElfSymbolProvider;

class ElfModule
{
    //ElfModule() = delete;
    ElfModule& operator=(const ElfModule&) = delete;
    Dwarf_Debug m_dbg = nullptr;
    uint32_t m_virtualSize = 0;

    struct cu_info
    {
        std::unique_ptr<ElfDie> die;
        std::unique_ptr<ElfLineTable> lineTable;
    };
    std::map<DWORD, cu_info> m_compilationUnits;
    std::map<DWORD, ElfDie*> m_subprograms;
    std::string m_filepath;
    CComPtr<IDebugModule2> m_pDebugModule;
    IElfSymbolProvider* m_pSymbolProvider = nullptr;
    std::unordered_map<Dwarf_Off, ElfDie*> m_diesFromOfs; // from dwarf offset

    Dwarf_Cie* m_cieTable = nullptr;
    Dwarf_Signed m_cieCount = 0;
    Dwarf_Fde* m_fdeTable = nullptr;
    Dwarf_Signed m_fdeCount = 0;

    cu_info* CUFromAddress(DWORD address);
    void LoadChildren(ElfDie* die);
    void LoadFrames();
    Dwarf_Fde FdeFromAddress(DWORD address);
    void GetRegisters(DWORD address);
public:
    ElfModule(IElfSymbolProvider* pSymbolProvider);
    ~ElfModule();

    void Load(IDebugModule2* pDebugModule, const char* filepath);

    HRESULT GetContextFromAddress(IDebugAddress* pAddress, IDebugDocumentContext2** ppDocContext);

    DWORD VirtualSize();

    HRESULT GetStackFrame(IDebugAddress* pAddress,
        IDebugThread2* pThread,
        IMemoryOperation* pMemoryOp,
        IRegisterOperation* pRegisterOp,
        IDebugStackFrame2** ppStackFrame);

    inline const char* GetFilepath() { return m_filepath.c_str(); }
    inline IDebugModule2* GetDebugModule() { return m_pDebugModule; }

    ElfDie* GetFunction(IDebugAddress* pAddress);
    void UnwindRegisters(MipsRegisters* pRegisters, IMemoryOperation* pMemoryOp);
    Dwarf_Unsigned UnwindRegister(Dwarf_Regtable_Entry3_s& entry,
        int registerIndex,
        Dwarf_Unsigned cfa,
        Dwarf_Signed alignmentFactory,
        IMemoryOperation* pMemoryOp,
        MipsRegisters* pRegisters);

    // Get die from dwarf offset
    ElfDie* GetDieFromOffset(Dwarf_Off ofs);
};