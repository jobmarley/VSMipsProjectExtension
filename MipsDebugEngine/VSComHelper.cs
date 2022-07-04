using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace FPGAProjectExtension
{
	/*
	 * That shit is ugly, but thats a way to load a non registered COM component.
	 * I tried to use it as multithreaded and it didnt work (IID not registered in combase when calling functions on the component)
	 * Maybe because it doesnt have the registry settings.
	 * Another way would be to use Side-by-side + activation context, this should allow to set the settings properly
	 */

	//// Create an activation context
	//Type actCtxType = System.Type.GetTypeFromProgID("Microsoft.Windows.ActCtx");
	//dynamic actCtx = System.Activator.CreateInstance(actCtxType);
	//actCtx.Manifest = @"Path\To\COMClient.manifest";

	//// Create the object you want, using the activation context
	//dynamic obj = actCtx.CreateObject("COMTestService.COMTestObject");

	//// Now use it!
	//var question = obj.GetQuestionFromAnswer(42);

	public class VSComHelper
	{
		[DllImport("Kernel32.dll", CharSet = CharSet.Unicode)]
		extern static IntPtr LoadLibraryW(string name);
		[DllImport("Kernel32.dll", CharSet = CharSet.Ansi)]
		extern static IntPtr GetProcAddress(IntPtr hModule, string lpProcName);
		[DllImport("Kernel32.dll", CharSet = CharSet.Unicode)]
		extern static bool SetDllDirectoryW(string lpPathName);
		delegate int GetClassObjectDelegate(ref Guid rclsid, ref Guid riid, [MarshalAs(UnmanagedType.IUnknown)] out object ppv);
		// Necessary because Type.GetTypeFromCLSID + Activator.CreateInstance cannot create objects registered with CLSID in the hive
		public static T CreateFromCLSID<T>(string registryRoot, Guid guid)
		{
			return Microsoft.VisualStudio.Shell.ThreadHelper.JoinableTaskFactory.Run(async () =>
			{
				await Microsoft.VisualStudio.Shell.ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
				Microsoft.Win32.RegistryKey Key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(string.Format("{0}\\CLSID\\{{{1}}}\\InprocServer32", registryRoot, guid));
				string libraryPath = (string)Key.GetValue(null);
				SetDllDirectoryW(System.IO.Path.GetDirectoryName(libraryPath));
				IntPtr hModule = LoadLibraryW(libraryPath);
				SetDllDirectoryW(null);
				IntPtr functionPtr = GetProcAddress(hModule, "DllGetClassObject");
				GetClassObjectDelegate d = Marshal.GetDelegateForFunctionPointer<GetClassObjectDelegate>(functionPtr);
				Guid clsid = guid;
				Guid unknowniid = new Guid("00000000-0000-0000-C000-000000000046");
				object unkfactory = null;
				int err = d(ref clsid, ref unknowniid, out unkfactory);
				if (err != 0)
					throw new COMException("", err);
				IClassFactory factory = (IClassFactory)unkfactory;

				object o;
				Guid iid = typeof(T).GUID;
				err = factory.CreateInstance(null, ref iid, out o);
				if (err != 0)
					throw new COMException("", err);
				return (T)o;
			});
		}

		[ComImport]
		[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
		[Guid("00000001-0000-0000-C000-000000000046")]
		interface IClassFactory
		{
			int CreateInstance([MarshalAs(UnmanagedType.IUnknown)] object pUnkOuter, ref Guid riid, [MarshalAs(UnmanagedType.IUnknown)] out object ppvObject);

			int LockServer(bool fLock);
		}
	}
}
