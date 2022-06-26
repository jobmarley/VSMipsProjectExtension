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
	class EnumDebugPrograms2
		: IEnumDebugPrograms2
	{
		IList<IDebugProgram2> m_l = null;
		uint m_i = 0;
		public EnumDebugPrograms2(IList<IDebugProgram2> l)
		{
			m_l = l;
		}
		public int Next(uint celt, IDebugProgram2[] rgelt, ref uint pceltFetched)
		{
			uint start = m_i;
			uint count = (uint)Math.Min(m_l.Count, m_i + celt);
			for (; m_i < count; ++m_i)
				rgelt[m_i - start] = m_l[(int)m_i];
			pceltFetched = count;
			return VSConstants.S_OK;
		}

		public int Skip(uint celt)
		{
			m_i += celt;
			return VSConstants.S_OK;
		}

		public int Reset()
		{
			m_i = 0;
			return VSConstants.S_OK;
		}

		public int Clone(out IEnumDebugPrograms2 ppEnum)
		{
			ppEnum = new EnumDebugPrograms2(m_l);
			return VSConstants.S_OK;
		}

		public int GetCount(out uint pcelt)
		{
			pcelt = (uint)m_l.Count();
			return VSConstants.S_OK;
		}
	}
	class EnumDebugPorts2
		: IEnumDebugPorts2
	{
		IList<IDebugPort2> m_l = null;
		uint m_i = 0;
		public EnumDebugPorts2(IList<IDebugPort2> l)
		{
			m_l = l;
		}
		public int Next(uint celt, IDebugPort2[] rgelt, ref uint pceltFetched)
		{
			uint start = m_i;
			uint count = (uint)Math.Min(m_l.Count, m_i + celt);
			for (; m_i < count; ++m_i)
				rgelt[m_i - start] = m_l[(int)m_i];
			pceltFetched = count;
			return VSConstants.S_OK;
		}

		public int Skip(uint celt)
		{
			m_i += celt;
			return VSConstants.S_OK;
		}

		public int Reset()
		{
			m_i = 0;
			return VSConstants.S_OK;
		}

		public int Clone(out IEnumDebugPorts2 ppEnum)
		{
			ppEnum = new EnumDebugPorts2(m_l);
			return VSConstants.S_OK;
		}

		public int GetCount(out uint pcelt)
		{
			pcelt = (uint)m_l.Count();
			return VSConstants.S_OK;
		}
	}
	class EnumDebugModules2
		: IEnumDebugModules2
	{
		IList<IDebugModule2> m_l = null;
		uint m_i = 0;
		public EnumDebugModules2(IList<IDebugModule2> l)
		{
			m_l = l;
		}
		public int Next(uint celt, IDebugModule2[] rgelt, ref uint pceltFetched)
		{
			uint start = m_i;
			uint count = (uint)Math.Min(m_l.Count, m_i + celt);
			for (; m_i < count; ++m_i)
				rgelt[m_i - start] = m_l[(int)m_i];
			pceltFetched = count;
			return VSConstants.S_OK;
		}

		public int Skip(uint celt)
		{
			m_i += celt;
			return VSConstants.S_OK;
		}

		public int Reset()
		{
			m_i = 0;
			return VSConstants.S_OK;
		}

		public int Clone(out IEnumDebugModules2 ppEnum)
		{
			ppEnum = new EnumDebugModules2(m_l);
			return VSConstants.S_OK;
		}

		public int GetCount(out uint pcelt)
		{
			pcelt = (uint)m_l.Count();
			return VSConstants.S_OK;
		}
	}
	
}
