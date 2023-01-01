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

    Dwarf_Addr GetLineAddress(Dwarf_Line l)
    {
        Dwarf_Addr addr = 0;
        Dwarf_Error err = nullptr;
        int result = dwarf_lineaddr(l, &addr, &err);
        return addr;
    }
    Dwarf_Unsigned GetLineNumber(Dwarf_Line l)
    {
        Dwarf_Unsigned no = 0;
        Dwarf_Error err = nullptr;
        int result = dwarf_lineno(l, &no, &err);
        return no;
    }
    Dwarf_Bool IsBeginStatement(Dwarf_Line l)
    {
        Dwarf_Bool b = 0;
        Dwarf_Error err = nullptr;
        int result = dwarf_linebeginstatement(l, &b, &err);
        return b;
    }
    Dwarf_Bool IsEndSequence(Dwarf_Line l)
    {
        Dwarf_Bool b = 0;
        Dwarf_Error err = nullptr;
        int result = dwarf_lineendsequence(l, &b, &err);
        return b;
    }
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
    // Return the line corresponding to that address, or the line just before if no exact match. nullptr otherwise
    Dwarf_Line LineFromAddress(DWORD address)
    {
        Dwarf_Line* start = &m_lines[0];
        Dwarf_Line* end = &m_lines[m_lineCount];
        Dwarf_Line* found = std::upper_bound(start, end, address, [this](DWORD v, Dwarf_Line l) { return v < GetLineAddress(l); });

        // This gets the first bigger than, so if 0, miss
        if (found == start)
            return nullptr;

        --found;
        return *found;
    }

    std::vector<std::pair<DWORD, DWORD>> GetAddressesFromLine(DWORD line)
    {
        std::vector<std::pair<DWORD, DWORD>> result;
        int i = 0;
        while (i < m_lineCount)
        {
            Dwarf_Unsigned iStart = GetLineNumber(m_lines[i]);
            Dwarf_Addr addrStart = GetLineAddress(m_lines[i]);
            if (iStart == line && IsBeginStatement(m_lines[i]))
            {
                Dwarf_Unsigned iEnd = iStart;
                Dwarf_Addr addrEnd = addrStart;
                ++i;
                for (; i < m_lineCount && !IsEndSequence(m_lines[i]) && !IsBeginStatement(m_lines[i]); ++i)
                {
                    Dwarf_Unsigned iLine = GetLineNumber(m_lines[i]);
                    Dwarf_Addr addr = GetLineAddress(m_lines[i]);
                    if (iLine != iStart)
                        break;
                    iEnd = iLine;
                    addrEnd = addr;
                }
                result.push_back({ addrStart, addrEnd });
            }
            else
            {
                ++i;
            }
        }

        return result;
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

    bool HasSourceFile(std::string filepath);
    std::vector<std::pair<DWORD, DWORD>> GetAddressesFromLine(std::string filepath, DWORD line);
    cu_info* CUFromAddress(DWORD address);
};