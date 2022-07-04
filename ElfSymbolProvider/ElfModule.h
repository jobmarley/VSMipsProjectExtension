#pragma once
#include "framework.h"
#include "ElfDie.h"

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

//class ElfSubprogram
//	: public ElfDie
//{
//	ElfSubprogram() = delete;
//	ElfSubprogram& operator=(const ElfSubprogram&) = delete;
//public:
//	ElfSubprogram(Dwarf_Debug dbg, Dwarf_Die die, ElfDie* parent)
//		: ElfDie(dbg, die, parent)
//	{
//
//	}
//};
//class ElfCompilationUnit
//	: public ElfDie
//{
//	ElfCompilationUnit() = delete;
//	ElfCompilationUnit& operator=(const ElfCompilationUnit&) = delete;
//public:
//	ElfCompilationUnit(Dwarf_Debug dbg, Dwarf_Die die)
//		: ElfDie(dbg, die, nullptr)
//	{
//	}
//
//	ElfSubprogram* GetSubprogram(DWORD address)
//	{
//
//	}
//};
class ElfModule
{
    //ElfModule() = delete;
    ElfModule& operator=(const ElfModule&) = delete;
    Dwarf_Debug m_dbg = nullptr;
    Dwarf_Unsigned m_lang = 0;
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

    cu_info* CUFromAddress(DWORD address);
    void LoadChildren(ElfDie* die);
public:
    ElfModule();
    ~ElfModule();

    void Load(IDebugModule2* pDebugModule, const char* filepath);

    HRESULT GetContextFromAddress(IDebugAddress* pAddress, IDebugDocumentContext2** ppDocContext);

    DWORD VirtualSize();

    HRESULT GetStackFrame(IDebugAddress* pAddress, IDebugThread2* pThread, IDebugStackFrame2** ppStackFrame);

    inline const char* GetFilepath() { return m_filepath.c_str(); }
    inline IDebugModule2* GetDebugModule() { return m_pDebugModule; }
};