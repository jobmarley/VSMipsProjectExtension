#pragma once
#include "framework.h"
#include "Utils.h"

class ElfDie
{
    ElfDie() = delete;
    ElfDie& operator=(const ElfDie&) = delete;

    Dwarf_Debug m_dbg = nullptr;
    Dwarf_Die m_die = nullptr;
    Dwarf_Unsigned m_lang = 0;
public:
    ElfDie(Dwarf_Debug dbg, Dwarf_Die die);
    ~ElfDie();

    Dwarf_Half GetTag();
    const char* GetName();
    Dwarf_Unsigned GetLang();
};