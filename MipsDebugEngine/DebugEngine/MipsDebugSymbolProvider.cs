/*
 Copyright (C) 2022 jobmarley

 This file is part of VSMipsProjectExtension.

 This program is free software; you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation; either version 3 of the License, or (at your option) any later version.

 This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

 You should have received a copy of the GNU General Public License along with Foobar. If not, see <https://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;

namespace VSMipsProjectExtension.DebugEngine
{
	internal class MipsDebugSymbolProvider
		: IDebugSymbolProvider
	{
		public MipsDebugSymbolProvider()
		{

		}
		public int Initialize(IDebugEngineSymbolProviderServices pServices)
		{
			throw new NotImplementedException();
		}

		public int Uninitialize()
		{
			throw new NotImplementedException();
		}

		public int GetContainerField(IDebugAddress pAddress, out IDebugContainerField ppContainerField)
		{
			throw new NotImplementedException();
		}

		public int GetField(IDebugAddress pAddress, IDebugAddress pAddressCur, out IDebugField ppField)
		{
			throw new NotImplementedException();
		}

		public int GetAddressesFromPosition(IDebugDocumentPosition2 pDocPos, int fStatmentOnly, out IEnumDebugAddresses ppEnumBegAddresses, out IEnumDebugAddresses ppEnumEndAddresses)
		{
			throw new NotImplementedException();
		}

		public int GetAddressesFromContext(IDebugDocumentContext2 pDocContext, int fStatmentOnly, out IEnumDebugAddresses ppEnumBegAddresses, out IEnumDebugAddresses ppEnumEndAddresses)
		{
			throw new NotImplementedException();
		}

		public int GetContextFromAddress(IDebugAddress pAddress, out IDebugDocumentContext2 ppDocContext)
		{
			throw new NotImplementedException();
		}

		public int GetLanguage(IDebugAddress pAddress, out Guid pguidLanguage, out Guid pguidLanguageVendor)
		{
			throw new NotImplementedException();
		}

		public int GetGlobalContainer(out IDebugContainerField pField)
		{
			throw new NotImplementedException();
		}

		public int GetMethodFieldsByName(string pszFullName, NAME_MATCH nameMatch, out IEnumDebugFields ppEnum)
		{
			throw new NotImplementedException();
		}

		public int GetClassTypeByName(string pszClassName, NAME_MATCH nameMatch, out IDebugClassField ppField)
		{
			throw new NotImplementedException();
		}

		public int GetNamespacesUsedAtAddress(IDebugAddress pAddress, out IEnumDebugFields ppEnum)
		{
			throw new NotImplementedException();
		}

		public int GetTypeByName(string pszClassName, NAME_MATCH nameMatch, out IDebugField ppField)
		{
			throw new NotImplementedException();
		}

		public int GetNextAddress(IDebugAddress pAddress, int fStatmentOnly, out IDebugAddress ppAddress)
		{
			throw new NotImplementedException();
		}
	}
}
