#include "pch.h"
#include "ElfDie.h"
#include "ElfModule.h"
#include "ElfSymbolProvider_i.h"

ElfDie::ElfDie(Dwarf_Debug dbg, Dwarf_Die die, ElfModule* pModule, ElfDie* parent)
{
    m_dbg = dbg;
    m_die = die;
    m_pModule = pModule;
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
Dwarf_Addr ElfDie::GetHiPc()
{
    Dwarf_Error err = nullptr;
    Dwarf_Addr addr = 0;
    Dwarf_Half form;
    Dwarf_Form_Class cl;
    int result = dwarf_highpc_b(m_die, &addr, &form, &cl, &err);
    SafeThrowOnError(m_dbg, err);

	switch (cl)
	{
	case DW_FORM_CLASS_ADDRESS:
		return addr;
	case DW_FORM_CLASS_CONSTANT:
		return GetLowPc() + addr;
	default:
		throw std::exception("highpc format not supported");
	}
}
ElfDie* ElfDie::GetParent()
{
    return m_parent;
}
ElfDie* ElfDie::GetType()
{
    Dwarf_Off ofs = 0;
    Dwarf_Error err = nullptr;
    int result = dwarf_dietype_offset(m_die, &ofs, &err);
    SafeThrowOnError(m_dbg, err);
    if (result != DW_DLV_OK)
        throw std::exception();

    return m_pModule->GetDieFromOffset(ofs);
}
Dwarf_Off ElfDie::GetDwarfOfs()
{
    Dwarf_Off ofs = 0;
    Dwarf_Error err = nullptr;
    int result = dwarf_dieoffset(m_die, &ofs, &err);
    SafeThrowOnError(m_dbg, err);
    if (result != DW_DLV_OK)
        throw std::exception();
    return ofs;
}
std::unique_ptr<ElfAttribute> ElfDie::GetAttribute(Dwarf_Half attributeNum)
{
    Dwarf_Attribute attr = nullptr;
    Dwarf_Error err = nullptr;
    int result = dwarf_attr(m_die, attributeNum, &attr, &err);
    SafeThrowOnError(m_dbg, err);
    if (result != DW_DLV_OK)
        throw std::exception();

    return std::make_unique<ElfAttribute>(m_dbg, attr);
}
ElfAttribute::ElfAttribute(Dwarf_Debug dbg, Dwarf_Attribute attr)
{
    m_dbg = dbg;
    m_attr = attr;
}
ElfAttribute::~ElfAttribute()
{
    if (m_attr)
    {
        dwarf_dealloc_attribute(m_attr);
        m_attr = nullptr;
    }
}

Dwarf_Attribute ElfAttribute::Attr()
{
    return m_attr;
}


ElfLECalculator::ElfLECalculator(Dwarf_Debug dbg, Dwarf_Attribute attr, IMemoryOperation* pMemOp)
{
    m_dbg = dbg;

    Dwarf_Error err = nullptr;
    int result = dwarf_get_loclist_c(attr, &m_head, &m_count, &err);
    SafeThrowOnError(m_dbg, err);
    if (result != DW_DLV_OK)
        throw std::exception();

	m_pMemOp = pMemOp;
}
ElfLECalculator::~ElfLECalculator()
{
    if (m_head)
    {
        dwarf_dealloc_loc_head_c(m_head);
        m_head = nullptr;
    }
}

void ExecuteOperation(
    std::vector<uint64_t>& stack,
    MipsRegisters& registers,
    Dwarf_Small op,
    Dwarf_Unsigned operand1,
    Dwarf_Unsigned operand2,
    Dwarf_Unsigned operand3,
    Dwarf_Unsigned offset_for_branch,
	IMemoryOperation* memOp)
{
	HRESULT hr = S_OK;
    switch (op)
    {
		case DW_OP_addr:
			stack.push_back(operand1);
			break;
		case DW_OP_deref:
		{
			uint32_t val = 0;
			DWORD readCount = 0;
			hr = memOp->Read(reinterpret_cast<BYTE*>(&val), stack.back(), sizeof(val), &readCount);
			if (FAILED(hr) || readCount != sizeof(val))
				throw std::exception();
			stack.push_back(val);
			break;
		}
		case DW_OP_const1u:
			stack.push_back(operand1 & 0xFF);
			break;
		case DW_OP_const1s:
			stack.push_back(static_cast<int8_t>(operand1 & 0xFF));
			break;
		case DW_OP_const2u:
			stack.push_back(operand1 & 0xFFFF);
			break;
		case DW_OP_const2s:
			stack.push_back(static_cast<int16_t>(operand1 & 0xFFFF));
			break;
		case DW_OP_const4u:
			stack.push_back(operand1 & 0xFFFFFFFF);
			break;
		case DW_OP_const4s:
			stack.push_back(static_cast<int32_t>(operand1 & 0xFFFFFFFF));
			break;
		case DW_OP_const8u:
			stack.push_back(operand1);
			break;
		case DW_OP_const8s:
			stack.push_back(operand1);
			break;
		case DW_OP_constu:
			///stack.push(operand1);
			throw std::exception();
			break;
		case DW_OP_consts:
			//stack.push(operand1);
			throw std::exception();
			break;
		case DW_OP_dup:
			stack.push_back(stack.back());
			break;
		case DW_OP_drop:
			stack.pop_back();
			break;
		case DW_OP_over:
			stack.push_back(stack[stack.size() - 2]);
			break;
		case DW_OP_pick:
			stack.push_back(stack[operand1 & 0xFF]);
			break;
		case DW_OP_swap:
			std::swap(stack.back(), stack[stack.size() - 2]);
			break;
		case DW_OP_rot:
			std::swap(stack.back(), stack[stack.size() - 2]);
			std::swap(stack[stack.size() - 2], stack[stack.size() - 3]);
			break;
		case DW_OP_xderef:
			throw std::exception("not supported");
			break;
		case DW_OP_abs:
			stack.back() = static_cast<uint64_t>(std::abs(static_cast<int64_t>(stack.back())));
			break;
		case DW_OP_and:
		{
			uint64_t val1 = stack.back();
			stack.pop_back();
			stack.back() = val1 & stack.back();
			break;
		}
		case DW_OP_div:
		{
			uint64_t val1 = stack.back();
			stack.pop_back();
			stack.back() = static_cast<uint64_t>(static_cast<int64_t>(stack.back()) / static_cast<int64_t>(val1));
			break;
		}
		case DW_OP_minus:
		{
			uint64_t val1 = stack.back();
			stack.pop_back();
			stack.back() = stack.back() - val1;
			break;
		}
		case DW_OP_mod:
		{
			uint64_t val1 = stack.back();
			stack.pop_back();
			stack.back() = stack.back() % val1;
			break;
		}
		case DW_OP_mul:
		{
			uint64_t val1 = stack.back();
			stack.pop_back();
			stack.back() = stack.back() * val1;
			break;
		}
		case DW_OP_neg:
			stack.back() = static_cast<uint64_t>(-static_cast<int64_t>(stack.back()));
			break;
		case DW_OP_not:
			stack.back() = ~stack.back();
			break;
		case DW_OP_or:
		{
			uint64_t val1 = stack.back();
			stack.pop_back();
			stack.back() = stack.back() | val1;
			break;
		}
		case DW_OP_plus:
		{
			uint64_t val1 = stack.back();
			stack.pop_back();
			stack.back() = stack.back() + val1;
			break;
		}
		case DW_OP_plus_uconst:
		{
			stack.back() = stack.back() + operand1;
			break;
		}
		case DW_OP_shl:
		{
			uint64_t val1 = stack.back();
			stack.pop_back();
			stack.back() = stack.back() << val1;
			break;
		}
		case DW_OP_shr:
		{
			uint64_t val1 = stack.back();
			stack.pop_back();
			stack.back() = stack.back() >> val1;
			break;
		}
		case DW_OP_shra:
		{
			uint64_t val1 = stack.back();
			stack.pop_back();
			uint64_t val2 = stack.back();
			stack.back() = val2 >> val1;
			stack.back() |= val2 << (sizeof(val2) - val1);
			break;
		}
		case DW_OP_xor:
		{
			uint64_t val1 = stack.back();
			stack.pop_back();
			stack.back() ^= val1;
			break;
		}
		case DW_OP_bra:
			throw std::exception("not implemented");
			break;
		case DW_OP_eq:
		{
			uint64_t val1 = stack.back();
			stack.pop_back();
			stack.back() = (stack.back() == val1) ? 1 : 0;
			break;
		}
		case DW_OP_ge:
		{
			uint64_t val1 = stack.back();
			stack.pop_back();
			stack.back() = (stack.back() >= val1) ? 1 : 0;
			break;
		}
		case DW_OP_gt:
		{
			uint64_t val1 = stack.back();
			stack.pop_back();
			stack.back() = (stack.back() > val1) ? 1 : 0;
			break;
		}
		case DW_OP_le:
		{
			uint64_t val1 = stack.back();
			stack.pop_back();
			stack.back() = (stack.back() <= val1) ? 1 : 0;
			break;
		}
		case DW_OP_lt:
		{
			uint64_t val1 = stack.back();
			stack.pop_back();
			stack.back() = (stack.back() < val1) ? 1 : 0;
			break;
		}
		case DW_OP_ne:
		{
			uint64_t val1 = stack.back();
			stack.pop_back();
			stack.back() = (stack.back() != val1) ? 1 : 0;
			break;
		}
		case DW_OP_skip:
			break;
		case DW_OP_lit0:
		case DW_OP_lit1:
		case DW_OP_lit2:
		case DW_OP_lit3:
		case DW_OP_lit4:
		case DW_OP_lit5:
		case DW_OP_lit6:
		case DW_OP_lit7:
		case DW_OP_lit8:
		case DW_OP_lit9:
		case DW_OP_lit10:
		case DW_OP_lit11:
		case DW_OP_lit12:
		case DW_OP_lit13:
		case DW_OP_lit14:
		case DW_OP_lit15:
		case DW_OP_lit16:
		case DW_OP_lit17:
		case DW_OP_lit18:
		case DW_OP_lit19:
		case DW_OP_lit20:
		case DW_OP_lit21:
		case DW_OP_lit22:
		case DW_OP_lit23:
		case DW_OP_lit24:
		case DW_OP_lit25:
		case DW_OP_lit26:
		case DW_OP_lit27:
		case DW_OP_lit28:
		case DW_OP_lit29:
		case DW_OP_lit30:
		case DW_OP_lit31:
			stack.push_back(op - DW_OP_lit0);
			break;

		// NOTE: It is said register appears alone and the register contains the value
		// Thus it should not cause issues to push on the stack if the dwarf info is well formed
		case DW_OP_reg0:
			stack.push_back(0); // we use register 0 as PC because its hardwired 0
			break;
		case DW_OP_reg1:
		case DW_OP_reg2:
		case DW_OP_reg3:
		case DW_OP_reg4:
		case DW_OP_reg5:
		case DW_OP_reg6:
		case DW_OP_reg7:
		case DW_OP_reg8:
		case DW_OP_reg9:
		case DW_OP_reg10:
		case DW_OP_reg11:
		case DW_OP_reg12:
		case DW_OP_reg13:
		case DW_OP_reg14:
		case DW_OP_reg15:
		case DW_OP_reg16:
		case DW_OP_reg17:
		case DW_OP_reg18:
		case DW_OP_reg19:
		case DW_OP_reg20:
		case DW_OP_reg21:
		case DW_OP_reg22:
		case DW_OP_reg23:
		case DW_OP_reg24:
		case DW_OP_reg25:
		case DW_OP_reg26:
		case DW_OP_reg27:
		case DW_OP_reg28:
		case DW_OP_reg29:
		case DW_OP_reg30:
		case DW_OP_reg31:
			stack.push_back(registers.values[op - DW_OP_reg0]);
			break;
		case DW_OP_breg0:
			stack.push_back(static_cast<int64_t>(operand1));
			break;
		case DW_OP_breg1:
		case DW_OP_breg2:
		case DW_OP_breg3:
		case DW_OP_breg4:
		case DW_OP_breg5:
		case DW_OP_breg6:
		case DW_OP_breg7:
		case DW_OP_breg8:
		case DW_OP_breg9:
		case DW_OP_breg10:
		case DW_OP_breg11:
		case DW_OP_breg12:
		case DW_OP_breg13:
		case DW_OP_breg14:
		case DW_OP_breg15:
		case DW_OP_breg16:
		case DW_OP_breg17:
		case DW_OP_breg18:
		case DW_OP_breg19:
		case DW_OP_breg20:
		case DW_OP_breg21:
		case DW_OP_breg22:
		case DW_OP_breg23:
		case DW_OP_breg24:
		case DW_OP_breg25:
		case DW_OP_breg26:
		case DW_OP_breg27:
		case DW_OP_breg28:
		case DW_OP_breg29:
		case DW_OP_breg30:
		case DW_OP_breg31:
			stack.push_back(registers.values[op - DW_OP_breg0] + static_cast<int64_t>(operand1));
			break;
		case DW_OP_regx:
			if (operand1 < 0 || operand1 > 31)
				throw std::exception();
			stack.push_back((operand1 == 0) ? 0 : registers.values[operand1]);
			break;
		case DW_OP_fbreg:
			throw std::exception("not implemented");
			break;
		case DW_OP_bregx:
			if (operand2 < 0 || operand2 > 31)
				throw std::exception();
			stack.push_back(((operand2 == 0) ? 0 : registers.values[operand2]) + static_cast<int64_t>(operand1));
			break;
		case DW_OP_piece:
			throw std::exception("not implemented");
			break;
		case DW_OP_deref_size:
		{
			uint32_t val = 0;
			DWORD readCount = 0;
			if (operand1 > 4)
				throw std::exception();
			hr = memOp->Read(reinterpret_cast<BYTE*>(&val), stack.back(), sizeof(operand1), &readCount);
			if (FAILED(hr) || readCount != operand1)
				throw std::exception();
			stack.push_back(val);
			break;
		}
		case DW_OP_xderef_size:
			throw std::exception("not supported");
			break;
		case DW_OP_nop:
			break;
		case DW_OP_push_object_address:
			throw std::exception("not implemented");
			break;
		case DW_OP_call2:
			throw std::exception("not implemented");
			break;
		case DW_OP_call4:
			throw std::exception("not implemented");
			break;
		case DW_OP_call_ref:
			throw std::exception("not implemented");
			break;
		case DW_OP_form_tls_address:
			throw std::exception("not implemented");
			break;
		case DW_OP_call_frame_cfa:
			throw std::exception("not implemented");
			break;
		case DW_OP_bit_piece:
			throw std::exception("not implemented");
			break;
		case DW_OP_implicit_value:
			throw std::exception("not implemented");
			break;
		case DW_OP_stack_value:
			throw std::exception("not implemented");
			break;
		case DW_OP_implicit_pointer:
			throw std::exception("not implemented");
			break;
		case DW_OP_addrx:
			throw std::exception("not implemented");
			break;
		case DW_OP_constx:
			throw std::exception("not implemented");
			break;
		case DW_OP_entry_value:
			throw std::exception("not implemented");
			break;
		case DW_OP_const_type:
			throw std::exception("not implemented");
			break;
		case DW_OP_regval_type:
			throw std::exception("not implemented");
			break;
		case DW_OP_deref_type:
			throw std::exception("not implemented");
			break;
		case DW_OP_xderef_type:
			throw std::exception("not implemented");
			break;
		case DW_OP_convert:
			throw std::exception("not implemented");
			break;
		case DW_OP_reinterpret:
			throw std::exception("not implemented");
			break;
		default:
			throw std::exception("unknown operator");
    }
}
DWORD ElfLECalculator::Calculate(MipsRegisters& registers)
{
    if (m_count == 0)
        throw std::exception(); // present in source code but not in debug info because of opti maybe
    
	DWORD address = 0;
    for (Dwarf_Unsigned i = 0; i < m_count; ++i)
    {
        Dwarf_Error err = nullptr;
        Dwarf_Small dw_lle_value_out;
        Dwarf_Unsigned dw_rawlowpc;
        Dwarf_Unsigned dw_rawhipc;
        Dwarf_Bool dw_debug_addr_unavailable;
        Dwarf_Addr dw_lowpc_cooked;
        Dwarf_Addr dw_hipc_cooked;
        Dwarf_Unsigned dw_locexpr_op_count_out;
        Dwarf_Locdesc_c dw_locentry_out;
        Dwarf_Small dw_loclist_source_out;
        Dwarf_Unsigned dw_expression_offset_out;
        Dwarf_Unsigned dw_locdesc_offset_out;
        int result = dwarf_get_locdesc_entry_d(m_head, i,
            &dw_lle_value_out,
            &dw_rawlowpc,
            &dw_rawhipc,
            &dw_debug_addr_unavailable,
            &dw_lowpc_cooked,
            &dw_hipc_cooked,
            &dw_locexpr_op_count_out,
            &dw_locentry_out,
            &dw_loclist_source_out,
            &dw_expression_offset_out,
            &dw_locdesc_offset_out,
            &err);
        SafeThrowOnError(m_dbg, err);

        std::vector<uint64_t> stack;
        for (Dwarf_Unsigned j = 0; j < dw_locexpr_op_count_out; ++j)
        {
            Dwarf_Small dw_operator_out;
            Dwarf_Unsigned dw_operand1;
            Dwarf_Unsigned dw_operand2;
            Dwarf_Unsigned dw_operand3;
            Dwarf_Unsigned dw_offset_for_branch;
            result = dwarf_get_location_op_value_c(dw_locentry_out,
                j,
                &dw_operator_out,
                &dw_operand1,
                &dw_operand2,
                &dw_operand3,
                &dw_offset_for_branch,
                &err);
            SafeThrowOnError(m_dbg, err);
			ExecuteOperation(stack,
				registers,
				dw_operator_out,
				dw_operand1,
				dw_operand2,
				dw_operand3,
				dw_offset_for_branch,
				m_pMemOp);
        }
		address = stack.back();
		break;
    }

	DWORD value = 0;
	DWORD readCount = 0;
	HRESULT hr = m_pMemOp->Read(reinterpret_cast<BYTE*>(&value), address, sizeof(value), &readCount);
	if (FAILED(hr) || readCount != sizeof(value))
		throw std::exception();
	return value;
}
std::unique_ptr<ElfLECalculator> ElfAttribute::GetCalculator(IMemoryOperation* pMemOp)
{
	return std::make_unique<ElfLECalculator>(m_dbg, m_attr, pMemOp);
}