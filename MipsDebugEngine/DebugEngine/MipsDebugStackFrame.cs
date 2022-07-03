using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;

namespace FPGAProjectExtension.DebugEngine
{
	class MipsDebugExpression
		: IDebugExpression2
	{
		IDebugParsedExpression m_parsedExpression = null;
		public MipsDebugExpression(IDebugParsedExpression parsedExpression)
		{
			m_parsedExpression = parsedExpression;
		}
		public int EvaluateAsync(enum_EVALFLAGS dwFlags, IDebugEventCallback2 pExprCallback)
		{
			throw new NotImplementedException();
		}

		public int Abort()
		{
			throw new NotImplementedException();
		}

		public int EvaluateSync(enum_EVALFLAGS dwFlags, uint dwTimeout, IDebugEventCallback2 pExprCallback, out IDebugProperty2 ppResult)
		{
			throw new NotImplementedException();
		}
	}
	class MipsDebugAddress
		: IDebugAddress
	{
		public MipsDebugAddress()
		{

		}
		public int GetAddress(DEBUG_ADDRESS[] pAddress)
		{
			throw new NotImplementedException();
		}
	}
	class MipsDebugExpressionContext
		: IDebugExpressionContext2
	{
		IDebugExpressionEvaluator2 m_expressionEvaluator = null;
		public MipsDebugExpressionContext(IDebugExpressionEvaluator2 expressionEvaluator)
		{
			m_expressionEvaluator = expressionEvaluator;
		}
		public int GetName(out string pbstrName)
		{
			pbstrName = "";
			return VSConstants.S_OK;
		}

		public int ParseText(string pszCode, enum_PARSEFLAGS dwFlags, uint nRadix, out IDebugExpression2 ppExpr, out string pbstrError, out uint pichError)
		{
			ppExpr = null;
			pbstrError = "";
			pichError = 0;
			IDebugExpressionEvaluator3 ee3 = (IDebugExpressionEvaluator3)m_expressionEvaluator;
			IDebugParsedExpression parsedExpression = null;
			int err = ee3.Parse2(pszCode, dwFlags, nRadix, new MipsDebugSymbolProvider(), new MipsDebugAddress(), out pbstrError, out pichError, out parsedExpression);
			if (err != VSConstants.S_OK)
				return err;

			ppExpr = new MipsDebugExpression(parsedExpression);
			return VSConstants.S_OK;
		}
	}
	internal class MipsDebugStackFrame
		: IDebugStackFrame2
	{
		public uint Address { get; } = 0;
		public MipsDebugThread Thread { get; } = null;
		IElfSymbolProvider m_symbolProvider = null;
		IDebugDocumentContext2 m_documentContext = null;
		public MipsDebugStackFrame(MipsDebugThread thread, uint address, IElfSymbolProvider symbolProvider)
		{
			Thread = thread;
			Address = address;
			m_symbolProvider = symbolProvider;

			IDebugAddress debugAddress = null;
			int hr = m_symbolProvider.GetAddressFromMemory(Address, out debugAddress);
			if (hr != VSConstants.S_OK)
				throw new Exception();

			hr = m_symbolProvider.GetContextFromAddress(debugAddress, out m_documentContext);
			if (hr != VSConstants.S_OK)
				throw new Exception();
		}
		public int GetCodeContext(out IDebugCodeContext2 ppCodeCxt)
		{
			ppCodeCxt = null;
			return VSConstants.E_FAIL;
		}

		public int GetDocumentContext(out IDebugDocumentContext2 ppCxt)
		{
			ppCxt = m_documentContext;
			return VSConstants.S_OK;
		}

		public int GetName(out string pbstrName)
		{
			pbstrName = string.Format("{0:X8}", Address);
			return VSConstants.S_OK;
		}

		MipsDebugModule GetModuleFromAddress(uint address)
		{
			return Thread?.Program?.Modules.FirstOrDefault(x => (address >= x.Address) && (address < x.Address + x.Size));
		}
		public int GetInfo(enum_FRAMEINFO_FLAGS dwFieldSpec, uint nRadix, FRAMEINFO[] pFrameInfo)
		{
			if (pFrameInfo == null || pFrameInfo.Length != 1)
				return VSConstants.E_INVALIDARG;

			FRAMEINFO fi = new FRAMEINFO();
			if (dwFieldSpec.HasFlag(enum_FRAMEINFO_FLAGS.FIF_FUNCNAME))
			{
				MipsDebugModule module = null;
				if (dwFieldSpec.HasFlag(enum_FRAMEINFO_FLAGS.FIF_FUNCNAME_MODULE))
					module = GetModuleFromAddress(Address);

				if (module != null)
				{
					fi.m_bstrFuncName = string.Format("{0}!{1:X8}()", System.IO.Path.GetFileName(module.Filepath), Address);
					fi.m_dwValidFields |= enum_FRAMEINFO_FLAGS.FIF_FUNCNAME;
				}
				else
				{
					fi.m_bstrFuncName = string.Format("{0:X8}()", Address);
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
				Guid langGuid = Guid.Empty;
				if (GetLanguageInfo(ref fi.m_bstrLanguage, ref langGuid) != VSConstants.S_OK)
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
			ppExprCxt = null;
			return VSConstants.E_NOTIMPL;
		}

		public int GetLanguageInfo(ref string pbstrLanguage, ref Guid pguidLanguage)
		{
			if (m_documentContext != null)
				return m_documentContext.GetLanguageInfo(ref pbstrLanguage, ref pguidLanguage);

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
