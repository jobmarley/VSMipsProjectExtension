using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using MipsRemoteDebuggerUtils;

namespace MipsRemoteDebugger
{
	internal class mipsdebug_api
	{

		public delegate void md_callback(md_event e);

		[DllImport("mipsdebug.dll", CallingConvention = CallingConvention.StdCall)]
		public static extern md_status md_open(out IntPtr device);
		[DllImport("mipsdebug.dll", CallingConvention = CallingConvention.StdCall)]
		public static extern md_status md_register_callback(IntPtr device, md_callback c);
		[DllImport("mipsdebug.dll", CallingConvention = CallingConvention.StdCall)]
		public static extern md_status md_unregister_callback(IntPtr device, md_callback c);
		[DllImport("mipsdebug.dll", CallingConvention = CallingConvention.StdCall)]
		public static extern md_status md_read_memory(IntPtr device, [MarshalAs(UnmanagedType.LPArray)] byte[] buffer, uint count, uint offset, out uint readcount);
		[DllImport("mipsdebug.dll", CallingConvention = CallingConvention.StdCall)]
		public static extern md_status md_write_memory(IntPtr device, [MarshalAs(UnmanagedType.LPArray)] byte[] buffer, uint count, uint offset, out uint writtencount);
		[DllImport("mipsdebug.dll", CallingConvention = CallingConvention.StdCall)]
		public static extern md_status md_read_register(IntPtr device, md_register r, out uint value);
		[DllImport("mipsdebug.dll", CallingConvention = CallingConvention.StdCall)]
		public static extern md_status md_write_register(IntPtr device, md_register r, uint value);
		[DllImport("mipsdebug.dll", CallingConvention = CallingConvention.StdCall)]
		public static extern md_status md_get_state(IntPtr device, out md_state state);
		[DllImport("mipsdebug.dll", CallingConvention = CallingConvention.StdCall)]
		public static extern md_status md_set_state(IntPtr device, md_state state);
		[DllImport("mipsdebug.dll", CallingConvention = CallingConvention.StdCall)]
		public static extern void md_close(IntPtr device);
	}
}
