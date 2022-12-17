#pragma once
#include "framework.h"
#include "Utils.h"
#include "ElfSymbolProvider_i.h"

class ElfModule;
class ElfAttribute;
struct MipsRegisters;

class ElfDie
{
    ElfDie() = delete;
    ElfDie& operator=(const ElfDie&) = delete;

    Dwarf_Debug m_dbg = nullptr;
    Dwarf_Die m_die = nullptr;
    Dwarf_Unsigned m_lang = 0;
    ElfDie* m_parent = nullptr;
    ElfModule* m_pModule = nullptr;

    std::vector<std::unique_ptr<ElfDie>> m_children;
public:
    ElfDie(Dwarf_Debug dbg, Dwarf_Die die, ElfModule* pModule, ElfDie* parent);
    virtual ~ElfDie();

    Dwarf_Half GetTag();
    const char* GetName();
    Dwarf_Unsigned GetLang();

    Dwarf_Addr GetLowPc();
    Dwarf_Addr GetHiPc();

    ElfDie* GetParent();
    inline std::vector<std::unique_ptr<ElfDie>>& GetChildrens() { return m_children; }
    inline Dwarf_Debug Dbg() { return m_dbg; }
    inline Dwarf_Die Die() { return m_die; }

    ElfDie* GetType();

    Dwarf_Off GetDwarfOfs();

    std::unique_ptr<ElfAttribute> GetAttribute(Dwarf_Half attributeNum);
};


// Calculate location expressions
class ElfLECalculator
{
    ElfLECalculator() = delete;
    ElfLECalculator& operator=(const ElfLECalculator&) = delete;

    Dwarf_Loc_Head_c m_head = nullptr;
    Dwarf_Unsigned m_count = 0;
    Dwarf_Debug m_dbg = nullptr;
    ATL::CComPtr<IMemoryOperation> m_pMemOp;
public:
    ElfLECalculator(Dwarf_Debug dbg, Dwarf_Attribute attr, IMemoryOperation* pMemOp);
    ~ElfLECalculator();

    DWORD Calculate(MipsRegisters& registers);
};

class ElfAttributeValue
{
    std::variant<std::string, int64_t, uint64_t> m_v;
public:
    ElfAttributeValue(std::string v)
        : m_v(v)
    {
    }
    ElfAttributeValue(int64_t v)
        : m_v(v)
    {
    }
    ElfAttributeValue(uint64_t v)
        : m_v(v)
    {
    }
    std::string AsString()
    {
        return std::get<std::string>(m_v);
    }
    int64_t AsInt64()
    {
        return std::get<int64_t>(m_v);
    }
    uint64_t AsUInt64()
    {
        return std::get<uint64_t>(m_v);
    }
};
class ElfAttribute
{
    ElfAttribute() = delete;
    ElfAttribute& operator=(const ElfAttribute&) = delete;

    Dwarf_Attribute m_attr = nullptr;
    Dwarf_Debug m_dbg = nullptr;
public:
    ElfAttribute(Dwarf_Debug dbg, Dwarf_Attribute attr);
    ~ElfAttribute();

    Dwarf_Attribute Attr();

    std::unique_ptr<ElfLECalculator> GetCalculator(IMemoryOperation* pMemOp);
    ElfAttributeValue GetValue();
};
