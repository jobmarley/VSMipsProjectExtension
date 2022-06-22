

/* this ALWAYS GENERATED file contains the definitions for the interfaces */


 /* File created by MIDL compiler version 8.01.0628 */
/* at Tue Jan 19 04:14:07 2038
 */
/* Compiler settings for FPGADebugEngine.idl:
    Oicf, W1, Zp8, env=Win64 (32b run), target_arch=AMD64 8.01.0628 
    protocol : all , ms_ext, c_ext, robust
    error checks: allocation ref bounds_check enum stub_data 
    VC __declspec() decoration level: 
         __declspec(uuid()), __declspec(selectany), __declspec(novtable)
         DECLSPEC_UUID(), MIDL_INTERFACE()
*/
/* @@MIDL_FILE_HEADING(  ) */



/* verify that the <rpcndr.h> version is high enough to compile this file*/
#ifndef __REQUIRED_RPCNDR_H_VERSION__
#define __REQUIRED_RPCNDR_H_VERSION__ 500
#endif

#include "rpc.h"
#include "rpcndr.h"

#ifndef __RPCNDR_H_VERSION__
#error this stub requires an updated version of <rpcndr.h>
#endif /* __RPCNDR_H_VERSION__ */


#ifndef __FPGADebugEngine_i_h__
#define __FPGADebugEngine_i_h__

#if defined(_MSC_VER) && (_MSC_VER >= 1020)
#pragma once
#endif

#ifndef DECLSPEC_XFGVIRT
#if defined(_CONTROL_FLOW_GUARD_XFG)
#define DECLSPEC_XFGVIRT(base, func) __declspec(xfg_virtual(base, func))
#else
#define DECLSPEC_XFGVIRT(base, func)
#endif
#endif

/* Forward Declarations */ 

#ifndef __Engine_FWD_DEFINED__
#define __Engine_FWD_DEFINED__

#ifdef __cplusplus
typedef class Engine Engine;
#else
typedef struct Engine Engine;
#endif /* __cplusplus */

#endif 	/* __Engine_FWD_DEFINED__ */


#ifndef __FPGADebugProcess_FWD_DEFINED__
#define __FPGADebugProcess_FWD_DEFINED__

#ifdef __cplusplus
typedef class FPGADebugProcess FPGADebugProcess;
#else
typedef struct FPGADebugProcess FPGADebugProcess;
#endif /* __cplusplus */

#endif 	/* __FPGADebugProcess_FWD_DEFINED__ */


#ifndef __FPGADebugProgram_FWD_DEFINED__
#define __FPGADebugProgram_FWD_DEFINED__

#ifdef __cplusplus
typedef class FPGADebugProgram FPGADebugProgram;
#else
typedef struct FPGADebugProgram FPGADebugProgram;
#endif /* __cplusplus */

#endif 	/* __FPGADebugProgram_FWD_DEFINED__ */


#ifndef __FPGADebugProgramEnumerator_FWD_DEFINED__
#define __FPGADebugProgramEnumerator_FWD_DEFINED__

#ifdef __cplusplus
typedef class FPGADebugProgramEnumerator FPGADebugProgramEnumerator;
#else
typedef struct FPGADebugProgramEnumerator FPGADebugProgramEnumerator;
#endif /* __cplusplus */

#endif 	/* __FPGADebugProgramEnumerator_FWD_DEFINED__ */


#ifndef __FPGADebugProcessCreateEvent2_FWD_DEFINED__
#define __FPGADebugProcessCreateEvent2_FWD_DEFINED__

#ifdef __cplusplus
typedef class FPGADebugProcessCreateEvent2 FPGADebugProcessCreateEvent2;
#else
typedef struct FPGADebugProcessCreateEvent2 FPGADebugProcessCreateEvent2;
#endif /* __cplusplus */

#endif 	/* __FPGADebugProcessCreateEvent2_FWD_DEFINED__ */


#ifndef __FPGADebugThread2_FWD_DEFINED__
#define __FPGADebugThread2_FWD_DEFINED__

#ifdef __cplusplus
typedef class FPGADebugThread2 FPGADebugThread2;
#else
typedef struct FPGADebugThread2 FPGADebugThread2;
#endif /* __cplusplus */

#endif 	/* __FPGADebugThread2_FWD_DEFINED__ */


#ifndef __FPGADebugProgramCreateEvent2_FWD_DEFINED__
#define __FPGADebugProgramCreateEvent2_FWD_DEFINED__

#ifdef __cplusplus
typedef class FPGADebugProgramCreateEvent2 FPGADebugProgramCreateEvent2;
#else
typedef struct FPGADebugProgramCreateEvent2 FPGADebugProgramCreateEvent2;
#endif /* __cplusplus */

