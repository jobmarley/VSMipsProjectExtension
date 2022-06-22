#pragma once
#include "FPGADebugEngine_i.h"

// {01ADD4A1-2CD8-4B68-86B9-87E6737473B6}
DEFINE_GUID(IID_IFPGADebugEvent2,
	0x1add4a1, 0x2cd8, 0x4b68, 0x86, 0xb9, 0x87, 0xe6, 0x73, 0x74, 0x73, 0xb6);

MIDL_INTERFACE("01ADD4A1-2CD8-4B68-86B9-87E6737473B6")
IFPGADebugEvent2 : public IDebugEvent2
{
public:
	virtual HRESULT STDMETHODCALLTYPE SetAttributes(DWORD dwAttrib) = 0;
};