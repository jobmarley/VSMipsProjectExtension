/*
 Copyright (C) 2022 jobmarley

 This file is part of VSMipsProjectExtension.

 This program is free software; you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation; either version 3 of the License, or (at your option) any later version.

 This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

 You should have received a copy of the GNU General Public License along with Foobar. If not, see <https://www.gnu.org/licenses/>.
 */

using Microsoft.VisualStudio.Debugger.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace VSMipsProjectExtension.DebugEngine
{

	[ComImport]
	[Guid("BB324892-D76E-41A6-97EC-8A106D43C4CF")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	interface IRegisterOperation
	{

		[MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall)]
		int Read(uint index, out uint pValue);

		[MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall)]
		int Write(uint index, uint value);
	};
	[ComImport]
	[Guid("06E76594-7C91-4282-9B10-6A626D5D4864")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	interface IMemoryOperation
	{

		[MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall)]
		int Read([In, Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] byte[] buffer, uint offset, uint count, out uint pReadCount);

		[MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall)]
		int Write([In, Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] byte[] buffer, uint offset, uint count, out uint pWrittenCount);
	};

	[ComImport]
	[Guid("c8b40372-81f0-45f5-8532-02f8443dd2a6")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	interface IElfSymbolProvider
		: IDebugSymbolProvider
	{
		[MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall)]
		new int Initialize([In][MarshalAs(UnmanagedType.Interface)] IDebugEngineSymbolProviderServices pServices);

		[MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall)]
		new int Uninitialize();

		[MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall)]
		new int GetContainerField([In][MarshalAs(UnmanagedType.Interface)] IDebugAddress pAddress, [MarshalAs(UnmanagedType.Interface)] out IDebugContainerField ppContainerField);

		[MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall)]
		new int GetField([In][MarshalAs(UnmanagedType.Interface)] IDebugAddress pAddress, [In][MarshalAs(UnmanagedType.Interface)] IDebugAddress pAddressCur, [MarshalAs(UnmanagedType.Interface)] out IDebugField ppField);

		[MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall)]
		new int GetAddressesFromPosition([In][MarshalAs(UnmanagedType.Interface)] IDebugDocumentPosition2 pDocPos, [In] int fStatmentOnly, [MarshalAs(UnmanagedType.Interface)] out IEnumDebugAddresses ppEnumBegAddresses, [MarshalAs(UnmanagedType.Interface)] out IEnumDebugAddresses ppEnumEndAddresses);

		[MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall)]
		new int GetAddressesFromContext([In][MarshalAs(UnmanagedType.Interface)] IDebugDocumentContext2 pDocContext, [In] int fStatmentOnly, [MarshalAs(UnmanagedType.Interface)] out IEnumDebugAddresses ppEnumBegAddresses, [MarshalAs(UnmanagedType.Interface)] out IEnumDebugAddresses ppEnumEndAddresses);

		[MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall)]
		new int GetContextFromAddress([In][MarshalAs(UnmanagedType.Interface)] IDebugAddress pAddress, [MarshalAs(UnmanagedType.Interface)] out IDebugDocumentContext2 ppDocContext);

		[MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall)]
		new int GetLanguage([In][MarshalAs(UnmanagedType.Interface)] IDebugAddress pAddress, out Guid pguidLanguage, out Guid pguidLanguageVendor);

		[MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall)]
		new int GetGlobalContainer([MarshalAs(UnmanagedType.Interface)] out IDebugContainerField pField);

		[MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall)]
		new int GetMethodFieldsByName([In][MarshalAs(UnmanagedType.LPWStr)] string pszFullName, [In][ComAliasName("Microsoft.VisualStudio.Debugger.Interop.NAME_MATCH")] NAME_MATCH nameMatch, [MarshalAs(UnmanagedType.Interface)] out IEnumDebugFields ppEnum);

		[MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall)]
		new int GetClassTypeByName([In][MarshalAs(UnmanagedType.LPWStr)] string pszClassName, [In][ComAliasName("Microsoft.VisualStudio.Debugger.Interop.NAME_MATCH")] NAME_MATCH nameMatch, [MarshalAs(UnmanagedType.Interface)] out IDebugClassField ppField);

		[MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall)]
		new int GetNamespacesUsedAtAddress([In][MarshalAs(UnmanagedType.Interface)] IDebugAddress pAddress, [MarshalAs(UnmanagedType.Interface)] out IEnumDebugFields ppEnum);

		[MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall)]
		new int GetTypeByName([In][MarshalAs(UnmanagedType.LPWStr)] string pszClassName, [In][ComAliasName("Microsoft.VisualStudio.Debugger.Interop.NAME_MATCH")] NAME_MATCH nameMatch, [MarshalAs(UnmanagedType.Interface)] out IDebugField ppField);

		[MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall)]
		new int GetNextAddress([In][MarshalAs(UnmanagedType.Interface)] IDebugAddress pAddress, [In] int fStatmentOnly, [MarshalAs(UnmanagedType.Interface)] out IDebugAddress ppAddress);

		[MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall)]
		int GetAddressFromMemory(uint memAddr, out IDebugAddress ppAddress);

		[MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall)]
		int LoadModule(IDebugModule2 debugModule, string pszFilepath, uint address);
		[MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall)]
		int GetStackFrame(IDebugAddress pAddress, IDebugThread2 pThread, IMemoryOperation pMemoryOp, IRegisterOperation pRegisterOp, out IDebugStackFrame2 ppStackFrame);
		[MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall)]
		int GetPreviousStackFrame(IDebugStackFrame2 pStackFrame, out IDebugStackFrame2 ppStackFrame);
		[MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall)]
		int SetEventCallback(IMipsDEEventCallback pDEEventCallback);
		[MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall)]
		int UnloadAll();
	}
}