#endif 	/* __FPGADebugProgramCreateEvent2_FWD_DEFINED__ */


#ifndef __FPGADebugPortSupplier2_FWD_DEFINED__
#define __FPGADebugPortSupplier2_FWD_DEFINED__

#ifdef __cplusplus
typedef class FPGADebugPortSupplier2 FPGADebugPortSupplier2;
#else
typedef struct FPGADebugPortSupplier2 FPGADebugPortSupplier2;
#endif /* __cplusplus */

#endif 	/* __FPGADebugPortSupplier2_FWD_DEFINED__ */


#ifndef __FPGADebugProgramProvider2_FWD_DEFINED__
#define __FPGADebugProgramProvider2_FWD_DEFINED__

#ifdef __cplusplus
typedef class FPGADebugProgramProvider2 FPGADebugProgramProvider2;
#else
typedef struct FPGADebugProgramProvider2 FPGADebugProgramProvider2;
#endif /* __cplusplus */

#endif 	/* __FPGADebugProgramProvider2_FWD_DEFINED__ */


#ifndef __FPGADebugPort2_FWD_DEFINED__
#define __FPGADebugPort2_FWD_DEFINED__

#ifdef __cplusplus
typedef class FPGADebugPort2 FPGADebugPort2;
#else
typedef struct FPGADebugPort2 FPGADebugPort2;
#endif /* __cplusplus */

#endif 	/* __FPGADebugPort2_FWD_DEFINED__ */


/* header files for imported files */
#include "oaidl.h"
#include "ocidl.h"
#include "msdbg.h"
#include "shobjidl.h"

#ifdef __cplusplus
extern "C"{
#endif 



#ifndef __FPGADebugEngineLib_LIBRARY_DEFINED__
#define __FPGADebugEngineLib_LIBRARY_DEFINED__

/* library FPGADebugEngineLib */
/* [version][uuid] */ 


EXTERN_C const IID LIBID_FPGADebugEngineLib;

EXTERN_C const CLSID CLSID_Engine;

#ifdef __cplusplus

class DECLSPEC_UUID("E13B43AC-F1B2-4636-A20A-364ACB053A04")
Engine;
#endif

EXTERN_C const CLSID CLSID_FPGADebugProcess;

#ifdef __cplusplus

class DECLSPEC_UUID("1a8f209c-1993-40ef-8a32-26c0fbd415b0")
FPGADebugProcess;
#endif

EXTERN_C const CLSID CLSID_FPGADebugProgram;

#ifdef __cplusplus

class DECLSPEC_UUID("ca8b28d5-faaa-47e8-a8eb-0049fe3d0b18")
FPGADebugProgram;
#endif

EXTERN_C const CLSID CLSID_FPGADebugProgramEnumerator;

#ifdef __cplusplus

class DECLSPEC_UUID("6ee3fdcc-226f-4daf-ba00-cfacb76d7393")
FPGADebugProgramEnumerator;
#endif

EXTERN_C const CLSID CLSID_FPGADebugProcessCreateEvent2;

#ifdef __cplusplus

class DECLSPEC_UUID("cf63b309-ce44-48d0-adff-4d1fb50f6e26")
FPGADebugProcessCreateEvent2;
#endif

EXTERN_C const CLSID CLSID_FPGADebugThread2;

#ifdef __cplusplus

class DECLSPEC_UUID("099af354-8930-49eb-a95d-72792d509658")
FPGADebugThread2;
#endif

EXTERN_C const CLSID CLSID_FPGADebugProgramCreateEvent2;

#ifdef __cplusplus

class DECLSPEC_UUID("1f4f6d7b-e4f0-40b8-b3fc-8a6eb00e92b3")
FPGADebugProgramCreateEvent2;
#endif

EXTERN_C const CLSID CLSID_FPGADebugPortSupplier2;

#ifdef __cplusplus

class DECLSPEC_UUID("2572f753-9309-4246-8c65-58bd4d427ce2")
FPGADebugPortSupplier2;
#endif

EXTERN_C const CLSID CLSID_FPGADebugProgramProvider2;

#ifdef __cplusplus

class DECLSPEC_UUID("fe85e1f0-a6cd-4e87-9ab1-db4ad77b94cd")
FPGADebugProgramProvider2;
#endif

EXTERN_C const CLSID CLSID_FPGADebugPort2;

#ifdef __cplusplus

class DECLSPEC_UUID("968191bd-5c47-4474-bff6-d613b2c353bd")
FPGADebugPort2;
#endif
#endif /* __FPGADebugEngineLib_LIBRARY_DEFINED__ */

/* Additional Prototypes for ALL interfaces */

/* end of Additional Prototypes */

#ifdef __cplusplus
}
#endif

#endif


