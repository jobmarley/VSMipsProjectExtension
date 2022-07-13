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
	abstract class EnumeratorBase<T>
	{
		protected IEnumerable<T> m_l = null;
		protected IEnumerator<T> m_current = null;
		protected EnumeratorBase(IEnumerable<T> l)
		{
			m_l = l;
			m_current = l.GetEnumerator();
		}
		protected int Next(uint celt, T[] rgelt, ref uint pceltFetched)
		{
			pceltFetched = 0;
			while (pceltFetched < celt)
			{
				if (!m_current.MoveNext())
					break;
				rgelt[pceltFetched++] = m_current.Current;
			}
			return VSConstants.S_OK;
		}

		protected int Skip(uint celt)
		{
			while (celt > 0)
			{
				if (!m_current.MoveNext())
					return VSConstants.S_FALSE;
				--celt;
			}
			return VSConstants.S_OK;
		}

		protected int Reset()
		{
			m_current.Reset();
			return VSConstants.S_OK;
		}

		protected int GetCount(out uint pcelt)
		{
			pcelt = (uint)m_l.Count();
			return VSConstants.S_OK;
		}
	}
	class EnumDebugPrograms2
		: EnumeratorBase<IDebugProgram2>,
		IEnumDebugPrograms2
	{
		public EnumDebugPrograms2(IEnumerable<IDebugProgram2> l)
			: base(l)
		{
		}
		int IEnumDebugPrograms2.Next(uint celt, IDebugProgram2[] rgelt, ref uint pceltFetched)
		{
			return base.Next(celt, rgelt, ref pceltFetched);
		}

		int IEnumDebugPrograms2.Skip(uint celt)
		{
			return base.Skip(celt);
		}

		int IEnumDebugPrograms2.Reset()
		{
			return base.Reset();
		}

		int IEnumDebugPrograms2.Clone(out IEnumDebugPrograms2 ppEnum)
		{
			ppEnum = new EnumDebugPrograms2(m_l);
			return VSConstants.S_OK;
		}

		int IEnumDebugPrograms2.GetCount(out uint pcelt)
		{
			return base.GetCount(out pcelt);
		}
	}
	class EnumDebugPorts2
		: EnumeratorBase<IDebugPort2>,
		IEnumDebugPorts2
	{
		public EnumDebugPorts2(IEnumerable<IDebugPort2> l)
			: base(l)
		{
		}
		int IEnumDebugPorts2.Next(uint celt, IDebugPort2[] rgelt, ref uint pceltFetched)
		{
			return base.Next(celt, rgelt, ref pceltFetched);
		}

		int IEnumDebugPorts2.Skip(uint celt)
		{
			return base.Skip(celt);
		}

		int IEnumDebugPorts2.Reset()
		{
			return base.Reset();
		}

		int IEnumDebugPorts2.Clone(out IEnumDebugPorts2 ppEnum)
		{
			ppEnum = new EnumDebugPorts2(m_l);
			return VSConstants.S_OK;
		}

		int IEnumDebugPorts2.GetCount(out uint pcelt)
		{
			return base.GetCount(out pcelt);
		}
	}
	class EnumDebugModules2
		: EnumeratorBase<IDebugModule2>,
		IEnumDebugModules2
	{
		public EnumDebugModules2(IEnumerable<IDebugModule2> l)
			: base(l)
		{
		}
		int IEnumDebugModules2.Next(uint celt, IDebugModule2[] rgelt, ref uint pceltFetched)
		{
			return base.Next(celt, rgelt, ref pceltFetched);
		}

		int IEnumDebugModules2.Skip(uint celt)
		{
			return base.Skip(celt);
		}

		int IEnumDebugModules2.Reset()
		{
			return base.Reset();
		}

		int IEnumDebugModules2.Clone(out IEnumDebugModules2 ppEnum)
		{
			ppEnum = new EnumDebugModules2(m_l);
			return VSConstants.S_OK;
		}

		int IEnumDebugModules2.GetCount(out uint pcelt)
		{
			return base.GetCount(out pcelt);
		}
	}
	class EnumDebugFrameInfo2
		: EnumeratorBase<FRAMEINFO>,
		IEnumDebugFrameInfo2
	{
		public EnumDebugFrameInfo2(IEnumerable<FRAMEINFO> l)
			: base(l)
		{
		}
		int IEnumDebugFrameInfo2.Next(uint celt, FRAMEINFO[] rgelt, ref uint pceltFetched)
		{
			return base.Next(celt, rgelt, ref pceltFetched);
		}

		int IEnumDebugFrameInfo2.Skip(uint celt)
		{
			return base.Skip(celt);
		}

		int IEnumDebugFrameInfo2.Reset()
		{
			return base.Reset();
		}

		int IEnumDebugFrameInfo2.Clone(out IEnumDebugFrameInfo2 ppEnum)
		{
			ppEnum = new EnumDebugFrameInfo2(m_l);
			return VSConstants.S_OK;
		}

		int IEnumDebugFrameInfo2.GetCount(out uint pcelt)
		{
			return base.GetCount(out pcelt);
		}
	}
	class EnumDebugThreads2
		: EnumeratorBase<IDebugThread2>,
		IEnumDebugThreads2
	{
		public EnumDebugThreads2(IEnumerable<IDebugThread2> l)
			: base(l)
		{
		}
		int IEnumDebugThreads2.Next(uint celt, IDebugThread2[] rgelt, ref uint pceltFetched)
		{
			return base.Next(celt, rgelt, ref pceltFetched);
		}

		int IEnumDebugThreads2.Skip(uint celt)
		{
			return base.Skip(celt);
		}

		int IEnumDebugThreads2.Reset()
		{
			return base.Reset();
		}

		int IEnumDebugThreads2.Clone(out IEnumDebugThreads2 ppEnum)
		{
			ppEnum = new EnumDebugThreads2(m_l);
			return VSConstants.S_OK;
		}

		int IEnumDebugThreads2.GetCount(out uint pcelt)
		{
			return base.GetCount(out pcelt);
		}
	}
	

}
