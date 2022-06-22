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
	[ExportDebugger("MyDebugger")]
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
			Stream stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("XamlRuleToCode:MyDebugger.xaml");
			Rule rule = ((IProjectSchemaNode)XamlServices.Load(stream)).GetSchemaObjects(typeof(Rule)).Cast<Rule>().FirstOrDefault();
			rds.AddRuleDefinition(rule, "Project");
		}

		// This is one of the methods of injecting rule xaml files into the project system.
		//[ExportPropertyXamlRuleDefinition("FPGAProjectExtension, Version=1.0.0.0, Culture=neutral, PublicKeyToken=9be6e469bc4921f1", "XamlRuleToCode:MyDebugger.xaml", "Project")]
		[ExportPropertyXamlRuleDefinition("FPGAProjectExtension", "XamlRuleToCode:MyDebugger.xaml", "Project")]
		//[AppliesTo("BuildWindowsDesktopTarget")]
		//[AppliesTo("VisualC")]
		[AppliesTo("FPGADebuggerCapability")]
		private object DebuggerXaml { get { throw new NotImplementedException(); } }

		public override Task<bool> CanLaunchAsync(DebugLaunchOptions launchOptions)
		{
			// perform any necessary logic to determine if the debugger can launch
			return Task.FromResult(true);
		}

		Guid MyDebuggerPortSupplierGuid = new Guid("4D9035C6-9DFB-4FCA-AE14-86E396E05DC1");//new Guid("2572f753-9309-4246-8c65-58bd4d427ce2");
		Guid MyDebuggerEngineGuid = new Guid("C3198026-B726-469D-996A-4CDF46BF4A49");
		public override async Task<IReadOnlyList<IDebugLaunchSettings>> QueryDebugTargetsAsync(DebugLaunchOptions launchOptions)
		{
			var settings = new DebugLaunchSettings(launchOptions);

			settings.LaunchDebugEngineGuid = MyDebuggerEngineGuid; // Microsoft.VisualStudio.ProjectSystem.Debug.DebuggerEngines has some well known engines
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
			settings.PortName = "baba";
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