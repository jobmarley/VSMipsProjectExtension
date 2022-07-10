using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace VSMipsProjectExtension
{
	public class VSComHelper
	{
		public class ActivationContext
			: IDisposable
		{
			public class ActivationContextGuard
				: IDisposable
			{
				private bool disposedValue;
				private ActivationContext m_context = null;
				private IntPtr m_cookie = IntPtr.Zero;

				public ActivationContextGuard(ActivationContext context)
				{
					m_context = context;
					if (!context.Activate(out m_cookie))
						throw new Exception();
				}
				protected virtual void Dispose(bool disposing)
				{
					if (!disposedValue)
					{
						if (disposing)
						{
							// TODO: dispose managed state (managed objects)
						}

						// TODO: free unmanaged resources (unmanaged objects) and override finalizer
						if (m_cookie != IntPtr.Zero)
						{
							m_context.Deactivate(0, m_cookie);
							m_cookie = IntPtr.Zero;
						}
						// TODO: set large fields to null
						disposedValue = true;
					}
				}

				// // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
				~ActivationContextGuard()
				{
					// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
					Dispose(disposing: false);
				}

				public void Dispose()
				{
					// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
					Dispose(disposing: true);
					GC.SuppressFinalize(this);
				}
			}

			private bool disposedValue;
			private IntPtr m_hActCtx = IntPtr.Zero;

			[StructLayout(LayoutKind.Sequential, Pack = 1)]
			struct PCACTCTXA
			{
				public uint cbSize;
				public uint dwFlags;
				[MarshalAs(UnmanagedType.LPStr)]
				public string lpSource;
				public ushort wProcessorArchitecture;
				public ushort wLangId;
				[MarshalAs(UnmanagedType.LPStr)]
				public string lpAssemblyDirectory;
				[MarshalAs(UnmanagedType.LPStr)]
				public string lpResourceName;
				[MarshalAs(UnmanagedType.LPStr)]
				public string lpApplicationName;
				public IntPtr hModule;
			};

			[DllImport("Kernel32.dll", CharSet = CharSet.Ansi)]
			extern static IntPtr CreateActCtxA(ref PCACTCTXA pActCtx);
			[DllImport("Kernel32.dll", CharSet = CharSet.Ansi)]
			extern static bool ActivateActCtx(IntPtr hActCtx, out IntPtr lpCookie);
			[DllImport("Kernel32.dll", CharSet = CharSet.Ansi)]
			extern static void ReleaseActCtx(IntPtr hActCtx);
			[DllImport("Kernel32.dll", CharSet = CharSet.Ansi)]
			extern static bool DeactivateActCtx(uint dwFlags, IntPtr ulCookie);
			public ActivationContext(string filename)
			{
				string asmFilepath = System.Reflection.Assembly.GetExecutingAssembly().Location;
				string manifestFilepath = System.IO.Path.Combine(
					System.IO.Path.GetDirectoryName(asmFilepath),
					System.IO.Path.GetFileNameWithoutExtension(asmFilepath) + ".manifest");
				PCACTCTXA act = new PCACTCTXA();
				act.cbSize = (uint)Marshal.SizeOf(act); // 52
				act.lpSource = manifestFilepath;
				act.lpAssemblyDirectory = System.IO.Path.GetDirectoryName(asmFilepath);
				//act.dwFlags = 0x4 | 0x32;
				m_hActCtx = CreateActCtxA(ref act);
			}

			public bool Activate(out IntPtr cookie)
			{
				return ActivateActCtx(m_hActCtx, out cookie);
			}
			public ActivationContextGuard Activate()
			{
				return new ActivationContextGuard(this);
			}
			public bool Deactivate(uint dwFlags, IntPtr ulCookie)
			{
				return DeactivateActCtx(dwFlags, ulCookie);
			}
			protected virtual void Dispose(bool disposing)
			{
				if (!disposedValue)
				{
					if (disposing)
					{
						// TODO: dispose managed state (managed objects)
					}

					// TODO: free unmanaged resources (unmanaged objects) and override finalizer
					if (m_hActCtx != IntPtr.Zero)
					{
						ReleaseActCtx(m_hActCtx);
						m_hActCtx = IntPtr.Zero;
					}
					// TODO: set large fields to null
					disposedValue = true;
				}
			}

			~ActivationContext()
			{
				// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
				Dispose(disposing: false);
			}

			public void Dispose()
			{
				// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
				Dispose(disposing: true);
				GC.SuppressFinalize(this);
			}
		}
	}
}
