using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Threading.Tasks;
using System.Xaml;
using Microsoft.Build.Framework.XamlTypes;
using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.ProjectSystem.Debug;
using Microsoft.VisualStudio.ProjectSystem.Properties;
using Microsoft.VisualStudio.ProjectSystem.VS.Debug;
using System.Linq;
using Microsoft.VisualStudio.Shell.Interop;

namespace FPGAProjectExtension
{

	//[Export(ExportContractNames.Scopes.ConfiguredProject, typeof(IProjectDynamicLoadComponent))]
	/*[ExportDebugger(MyDebugger.SchemaName)] // name of the schema from above
	[Export(typeof(IDebugLaunchProvider))]
	[AppliesTo("CSharp")]*/
	[Export(ExportContractNames.Scopes.ConfiguredProject, typeof(IProjectDynamicLoadComponent))]
	[ExportDebugger("MipsDebugger")]
	//[AppliesTo("!DisableBuiltInDebuggerServices")]
	//[PartMetadata("AppliesToIntentionallyInconsistent", true)]
	//[Order(orderPrecedence: 9999)]
	//[AppliesTo("VisualC")]
	// check IDynamicDebugTargetsGenerator 
	[AppliesTo("FPGADebuggerCapability")]
	public class FPGAProjectDebugLaunchProvider
		: DebugLaunchProviderBase,
		IProjectDynamicLoadComponent
	{
		// Code-generated type from compiling "XamlPropertyRule"
		private readonly ProjectProperties projectProperties;

		private ConfiguredProject m_configuredProject = null;

		[ImportMany(ExportContractNames.VsTypes.IVsHierarchy)]
		private OrderPrecedenceImportCollection<IVsHierarchy> IVsHierarchies { get; set; }

		[ImportingConstructor]
		public FPGAProjectDebugLaunchProvider(ConfiguredProject configuredProject, IAdditionalRuleDefinitionsService rds)
			: base(configuredProject)
		{
			IVsHierarchies = new OrderPrecedenceImportCollection<IVsHierarchy>(projectCapabilityCheckProvider: configuredProject.UnconfiguredProject);
			m_configuredProject = configuredProject;
			//this.projectProperties = projectProperties;
			bool b = configuredProject.Capabilities.AppliesTo("VisualC");
			bool b2 = configuredProject.Capabilities.AppliesTo("FPGADebuggerCapability");
			int qzdqzd = 0;
			Stream stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("XamlRuleToCode:MipsDebugger.xaml");
			Rule rule = ((IProjectSchemaNode)XamlServices.Load(stream)).GetSchemaObjects(typeof(Rule)).Cast<Rule>().FirstOrDefault();
			rds.AddRuleDefinition(rule, "Project");
		}

		// This is one of the methods of injecting rule xaml files into the project system.
		//[ExportPropertyXamlRuleDefinition("FPGAProjectExtension, Version=1.0.0.0, Culture=neutral, PublicKeyToken=9be6e469bc4921f1", "XamlRuleToCode:MipsDebugger.xaml", "Project")]
		[ExportPropertyXamlRuleDefinition("FPGAProjectExtension", "XamlRuleToCode:MipsDebugger.xaml", "Project")]
		//[AppliesTo("BuildWindowsDesktopTarget")]
		//[AppliesTo("VisualC")]
		[AppliesTo("FPGADebuggerCapability")]
		private object DebuggerXaml { get { throw new NotImplementedException(); } }

		public override Task<bool> CanLaunchAsync(DebugLaunchOptions launchOptions)
		{
			// perform any necessary logic to determine if the debugger can launch
			return Task.FromResult(true);
		}
		[System.Runtime.InteropServices.DllImport("ole32.Dll")]
		static public extern uint CoCreateInstance(
			ref Guid clsid,
			[System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.IUnknown)] object inner,
			uint context,
			ref Guid uuid,
			[System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.IUnknown)] out object rReturnedComObject);

		Guid MipsDebuggerEngineGuid = new Guid("23CCB575-0BF4-423C-B534-73B1AD053EBB");
		public override async Task<IReadOnlyList<IDebugLaunchSettings>> QueryDebugTargetsAsync(DebugLaunchOptions launchOptions)
		{
			//VSDebug.dll!CDebugger::LaunchTargets(struct VSDEBUG_RESULT *,unsigned long,struct _VsDebugTargetInfo4 *,struct _VsDebugTargetProcessInfo *)	Unknown
			//VSDebug.dll!CDefaultLaunchHook::OnLaunchDebugTargets(unsigned long,struct _VsDebugTargetInfo4 *,struct _VsDebugTargetProcessInfo *)	Unknown
			//[Managed to Native Transition]
			//VsGraphicsLaunchHookPkg.dll!Microsoft.VsPixPkg.XXX_DebugLaunchHook.OnLaunchDebugTargets(uint DebugTargetCount, Microsoft.VisualStudio.Shell.Interop.VsDebugTargetInfo4[] pDebugTargets, Microsoft.VisualStudio.Shell.Interop.VsDebugTargetProcessInfo[] pLaunchResults) Unknown
			//[Native to Managed Transition]
			//VSDebug.dll!CDebugger::LaunchTargetsThruHooks(struct VSDEBUG_RESULT *,unsigned long,struct _VsDebugTargetInfo4 *,struct _VsDebugTargetProcessInfo *)	Unknown
			//VSDebug.dll!CDebugger::LaunchDebugTargets4(unsigned long,struct _VsDebugTargetInfo4 *,struct _VsDebugTargetProcessInfo *)	Unknown
			//[Managed to Native Transition]
			//Microsoft.VisualStudio.ProjectSystem.VS.dll!Microsoft.VisualStudio.ProjectSystem.VS.Debug.DebugLaunchProviderBase.LaunchAsync(Microsoft.VisualStudio.ProjectSystem.VS.Debug.IDebugLaunchSettings[] launchSettings) Unknown

			//object comObj = null;
			//Guid clsid = new Guid("E13B43AC-F1B2-4636-A20A-364ACB053A04");
			//Guid interfaceid = typeof(Microsoft.VisualStudio.Debugger.Interop.IDebugEngine2).GUID;
			//uint err = CoCreateInstance(ref clsid, null, 1, ref interfaceid, out comObj);
#if DEBUG
			// This is necessary because experimental instance uses a hive. The debug engine needs to be registered in the hive
			// but the COM components must also be registered in the real registry
			//DebugEngine.MipsDebugEngine.RegisterFunction(typeof(DebugEngine.MipsDebugEngine));
#endif
			var settings = new DebugLaunchSettings(launchOptions);

			//settings.LaunchDebugEngineGuid = MyDebuggerEngineGuid; // Microsoft.VisualStudio.ProjectSystem.Debug.DebuggerEngines has some well known engines
			settings.LaunchDebugEngineGuid = MipsDebuggerEngineGuid;
			//settings.LaunchDebugEngineGuid = Guid.Empty;// DebuggerEngines.NativeOnlyEngine;

			/*
			 * If using a PortSupplier, LaunchDebugEngineGuid must be empty.
			 * If you use DebuggerEngines.NativeOnlyEngine, the IDebugPort2 implementation
			 * is expected to support a custom interface (96E1374B-33E5-4128-9F22-499D00E94ABA I think?)
			 * and it fails.
			 * 
			 * Actually even when it's empty, it still fails and IDK why.
			 */
			settings.AppPackageLaunchInfo = new VsAppPackageLaunchInfo();
			settings.Arguments = "";
			settings.CurrentDirectory = @"";
			settings.Executable = @"";
			settings.LaunchOperation = DebugLaunchOperation.CreateProcess;
			settings.LaunchOptions = launchOptions;
			settings.Options = "";
			settings.PortName = "";
			settings.PortSupplierGuid = Guid.Empty;
			//settings.PortSupplierGuid = MyDebuggerPortSupplierGuid;
			settings.ProcessId = 0;
			settings.ProcessLanguageGuid = Guid.Empty;
			settings.Project = IVsHierarchies.FirstOrDefault()?.Value;
			settings.RemoteMachine = "";
			settings.SendToOutputWindow = false;
			settings.StandardErrorHandle = IntPtr.Zero;
			settings.StandardInputHandle = IntPtr.Zero;
			settings.StandardOutputHandle = IntPtr.Zero;

			try
			{
				//LaunchAsync(settings);
			}
			catch (Exception ex)
			{
				int qzdq = 0;
			}

			//Microsoft.VisualStudio.Shell.Interop.IVsDebugger4 service = ((IServiceProvider)this.ServiceProvider).GetService(typeof(Microsoft.VisualStudio.Shell.Interop.SVsShellDebugger)) as Microsoft.VisualStudio.Shell.Interop.IVsDebugger4;
			// you can get your xaml properties via:
			//var debuggerProperties = await this.projectProperties.GetMyDebuggerPropertiesAsync();

			//return Task.FromResult<IReadOnlyList<IDebugLaunchSettings>>(new IDebugLaunchSettings[] { settings });
			return new IDebugLaunchSettings[] { settings };
		}
		//[Import]
		//IAdditionalRuleDefinitionsService AdditionaRuleDefinitionsService { get; }

		public async Task LoadAsync()
		{
			
		}
		public async Task UnloadAsync()
		{
		}
	}
}