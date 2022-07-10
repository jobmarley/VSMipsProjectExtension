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

namespace VSMipsProjectExtension
{
	[Export(ExportContractNames.Scopes.ConfiguredProject, typeof(IProjectDynamicLoadComponent))]
	[ExportDebugger("MipsDebugger")]
	[AppliesTo("MipsDebuggerCapability")]
	public class MipsProjectDebugLaunchProvider
		: DebugLaunchProviderBase,
		IProjectDynamicLoadComponent
	{
		// Code-generated type from compiling "XamlPropertyRule"
		private readonly ProjectProperties projectProperties;

		private ConfiguredProject m_configuredProject = null;

		[ImportMany(ExportContractNames.VsTypes.IVsHierarchy)]
		private OrderPrecedenceImportCollection<IVsHierarchy> IVsHierarchies { get; set; } = null;

		[ImportingConstructor]
		public MipsProjectDebugLaunchProvider(ConfiguredProject configuredProject, IAdditionalRuleDefinitionsService rds)
			: base(configuredProject)
		{
			IVsHierarchies = new OrderPrecedenceImportCollection<IVsHierarchy>(projectCapabilityCheckProvider: configuredProject.UnconfiguredProject);
			m_configuredProject = configuredProject;
			Stream stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("XamlRuleToCode:MipsDebugger.xaml");
			Rule rule = ((IProjectSchemaNode)XamlServices.Load(stream)).GetSchemaObjects(typeof(Rule)).Cast<Rule>().FirstOrDefault();
			rds.AddRuleDefinition(rule, "Project");
		}

		// This is one of the methods of injecting rule xaml files into the project system.
		[ExportPropertyXamlRuleDefinition("VSMipsProjectExtension", "XamlRuleToCode:MipsDebugger.xaml", "Project")]
		[AppliesTo("MipsDebuggerCapability")]
		private object DebuggerXaml { get { throw new NotImplementedException(); } }

		public override Task<bool> CanLaunchAsync(DebugLaunchOptions launchOptions)
		{
			return Task.FromResult(true);
		}

		Guid MipsDebugEngineGuid = new Guid("23CCB575-0BF4-423C-B534-73B1AD053EBB");
		Guid MipsDebugPortSupplierGuid = new Guid("76EE65B4-DFC2-44A7-88D7-52E26DA2E2BE");
		public override async Task<IReadOnlyList<IDebugLaunchSettings>> QueryDebugTargetsAsync(DebugLaunchOptions launchOptions)
		{
			var settings = new DebugLaunchSettings(launchOptions);

			settings.LaunchDebugEngineGuid = MipsDebugEngineGuid;

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

			settings.AppPackageLaunchInfo = new VsAppPackageLaunchInfo();
			settings.Arguments = "";
			settings.LaunchOperation = DebugLaunchOperation.CreateProcess;
			settings.LaunchOptions = launchOptions;
			settings.Options = "";
			settings.PortName = serverName + ":" + port;
			settings.PortSupplierGuid = MipsDebugPortSupplierGuid;
			settings.ProcessId = 0;
			settings.ProcessLanguageGuid = Guid.Empty;
			settings.Project = IVsHierarchies.FirstOrDefault()?.Value;
			settings.RemoteMachine = "";
			settings.SendToOutputWindow = false;
			settings.StandardErrorHandle = IntPtr.Zero;
			settings.StandardInputHandle = IntPtr.Zero;
			settings.StandardOutputHandle = IntPtr.Zero;

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