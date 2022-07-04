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
    ElfDie* m_parent = nullptr;

    std::vector<std::unique_ptr<ElfDie>> m_children;
public:
    ElfDie(Dwarf_Debug dbg, Dwarf_Die die, ElfDie* parent);
    virtual ~ElfDie();

    Dwarf_Half GetTag();
    const char* GetName();
    Dwarf_Unsigned GetLang();

    Dwarf_Addr GetLowPc();

    ElfDie* GetParent();
    inline std::vector<std::unique_ptr<ElfDie>>& GetChildrens() { return m_children; }
    inline Dwarf_Debug Dbg() { return m_dbg; }
    inline Dwarf_Die Die() { return m_die; }
};