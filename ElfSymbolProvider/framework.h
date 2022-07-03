#pragma once

#ifndef STRICT
#define STRICT
#endif

#include "targetver.h"

#define _ATL_APARTMENT_THREADED

#define _ATL_NO_AUTOMATIC_NAMESPACE

#define _ATL_CSTRING_EXPLICIT_CONSTRUCTORS	// some CString constructors will be explicit


#define ATL_NO_ASSERT_ON_DESTROY_NONEXISTENT_WINDOW

#define NOMINMAX

#include "resource.h"
#include <atlbase.h>
#include <atlcom.h>
#include <atlctl.h>

#include <vector>
#include <string>
#include <memory>
#include <unordered_map>
#include <map>
#include <filesystem>
#include <set>
#include <string_view>
#include <mutex>
#include <fstream>

#include <msdbg.h> 
#include <dbgmetric.h> 
#include <sh.h>

#include "dwarf.h"
#include "libdwarf.h"


// allow hash with GUID
namespace std
{
    template<>
    struct hash<GUID>
    {
        std::size_t operator()(GUID g) const
        {
            return hash<std::string_view>()(std::string_view((const char*)&g, sizeof(g)));
        }
    };
}