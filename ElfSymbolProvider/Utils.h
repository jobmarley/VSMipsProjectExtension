#pragma once
#include "framework.h"

void SafeThrowOnError(Dwarf_Debug dbg, Dwarf_Error err);
uint32_t GetAddressValue(IDebugAddress* pAddress);