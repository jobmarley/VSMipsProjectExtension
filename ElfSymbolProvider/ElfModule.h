#pragma once
#include "framework.h"
#include "ElfDie.h"

class ElfModule
{
    //ElfModule() = delete;
    ElfModule& operator=(const ElfModule&) = delete;
    Dwarf_Debug m_dbg = nullptr;
    Dwarf_Line_Context m_lineContext = nullptr;
    Dwarf_Line* m_lines = nullptr;
    Dwarf_Signed m_lineCount = 0;
    std::unique_ptr<ElfDie> m_cuDie;
    Dwarf_Unsigned m_lang = 0;
    uint32_t m_virtualSize = 0;
public:
    ElfModule();
    ~ElfModule();

    void Load(const char* filepath);

    // Return the line corresponding to that address, or nullptr if not found
    Dwarf_Line LineFromAddress(DWORD address);

    HRESULT GetContextFromAddress(DEBUG_ADDRESS* pAddress, IDebugDocumentContext2** ppDocContext);

    DWORD VirtualSize();
};