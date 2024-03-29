﻿/*
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
		public const int E_BP_DELETED = unchecked((int)0x80040060);
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
		public MipsDebugThread Thread { get; } = null;
		public MipsDebugThreadCreateEvent(MipsDebugThread thread)
			: base(typeof(IDebugThreadCreateEvent2).GUID)
		{
			Thread = thread;
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
	internal class MipsDebugBreakEvent
		: MipsStoppingEvent,
		IDebugBreakEvent2,
		IDebugEvent2
	{
		public MipsDebugThread Thread { get; } = null;
		public MipsDebugBreakEvent(MipsDebugThread thread)
			: base(typeof(IDebugBreakEvent2).GUID)
		{
			Thread = thread;
		}
	}
	internal class MipsDebugEECompleteEvent
		: MipsAsynchronousEvent,
		IDebugExpressionEvaluationCompleteEvent2,
		IDebugEvent2
	{
		public MipsDebugThread Thread { get; } = null;
		public IDebugExpression2 Expression { get; } = null;
		public IDebugProperty2 Property { get; } = null;
		public MipsDebugEECompleteEvent(
			MipsDebugThread thread,
			IDebugExpression2 expression,
			IDebugProperty2 debugProperty)
			: base(typeof(IDebugExpressionEvaluationCompleteEvent2).GUID)
		{
			Thread = thread;
			Expression = expression;
			Property = debugProperty;
		}

		public int GetExpression(out IDebugExpression2 ppExpr)
		{
			ppExpr = Expression;
			return VSConstants.S_OK;
		}

		public int GetResult(out IDebugProperty2 ppResult)
		{
			ppResult = Property;
			return VSConstants.S_OK;
		}
	}
	internal class MipsPropertyCreateEvent
		: MipsAsynchronousEvent,
		IDebugPropertyCreateEvent2,
		IDebugEvent2
	{
		public IDebugProperty2 Property { get; } = null;
		public MipsPropertyCreateEvent(
			IDebugProperty2 debugProperty)
			: base(typeof(IDebugPropertyCreateEvent2).GUID)
		{
			Property = debugProperty;
		}

		public int GetDebugProperty(out IDebugProperty2 ppProperty)
		{
			ppProperty = Property;
			return VSConstants.S_OK;
		}
	}

	internal class MipsBreakpointErrorEvent
		: MipsAsynchronousEvent,
		IDebugBreakpointErrorEvent2,
		IDebugEvent2
	{
		IDebugErrorBreakpoint2 m_errorBreakpoint = null;
		public MipsBreakpointErrorEvent(
			IDebugErrorBreakpoint2 errorBreakpoint)
			: base(typeof(IDebugBreakpointErrorEvent2).GUID)
		{
			m_errorBreakpoint = errorBreakpoint;
		}
		public int GetErrorBreakpoint(out IDebugErrorBreakpoint2 ppErrorBP)
		{
			ppErrorBP = m_errorBreakpoint;
			return VSConstants.S_OK;
		}
	}

	internal class MipsBreakpointEvent
		: MipsAsynchronousEvent,
		IDebugBreakpointEvent2,
		IDebugEvent2
	{
		IEnumDebugBoundBreakpoints2 m_pEnumBoundBreakpoints = null;
		public MipsBreakpointEvent(
			IEnumDebugBoundBreakpoints2 pEnumBoundBreakpoints)
			: base(typeof(IDebugBreakpointEvent2).GUID)
		{
			m_pEnumBoundBreakpoints = pEnumBoundBreakpoints;
		}
		public int EnumBreakpoints(out IEnumDebugBoundBreakpoints2 ppEnum)
		{
			return m_pEnumBoundBreakpoints.Clone(out ppEnum);
		}
	}
	internal class MipsBreakpointBoundEvent
		: MipsAsynchronousEvent,
		IDebugBreakpointBoundEvent2,
		IDebugEvent2
	{
		IDebugPendingBreakpoint2 m_pendingBreakpoint = null;
		IEnumDebugBoundBreakpoints2 m_boundBreakpoints = null;
		public MipsBreakpointBoundEvent(
			IDebugPendingBreakpoint2 pendingBreakpoint,
			IEnumDebugBoundBreakpoints2 boundBreakpoints)
			: base(typeof(IDebugBreakpointBoundEvent2).GUID)
		{
			m_pendingBreakpoint = pendingBreakpoint;
			m_boundBreakpoints = boundBreakpoints;
		}

		public int GetPendingBreakpoint(out IDebugPendingBreakpoint2 ppPendingBP)
		{
			ppPendingBP = m_pendingBreakpoint;
			return VSConstants.S_OK;
		}

		public int EnumBoundBreakpoints(out IEnumDebugBoundBreakpoints2 ppEnum)
		{
			return m_boundBreakpoints.Clone(out ppEnum);
		}
	}
}
