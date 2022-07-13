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
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;

namespace VSMipsProjectExtension.DebugEngine
{
	[ComVisible(true)]
	[Guid("9D1EEDD2-CF21-416A-B53A-D933B2F47920")]
	public class MipsDebugProgramProvider
		: IDebugProgramProvider2
	{
		public int GetProviderProcessData(enum_PROVIDER_FLAGS Flags, IDebugDefaultPort2 pPort, AD_PROCESS_ID ProcessId, CONST_GUID_ARRAY EngineFilter, PROVIDER_PROCESS_DATA[] pProcess)
		{
			throw new NotImplementedException();
		}

		public int GetProviderProgramNode(enum_PROVIDER_FLAGS Flags, IDebugDefaultPort2 pPort, AD_PROCESS_ID ProcessId, ref Guid guidEngine, ulong programId, out IDebugProgramNode2 ppProgramNode)
		{
			throw new NotImplementedException();
		}

		public int WatchForProviderEvents(enum_PROVIDER_FLAGS Flags, IDebugDefaultPort2 pPort, AD_PROCESS_ID ProcessId, CONST_GUID_ARRAY EngineFilter, ref Guid guidLaunchingEngine, IDebugPortNotify2 pEventCallback)
		{
			return VSConstants.S_OK;
		}

		public int SetLocale(ushort wLangID)
		{
			return VSConstants.S_OK;
		}
	}
}
