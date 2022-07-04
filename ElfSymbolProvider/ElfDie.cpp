#include "pch.h"
#include "ElfDie.h"

ElfDie::ElfDie(Dwarf_Debug dbg, Dwarf_Die die, ElfDie* parent)
{
    m_dbg = dbg;
    m_die = die;
    m_parent = parent;
    Dwarf_Error err = nullptr;
    int result = dwarf_srclang(m_die, &m_lang, &err);
    SafeThrowOnError(m_dbg, err);
}
ElfDie::~ElfDie()
{
    if (m_die)
    {
        dwarf_dealloc_die(m_die);
    }
}
Dwarf_Half ElfDie::GetTag()
{
    Dwarf_Half tag = 0;
    Dwarf_Error err = nullptr;
    int result = dwarf_tag(m_die, &tag, &err);
    SafeThrowOnError(m_dbg, err);

    return tag;
}

const char* ElfDie::GetName()
{
    char* name = nullptr;
    Dwarf_Error err = nullptr;
    int result = dwarf_diename(m_die, &name, &err);
    SafeThrowOnError(m_dbg, err);
    return name;
}
Dwarf_Unsigned ElfDie::GetLang()
{
    return m_lang;
}
Dwarf_Addr ElfDie::GetLowPc()
{
    Dwarf_Error err = nullptr;
    Dwarf_Addr addr = 0;
    int result = dwarf_lowpc(m_die, &addr, &err);
    SafeThrowOnError(m_dbg, err);
    return addr;
}
ElfDie* ElfDie::GetParent()
{
    return m_parent;
}