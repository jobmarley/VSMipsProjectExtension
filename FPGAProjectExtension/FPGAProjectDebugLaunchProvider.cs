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
		private OrderPrecedenceImportCollection<IVsHierarchy> IVsHierarchies { get; set; } = null;

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

		Guid MipsDebugEngineGuid = new Guid("23CCB575-0BF4-423C-B534-73B1AD053EBB");
		Guid MipsDebugPortSupplierGuid = new Guid("76EE65B4-DFC2-44A7-88D7-52E26DA2E2BE");
		public override async Task<IReadOnlyList<IDebugLaunchSettings>> QueryDebugTargetsAsync(DebugLaunchOptions launchOptions)
		{
			var settings = new DebugLaunchSettings(launchOptions);

			//settings.LaunchDebugEngineGuid = DebuggerEngines.NativeOnlyEngine; // Microsoft.VisualStudio.ProjectSystem.Debug.DebuggerEngines has some well known engines
			settings.LaunchDebugEngineGuid = MipsDebugEngineGuid;
			//settings.LaunchDebugEngineGuid = Guid.Empty;

			string targetPath = "";
			string serverName = "";
			string port = "";
			string additionalFilesToLoad = "";

			// I think this is the right way to get evaluated properties
			await ConfiguredProject.Services.ProjectLockService.ReadLockAsync(
				async access =>
				{
					Microsoft.Build.Evaluation.Project project = await access.GetProjectAsync(ConfiguredProject);
					// Executable & CurrentDirectory should never be null, or the engine isnt launched
					targetPath = project.GetPropertyValue("TargetPath");
					serverName = project.GetPropertyValue("ServerName");
					port = project.GetPropertyValue("ServerPort");
					additionalFilesToLoad = project.GetPropertyValue("AdditionalFilesToLoad");
				});

			settings.Executable = additionalFilesToLoad;
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
			settings.LaunchOperation = DebugLaunchOperation.CreateProcess;
			settings.LaunchOptions = launchOptions;
			settings.Options = "";
			settings.PortName = serverName + ":" + port;
			//settings.PortSupplierGuid = Guid.Empty;
			settings.PortSupplierGuid = MipsDebugPortSupplierGuid;
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