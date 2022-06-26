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
	abstract class MipsDebugEvent
		: IDebugEvent2
	{
		public MipsDebugEvent(Guid iid, uint attributes)
		{
			IID = iid;
			Attributes = attributes;
		}
		public Guid IID { get; }
		public uint Attributes { get; }
		int IDebugEvent2.GetAttributes(out uint eventAttributes)
		{
			eventAttributes = Attributes;
			return VSConstants.S_OK;
		}
	}
	abstract class MipsAsynchronousEvent
		: MipsDebugEvent
	{
		public MipsAsynchronousEvent(Guid iid)
			: base(iid, (uint)enum_EVENTATTRIBUTES.EVENT_ASYNCHRONOUS)
		{

		}
	}

	abstract class MipsStoppingEvent
		: MipsDebugEvent
	{
		public MipsStoppingEvent(Guid iid)
			: base(iid, (uint)enum_EVENTATTRIBUTES.EVENT_ASYNC_STOP)
		{

		}
	}

	abstract class MipsSynchronousEvent
		: MipsDebugEvent
	{
		public MipsSynchronousEvent(Guid iid)
			: base(iid, (uint)enum_EVENTATTRIBUTES.EVENT_SYNCHRONOUS)
		{

		}
	}
	abstract class MipsSynchronousStoppingEvent
		: MipsDebugEvent
	{
		public MipsSynchronousStoppingEvent(Guid iid)
			: base(iid, (uint)enum_EVENTATTRIBUTES.EVENT_STOPPING | (uint)enum_EVENTATTRIBUTES.EVENT_SYNCHRONOUS)
		{

		}
	}

	internal class MipsDebugProcessCreateEvent
		: MipsAsynchronousEvent,
		IDebugProcessCreateEvent2
	{
		public MipsDebugProcess Process { get; } = null;
		public MipsDebugProcessCreateEvent(MipsDebugProcess process)
			: base(typeof(IDebugProcessCreateEvent2).GUID)
		{
			Process = process;
		}
	}
	internal class MipsDebugProgramCreateEvent
		: MipsAsynchronousEvent,
		IDebugProgramCreateEvent2
	{
		public MipsDebugProgram Program { get; } = null;
		public MipsDebugProgramCreateEvent(MipsDebugProgram program)
			: base(typeof(IDebugProgramCreateEvent2).GUID)
		{
			Program = program;
		}
	}
	internal class MipsDebugProgramDestroyEvent
		: MipsSynchronousEvent,
		IDebugProgramDestroyEvent2
	{
		uint m_exitCode = 0;
		public MipsDebugProgram Program { get; }
		public MipsDebugProgramDestroyEvent(uint exitCode, MipsDebugProgram program)
			: base(typeof(IDebugProgramDestroyEvent2).GUID)
		{
			m_exitCode = exitCode;
			Program = program;
		}

		public int GetExitCode(out uint pdwExit)
		{
			pdwExit = m_exitCode;
			return VSConstants.S_OK;
		}
	}
	internal class MipsDebugProcessDestroyEvent
		: MipsSynchronousEvent,
		IDebugProcessDestroyEvent2
	{
		public MipsDebugProcess Process { get; }
		public MipsDebugProcessDestroyEvent(MipsDebugProcess process)
			: base(typeof(IDebugProcessDestroyEvent2).GUID)
		{
			Process = process;
		}
	}
	internal class MipsDebugEngineCreateEvent
		: MipsAsynchronousEvent,
		IDebugEngineCreateEvent2
	{
		private IDebugEngine2 m_debugEngine = null;
		public MipsDebugEngineCreateEvent(IDebugEngine2 debugEngine)
			: base(typeof(IDebugEngineCreateEvent2).GUID)
		{
			m_debugEngine = debugEngine;
		}

		public int GetEngine(out IDebugEngine2 pEngine)
		{
			pEngine = m_debugEngine;
			return VSConstants.S_OK;
		}
	}
	internal class MipsDebugThreadCreateEvent
		: MipsAsynchronousEvent, 
		IDebugThreadCreateEvent2
	{
		public MipsDebugThreadCreateEvent()
			: base(typeof(IDebugThreadCreateEvent2).GUID)
		{
		}
	}
	internal class MipsDebugModuleLoadEvent
		: MipsAsynchronousEvent,
		IDebugModuleLoadEvent2
	{
		private bool m_loading = false;
		public MipsDebugModule Module { get; } = null;
		public MipsDebugModuleLoadEvent(MipsDebugModule module, bool loading)
			: base(typeof(IDebugModuleLoadEvent2).GUID)
		{
			Module = module;
			m_loading = loading;
		}

		public int GetModule(out IDebugModule2 pModule, ref string pbstrDebugMessage, ref int pbLoad)
		{
			pModule = Module;
			pbstrDebugMessage = "";
			pbLoad = m_loading ? 1 : 0;
			return VSConstants.S_OK;
		}
	}
	internal class MipsDebugLoadCompleteEvent
		: MipsStoppingEvent,
		IDebugLoadCompleteEvent2,
		IDebugEvent2
	{
		public MipsDebugProgram Program { get; } = null;
		public MipsDebugLoadCompleteEvent(MipsDebugProgram program)
			: base(typeof(IDebugLoadCompleteEvent2).GUID)
		{
			Program = program;
		}
	}
	internal class MipsDebugEntryPointEvent
		: MipsAsynchronousEvent,
		IDebugEntryPointEvent2,
		IDebugEvent2
	{
		public MipsDebugEntryPointEvent()
			: base(typeof(IDebugEntryPointEvent2).GUID)
		{
		}
	}
}
