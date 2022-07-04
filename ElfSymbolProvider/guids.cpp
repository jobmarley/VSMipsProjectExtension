#include "pch.h"

#ifndef __IID_DEFINED__
#define __IID_DEFINED__

    typedef struct _IID
    {
        unsigned long x;
        unsigned short s1;
        unsigned short s2;
        unsigned char  c[8];
    } IID;

#endif // __IID_DEFINED__

#ifndef CLSID_DEFINED
#define CLSID_DEFINED
    typedef IID CLSID;
#endif // CLSID_DEFINED

#define MIDL_DEFINE_GUID(type,name,l,w1,w2,b1,b2,b3,b4,b5,b6,b7,b8) \
        EXTERN_C __declspec(selectany) const type name = {l,w1,w2,{b1,b2,b3,b4,b5,b6,b7,b8}}


    MIDL_DEFINE_GUID(IID, IID_IElfSymbolProvider, 0xc8b40372, 0x81f0, 0x45f5, 0x85, 0x32, 0x02, 0xf8, 0x44, 0x3d, 0xd2, 0xa6);
    MIDL_DEFINE_GUID(IID, IID_IElfDebugAddress, 0x7fe412c4, 0x4202, 0x4173, 0xa8, 0xfd, 0xd7, 0x4d, 0x20, 0x07, 0xdb, 0x5e);
    MIDL_DEFINE_GUID(IID, IID_IElfDebugDocumentContext, 0xa5df09cd, 0xee52, 0x44d7, 0xa7, 0xf, 0x97, 0x69, 0xad, 0x41, 0xd0, 0xa1);
    MIDL_DEFINE_GUID(IID, IID_IElfDebugCodeContext, 0x388d8dc0, 0xe693, 0x494e, 0xb7, 0xe1, 0xd7, 0x1e, 0x00, 0xb8, 0xb8, 0xdb);
    MIDL_DEFINE_GUID(IID, IID_IElfDebugStackFrame, 0xbbac5e6b, 0xeb45, 0x4690, 0x93, 0x70, 0xbe, 0xac, 0x1e, 0x2e, 0x6a, 0x11);

#undef MIDL_DEFINE_GUID




