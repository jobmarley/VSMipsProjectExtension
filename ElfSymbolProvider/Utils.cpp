#include "pch.h"
#include "Utils.h"


void SafeThrowOnError(Dwarf_Debug dbg, Dwarf_Error err)
{
    if (err)
    {
        std::string msg = dwarf_errmsg(err);
        dwarf_dealloc_error(dbg, err);
        throw std::exception(msg.c_str());
    }
}