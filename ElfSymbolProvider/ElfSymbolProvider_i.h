

/* this ALWAYS GENERATED file contains the definitions for the interfaces */


 /* File created by MIDL compiler version 8.01.0628 */
/* at Tue Jan 19 04:14:07 2038
 */
/* Compiler settings for ElfSymbolProvider.idl:
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

#ifndef COM_NO_WINDOWS_H
#include "windows.h"
#include "ole2.h"
#endif /*COM_NO_WINDOWS_H*/

#ifndef __ElfSymbolProvider_i_h__
#define __ElfSymbolProvider_i_h__

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

#ifndef __IElfSymbolProvider_FWD_DEFINED__
#define __IElfSymbolProvider_FWD_DEFINED__
typedef interface IElfSymbolProvider IElfSymbolProvider;

#endif 	/* __IElfSymbolProvider_FWD_DEFINED__ */


#ifndef __ElfSymbolProvider_FWD_DEFINED__
#define __ElfSymbolProvider_FWD_DEFINED__

#ifdef __cplusplus
typedef class ElfSymbolProvider ElfSymbolProvider;
#else
typedef struct ElfSymbolProvider ElfSymbolProvider;
#endif /* __cplusplus */

#endif 	/* __ElfSymbolProvider_FWD_DEFINED__ */


#ifndef __ElfDebugAddress_FWD_DEFINED__
#define __ElfDebugAddress_FWD_DEFINED__

#ifdef __cplusplus
typedef class ElfDebugAddress ElfDebugAddress;
#else
typedef struct ElfDebugAddress ElfDebugAddress;
#endif /* __cplusplus */

#endif 	/* __ElfDebugAddress_FWD_DEFINED__ */


#ifndef __ElfDebugDocumentContext_FWD_DEFINED__
#define __ElfDebugDocumentContext_FWD_DEFINED__

#ifdef __cplusplus
typedef class ElfDebugDocumentContext ElfDebugDocumentContext;
#else
typedef struct ElfDebugDocumentContext ElfDebugDocumentContext;
#endif /* __cplusplus */

#endif 	/* __ElfDebugDocumentContext_FWD_DEFINED__ */


#ifndef __ElfDebugCodeContext_FWD_DEFINED__
#define __ElfDebugCodeContext_FWD_DEFINED__

#ifdef __cplusplus
typedef class ElfDebugCodeContext ElfDebugCodeContext;
#else
typedef struct ElfDebugCodeContext ElfDebugCodeContext;
#endif /* __cplusplus */

#endif 	/* __ElfDebugCodeContext_FWD_DEFINED__ */


#ifndef __ElfDebugStackFrame_FWD_DEFINED__
#define __ElfDebugStackFrame_FWD_DEFINED__

#ifdef __cplusplus
typedef class ElfDebugStackFrame ElfDebugStackFrame;
#else
typedef struct ElfDebugStackFrame ElfDebugStackFrame;
#endif /* __cplusplus */

#endif 	/* __ElfDebugStackFrame_FWD_DEFINED__ */


/* header files for imported files */
#include "oaidl.h"
#include "ocidl.h"
#include "msdbg.h"

