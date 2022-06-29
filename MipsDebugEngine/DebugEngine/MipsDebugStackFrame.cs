using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;

namespace FPGAProjectExtension.DebugEngine
{
	internal class MipsDebugStackFrame
		: IDebugStackFrame2
	{
		public uint Offset { get; set; } = 0;
		public MipsDebugThread Thread { get; } = null;
		public MipsDebugStackFrame(MipsDebugThread thread)
		{
			Thread = thread;
		}
		public int GetCodeContext(out IDebugCodeContext2 ppCodeCxt)
		{
			ppCodeCxt = null;
			return VSConstants.E_FAIL;
		}

		public int GetDocumentContext(out IDebugDocumentContext2 ppCxt)
		{
			ppCxt = null;
			return VSConstants.E_FAIL;
		}

		public int GetName(out string pbstrName)
		{
			pbstrName = "stackframe name";
			return VSConstants.S_OK;
		}

		MipsDebugModule GetModuleFromAddress(uint address)
		{
			return Thread?.Program?.Modules.FirstOrDefault(x => (address >= x.Offset) && (address < x.Offset + x.Size));
		}
		public int GetInfo(enum_FRAMEINFO_FLAGS dwFieldSpec, uint nRadix, FRAMEINFO[] pFrameInfo)
		{
			if (pFrameInfo == null || pFrameInfo.Length != 1)
				return VSConstants.E_INVALIDARG;

			FRAMEINFO fi = new FRAMEINFO();
			if (dwFieldSpec.HasFlag(enum_FRAMEINFO_FLAGS.FIF_FUNCNAME))
			{
				MipsDebugModule module = null;;
				if (dwFieldSpec.HasFlag(enum_FRAMEINFO_FLAGS.FIF_FUNCNAME_MODULE))
					module = GetModuleFromAddress(Offset);

				if (module != null)
				{
					fi.m_bstrFuncName = string.Format("{0}!{1:X8}()", System.IO.Path.GetFileName(module.Filepath), Offset);
					fi.m_dwValidFields |= enum_FRAMEINFO_FLAGS.FIF_FUNCNAME;
				}
				else
				{
					fi.m_bstrFuncName = string.Format("{0:X8}()", Offset);
					fi.m_dwValidFields |= enum_FRAMEINFO_FLAGS.FIF_FUNCNAME;
				}
			}
			if (dwFieldSpec.HasFlag(enum_FRAMEINFO_FLAGS.FIF_RETURNTYPE))
			{
				fi.m_bstrReturnType = "<return type>";
				fi.m_dwValidFields |= enum_FRAMEINFO_FLAGS.FIF_RETURNTYPE;
			}
			if (dwFieldSpec.HasFlag(enum_FRAMEINFO_FLAGS.FIF_ARGS))
			{
				fi.m_bstrArgs = "";
				fi.m_dwValidFields |= enum_FRAMEINFO_FLAGS.FIF_ARGS;
			}
			if (dwFieldSpec.HasFlag(enum_FRAMEINFO_FLAGS.FIF_LANGUAGE))
			{
				fi.m_bstrLanguage = "Unknown";
				fi.m_dwValidFields |= enum_FRAMEINFO_FLAGS.FIF_LANGUAGE;
			}
			if (dwFieldSpec.HasFlag(enum_FRAMEINFO_FLAGS.FIF_MODULE))
			{
				fi.m_bstrModule = System.IO.Path.GetFileNameWithoutExtension(Thread?.Program?.Modules?.FirstOrDefault()?.Filepath);
				fi.m_dwValidFields |= enum_FRAMEINFO_FLAGS.FIF_MODULE;
			}
			if (dwFieldSpec.HasFlag(enum_FRAMEINFO_FLAGS.FIF_DEBUG_MODULEP))
			{
				fi.m_pModule = Thread?.Program?.Modules?.FirstOrDefault();
				fi.m_dwValidFields |= enum_FRAMEINFO_FLAGS.FIF_DEBUG_MODULEP;
			}
			if (dwFieldSpec.HasFlag(enum_FRAMEINFO_FLAGS.FIF_STACKRANGE))
			{
				fi.m_addrMin = 0;
				fi.m_addrMax = 0x100;
				fi.m_dwValidFields |= enum_FRAMEINFO_FLAGS.FIF_STACKRANGE;
			}
			if (dwFieldSpec.HasFlag(enum_FRAMEINFO_FLAGS.FIF_FRAME))
			{
				fi.m_pFrame = this;
				fi.m_dwValidFields |= enum_FRAMEINFO_FLAGS.FIF_FRAME;
			}
			if (dwFieldSpec.HasFlag(enum_FRAMEINFO_FLAGS.FIF_DEBUGINFO))
			{
				fi.m_fHasDebugInfo = 0;
				fi.m_dwValidFields |= enum_FRAMEINFO_FLAGS.FIF_DEBUGINFO;
			}
			if (dwFieldSpec.HasFlag(enum_FRAMEINFO_FLAGS.FIF_STALECODE))
			{
				fi.m_fStaleCode = 0;
				fi.m_dwValidFields |= enum_FRAMEINFO_FLAGS.FIF_STALECODE;
			}
			pFrameInfo[0] = fi;
			return VSConstants.S_OK;
		}

		public int GetPhysicalStackRange(out ulong paddrMin, out ulong paddrMax)
		{
			throw new NotImplementedException();
		}

		public int GetExpressionContext(out IDebugExpressionContext2 ppExprCxt)
		{
			throw new NotImplementedException();
		}

		public int GetLanguageInfo(ref string pbstrLanguage, ref Guid pguidLanguage)
		{
			pbstrLanguage = "Unknown";
			pguidLanguage = Guid.Empty;
			return VSConstants.S_OK;
		}

		public int GetDebugProperty(out IDebugProperty2 ppProperty)
		{
			throw new NotImplementedException();
		}

		public int EnumProperties(enum_DEBUGPROP_INFO_FLAGS dwFields, uint nRadix, ref Guid guidFilter, uint dwTimeout, out uint pcelt, out IEnumDebugPropertyInfo2 ppEnum)
		{
			throw new NotImplementedException();
		}

		public int GetThread(out IDebugThread2 ppThread)
		{
			ppThread = Thread;
			return VSConstants.S_OK;
		}
	}
}
