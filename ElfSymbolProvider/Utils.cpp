/*
 Copyright (C) 2022 jobmarley

 This file is part of VSMipsProjectExtension.

 This program is free software; you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation; either version 3 of the License, or (at your option) any later version.

 This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

 You should have received a copy of the GNU General Public License along with Foobar. If not, see <https://www.gnu.org/licenses/>.
 */

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