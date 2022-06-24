using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;

namespace FPGAProjectExtension.DebugEngine
{
	class Constants
	{
		public const uint EVENT_ASYNCHRONOUS = 0;
		public const uint EVENT_SYNCHRONOUS = 0x1;
		public const uint EVENT_STOPPING = 0x2;
		public const uint EVENT_ASYNC_STOP = 0x2;
		public const uint EVENT_SYNC_STOP = 0x3;
		public const uint EVENT_IMMEDIATE = 0x4;
		public const uint EVENT_EXPRESSION_EVALUATION = 0x8;

		public const int E_PORTSUPPLIER_NO_PORT = unchecked((int)0x80040080);
	}
	[Guid("E5940649-4915-48BA-B14E-3FDC216C5496")]
	internal class MipsDebugProcessCreateEvent
		: IDebugProcessCreateEvent2,
		IDebugEvent2
	{
		public MipsDebugProcessCreateEvent(uint attributes)
		{
			Attributes = attributes;
		}

		public uint Attributes { get; set; }
		public int GetAttributes(out uint pdwAttrib)
		{
			pdwAttrib = Attributes;
			return VSConstants.S_OK;
		}
	}
}
