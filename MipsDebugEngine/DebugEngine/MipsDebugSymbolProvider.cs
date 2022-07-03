using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;

namespace FPGAProjectExtension.DebugEngine
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