#ifdef __cplusplus
extern "C"{
#endif 


#ifndef __IElfSymbolProvider_INTERFACE_DEFINED__
#define __IElfSymbolProvider_INTERFACE_DEFINED__

/* interface IElfSymbolProvider */
/* [unique][uuid][object] */ 


EXTERN_C const IID IID_IElfSymbolProvider;

#if defined(__cplusplus) && !defined(CINTERFACE)
    
    MIDL_INTERFACE("c8b40372-81f0-45f5-8532-02f8443dd2a6")
    IElfSymbolProvider : public IDebugSymbolProvider
    {
    public:
        virtual HRESULT STDMETHODCALLTYPE GetAddressFromMemory( 
            DWORD memAddr,
            IDebugAddress **ppAddress) = 0;
        
        virtual HRESULT STDMETHODCALLTYPE LoadModule( 
            IDebugModule2 *pDebugModule,
            LPCOLESTR pszFilepath,
            DWORD address) = 0;
        
        virtual HRESULT STDMETHODCALLTYPE GetStackFrame( 
            IDebugAddress *pAddress,
            IDebugThread2 *pThread,
            IDebugStackFrame2 **ppStackFrame) = 0;
        
    };
    
    
#else 	/* C style interface */

    typedef struct IElfSymbolProviderVtbl
    {
        BEGIN_INTERFACE
        
        DECLSPEC_XFGVIRT(IUnknown, QueryInterface)
        HRESULT ( STDMETHODCALLTYPE *QueryInterface )( 
            IElfSymbolProvider * This,
            /* [in] */ REFIID riid,
            /* [annotation][iid_is][out] */ 
            _COM_Outptr_  void **ppvObject);
        
        DECLSPEC_XFGVIRT(IUnknown, AddRef)
        ULONG ( STDMETHODCALLTYPE *AddRef )( 
            IElfSymbolProvider * This);
        
        DECLSPEC_XFGVIRT(IUnknown, Release)
        ULONG ( STDMETHODCALLTYPE *Release )( 
            IElfSymbolProvider * This);
        
        DECLSPEC_XFGVIRT(IDebugSymbolProvider, Initialize)
        HRESULT ( STDMETHODCALLTYPE *Initialize )( 
            IElfSymbolProvider * This,
            /* [in] */ IDebugEngineSymbolProviderServices *pServices);
        
        DECLSPEC_XFGVIRT(IDebugSymbolProvider, Uninitialize)
        HRESULT ( STDMETHODCALLTYPE *Uninitialize )( 
            IElfSymbolProvider * This);
        
        DECLSPEC_XFGVIRT(IDebugSymbolProvider, GetContainerField)
        HRESULT ( STDMETHODCALLTYPE *GetContainerField )( 
            IElfSymbolProvider * This,
            /* [in] */ IDebugAddress *pAddress,
            /* [out] */ IDebugContainerField **ppContainerField);
        
        DECLSPEC_XFGVIRT(IDebugSymbolProvider, GetField)
        HRESULT ( STDMETHODCALLTYPE *GetField )( 
            IElfSymbolProvider * This,
            /* [in] */ IDebugAddress *pAddress,
            /* [in] */ IDebugAddress *pAddressCur,
            /* [out] */ IDebugField **ppField);
        
        DECLSPEC_XFGVIRT(IDebugSymbolProvider, GetAddressesFromPosition)
        HRESULT ( STDMETHODCALLTYPE *GetAddressesFromPosition )( 
            IElfSymbolProvider * This,
            /* [in] */ IDebugDocumentPosition2 *pDocPos,
            /* [in] */ BOOL fStatmentOnly,
            /* [out] */ IEnumDebugAddresses **ppEnumBegAddresses,
            /* [out] */ IEnumDebugAddresses **ppEnumEndAddresses);
        
        DECLSPEC_XFGVIRT(IDebugSymbolProvider, GetAddressesFromContext)
        HRESULT ( STDMETHODCALLTYPE *GetAddressesFromContext )( 
            IElfSymbolProvider * This,
            /* [in] */ IDebugDocumentContext2 *pDocContext,
            /* [in] */ BOOL fStatmentOnly,
            /* [out] */ IEnumDebugAddresses **ppEnumBegAddresses,
            /* [out] */ IEnumDebugAddresses **ppEnumEndAddresses);
        
        DECLSPEC_XFGVIRT(IDebugSymbolProvider, GetContextFromAddress)
        HRESULT ( STDMETHODCALLTYPE *GetContextFromAddress )( 
            IElfSymbolProvider * This,
            /* [in] */ IDebugAddress *pAddress,
            /* [out] */ IDebugDocumentContext2 **ppDocContext);
        
        DECLSPEC_XFGVIRT(IDebugSymbolProvider, GetLanguage)
        HRESULT ( STDMETHODCALLTYPE *GetLanguage )( 
            IElfSymbolProvider * This,
            /* [in] */ IDebugAddress *pAddress,
            /* [out] */ GUID *pguidLanguage,
            /* [out] */ GUID *pguidLanguageVendor);
        
        DECLSPEC_XFGVIRT(IDebugSymbolProvider, GetGlobalContainer)
        HRESULT ( STDMETHODCALLTYPE *GetGlobalContainer )( 
            IElfSymbolProvider * This,
            /* [out] */ IDebugContainerField **pField);
        
        DECLSPEC_XFGVIRT(IDebugSymbolProvider, GetMethodFieldsByName)
        HRESULT ( STDMETHODCALLTYPE *GetMethodFieldsByName )( 
            IElfSymbolProvider * This,
            /* [full][in] */ LPCOLESTR pszFullName,
            /* [in] */ NAME_MATCH nameMatch,
            /* [out] */ IEnumDebugFields **ppEnum);
        
        DECLSPEC_XFGVIRT(IDebugSymbolProvider, GetClassTypeByName)
        HRESULT ( STDMETHODCALLTYPE *GetClassTypeByName )( 
            IElfSymbolProvider * This,
            /* [full][in] */ LPCOLESTR pszClassName,
            /* [in] */ NAME_MATCH nameMatch,
            /* [out] */ IDebugClassField **ppField);
        
        DECLSPEC_XFGVIRT(IDebugSymbolProvider, GetNamespacesUsedAtAddress)
        HRESULT ( STDMETHODCALLTYPE *GetNamespacesUsedAtAddress )( 
            IElfSymbolProvider * This,
            /* [in] */ IDebugAddress *pAddress,
            /* [out] */ IEnumDebugFields **ppEnum);
        
        DECLSPEC_XFGVIRT(IDebugSymbolProvider, GetTypeByName)
        HRESULT ( STDMETHODCALLTYPE *GetTypeByName )( 
            IElfSymbolProvider * This,
            /* [full][in] */ LPCOLESTR pszClassName,
            /* [in] */ NAME_MATCH nameMatch,
            /* [out] */ IDebugField **ppField);
        
        DECLSPEC_XFGVIRT(IDebugSymbolProvider, GetNextAddress)
        HRESULT ( STDMETHODCALLTYPE *GetNextAddress )( 
            IElfSymbolProvider * This,
            /* [in] */ IDebugAddress *pAddress,
            /* [in] */ BOOL fStatmentOnly,
            /* [out] */ IDebugAddress **ppAddress);
        
        DECLSPEC_XFGVIRT(IElfSymbolProvider, GetAddressFromMemory)
        HRESULT ( STDMETHODCALLTYPE *GetAddressFromMemory )( 
            IElfSymbolProvider * This,
            DWORD memAddr,
            IDebugAddress **ppAddress);
        
        DECLSPEC_XFGVIRT(IElfSymbolProvider, LoadModule)
        HRESULT ( STDMETHODCALLTYPE *LoadModule )( 
            IElfSymbolProvider * This,
            IDebugModule2 *pDebugModule,
            LPCOLESTR pszFilepath,
            DWORD address);
        
        DECLSPEC_XFGVIRT(IElfSymbolProvider, GetStackFrame)
        HRESULT ( STDMETHODCALLTYPE *GetStackFrame )( 
            IElfSymbolProvider * This,
            IDebugAddress *pAddress,
            IDebugThread2 *pThread,
            IDebugStackFrame2 **ppStackFrame);
        
        END_INTERFACE
    } IElfSymbolProviderVtbl;

    interface IElfSymbolProvider
    {
        CONST_VTBL struct IElfSymbolProviderVtbl *lpVtbl;
    };

    

#ifdef COBJMACROS


#define IElfSymbolProvider_QueryInterface(This,riid,ppvObject)	\
    ( (This)->lpVtbl -> QueryInterface(This,riid,ppvObject) ) 

#define IElfSymbolProvider_AddRef(This)	\
    ( (This)->lpVtbl -> AddRef(This) ) 

#define IElfSymbolProvider_Release(This)	\
    ( (This)->lpVtbl -> Release(This) ) 


#define IElfSymbolProvider_Initialize(This,pServices)	\
    ( (This)->lpVtbl -> Initialize(This,pServices) ) 

#define IElfSymbolProvider_Uninitialize(This)	\
    ( (This)->lpVtbl -> Uninitialize(This) ) 

#define IElfSymbolProvider_GetContainerField(This,pAddress,ppContainerField)	\
    ( (This)->lpVtbl -> GetContainerField(This,pAddress,ppContainerField) ) 

#define IElfSymbolProvider_GetField(This,pAddress,pAddressCur,ppField)	\
    ( (This)->lpVtbl -> GetField(This,pAddress,pAddressCur,ppField) ) 

#define IElfSymbolProvider_GetAddressesFromPosition(This,pDocPos,fStatmentOnly,ppEnumBegAddresses,ppEnumEndAddresses)	\
    ( (This)->lpVtbl -> GetAddressesFromPosition(This,pDocPos,fStatmentOnly,ppEnumBegAddresses,ppEnumEndAddresses) ) 

#define IElfSymbolProvider_GetAddressesFromContext(This,pDocContext,fStatmentOnly,ppEnumBegAddresses,ppEnumEndAddresses)	\
    ( (This)->lpVtbl -> GetAddressesFromContext(This,pDocContext,fStatmentOnly,ppEnumBegAddresses,ppEnumEndAddresses) ) 

#define IElfSymbolProvider_GetContextFromAddress(This,pAddress,ppDocContext)	\
    ( (This)->lpVtbl -> GetContextFromAddress(This,pAddress,ppDocContext) ) 

#define IElfSymbolProvider_GetLanguage(This,pAddress,pguidLanguage,pguidLanguageVendor)	\
    ( (This)->lpVtbl -> GetLanguage(This,pAddress,pguidLanguage,pguidLanguageVendor) ) 

#define IElfSymbolProvider_GetGlobalContainer(This,pField)	\
    ( (This)->lpVtbl -> GetGlobalContainer(This,pField) ) 

#define IElfSymbolProvider_GetMethodFieldsByName(This,pszFullName,nameMatch,ppEnum)	\
    ( (This)->lpVtbl -> GetMethodFieldsByName(This,pszFullName,nameMatch,ppEnum) ) 

#define IElfSymbolProvider_GetClassTypeByName(This,pszClassName,nameMatch,ppField)	\
    ( (This)->lpVtbl -> GetClassTypeByName(This,pszClassName,nameMatch,ppField) ) 

#define IElfSymbolProvider_GetNamespacesUsedAtAddress(This,pAddress,ppEnum)	\
    ( (This)->lpVtbl -> GetNamespacesUsedAtAddress(This,pAddress,ppEnum) ) 

#define IElfSymbolProvider_GetTypeByName(This,pszClassName,nameMatch,ppField)	\
    ( (This)->lpVtbl -> GetTypeByName(This,pszClassName,nameMatch,ppField) ) 

#define IElfSymbolProvider_GetNextAddress(This,pAddress,fStatmentOnly,ppAddress)	\
    ( (This)->lpVtbl -> GetNextAddress(This,pAddress,fStatmentOnly,ppAddress) ) 


#define IElfSymbolProvider_GetAddressFromMemory(This,memAddr,ppAddress)	\
    ( (This)->lpVtbl -> GetAddressFromMemory(This,memAddr,ppAddress) ) 

#define IElfSymbolProvider_LoadModule(This,pDebugModule,pszFilepath,address)	\
    ( (This)->lpVtbl -> LoadModule(This,pDebugModule,pszFilepath,address) ) 

#define IElfSymbolProvider_GetStackFrame(This,pAddress,pThread,ppStackFrame)	\
    ( (This)->lpVtbl -> GetStackFrame(This,pAddress,pThread,ppStackFrame) ) 

#endif /* COBJMACROS */


#endif 	/* C style interface */




#endif 	/* __IElfSymbolProvider_INTERFACE_DEFINED__ */



#ifndef __ElfSymbolProviderLib_LIBRARY_DEFINED__
#define __ElfSymbolProviderLib_LIBRARY_DEFINED__

/* library ElfSymbolProviderLib */
/* [version][uuid] */ 


EXTERN_C const IID LIBID_ElfSymbolProviderLib;

EXTERN_C const CLSID CLSID_ElfSymbolProvider;

#ifdef __cplusplus

class DECLSPEC_UUID("305ae8e3-ce4d-4a0b-889c-9836bf4ddedf")
ElfSymbolProvider;
#endif

EXTERN_C const CLSID CLSID_ElfDebugAddress;

#ifdef __cplusplus

class DECLSPEC_UUID("a40ab257-221d-4db3-8618-f7e2cfb16355")
ElfDebugAddress;
#endif

EXTERN_C const CLSID CLSID_ElfDebugDocumentContext;

#ifdef __cplusplus

class DECLSPEC_UUID("546d6384-97ab-428b-9be2-5e287080ce93")
ElfDebugDocumentContext;
#endif

EXTERN_C const CLSID CLSID_ElfDebugCodeContext;

#ifdef __cplusplus

class DECLSPEC_UUID("d8762730-223e-4616-9d72-236321bbfb9a")
ElfDebugCodeContext;
#endif

EXTERN_C const CLSID CLSID_ElfDebugStackFrame;

#ifdef __cplusplus

class DECLSPEC_UUID("ab1a8bf8-f8b7-4681-bdc8-5c389f9835fe")
ElfDebugStackFrame;
#endif
#endif /* __ElfSymbolProviderLib_LIBRARY_DEFINED__ */

/* Additional Prototypes for ALL interfaces */

/* end of Additional Prototypes */

#ifdef __cplusplus
}
#endif

#endif


