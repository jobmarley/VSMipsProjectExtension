using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Task = System.Threading.Tasks.Task;

namespace FPGAProjectExtension
{
	public static class TypeInformation
	{
		public static string GetTypeName(object comObject)
		{
			var dispatch = comObject as IDispatch;

			if (dispatch == null)
			{
				return null;
			}

			var pTypeInfo = dispatch.GetTypeInfo(0, 1033);

			string pBstrName;
			string pBstrDocString;
			int pdwHelpContext;
			string pBstrHelpFile;
			pTypeInfo.GetDocumentation(
				-1,
				out pBstrName,
				out pBstrDocString,
				out pdwHelpContext,
				out pBstrHelpFile);

			string str = pBstrName;
			if (str[0] == 95)
			{
				// remove leading '_'
				str = str.Substring(1);
			}

			return str;
		}

		[ComImport]
		[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
		[Guid("00020400-0000-0000-C000-000000000046")]
		private interface IDispatch
		{
			int GetTypeInfoCount();

			[return: MarshalAs(UnmanagedType.Interface)]
			ITypeInfo GetTypeInfo(
				[In, MarshalAs(UnmanagedType.U4)] int iTInfo,
				[In, MarshalAs(UnmanagedType.U4)] int lcid);

			void GetIDsOfNames(
				[In] ref Guid riid,
				[In, MarshalAs(UnmanagedType.LPArray)] string[] rgszNames,
				[In, MarshalAs(UnmanagedType.U4)] int cNames,
				[In, MarshalAs(UnmanagedType.U4)] int lcid,
				[Out, MarshalAs(UnmanagedType.LPArray)] int[] rgDispId);
		}
	}

	/// <summary>
	/// Command handler
	/// </summary>
	internal sealed class Command1
		: System.Windows.Input.ICommand,
		IVsUIDataSourcePropertyChangeEvents
	{
		/// <summary>
		/// Command ID.
		/// </summary>
		public const int CommandId = 0x0100;

		/// <summary>
		/// Command menu group (command set GUID).
		/// </summary>
		public static readonly Guid CommandSet = new Guid("5b8053a5-d17b-4f99-8c47-a4906b2fa884");

		/// <summary>
		/// VS Package that provides this command, not null.
		/// </summary>
		private readonly AsyncPackage package;

		public event EventHandler CanExecuteChanged;

		/// <summary>
		/// Initializes a new instance of the <see cref="Command1"/> class.
		/// Adds our command handlers for menu (commands must exist in the command table file)
		/// </summary>
		/// <param name="package">Owner package, not null.</param>
		/// <param name="commandService">Command service to add command to, not null.</param>
		private Command1(AsyncPackage package, OleMenuCommandService commandService)
		{
			this.package = package ?? throw new ArgumentNullException(nameof(package));
			commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

			var menuCommandID = new CommandID(CommandSet, CommandId);
			var menuItem = new MenuCommand(this.Execute, menuCommandID);
			commandService.AddCommand(menuItem);
		}

		/// <summary>
		/// Gets the instance of the command.
		/// </summary>
		public static Command1 Instance
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets the service provider from the owner package.
		/// </summary>
		private Microsoft.VisualStudio.Shell.IAsyncServiceProvider ServiceProvider
		{
			get
			{
				return this.package;
			}
		}

		/// <summary>
		/// Initializes the singleton instance of the command.
		/// </summary>
		/// <param name="package">Owner package, not null.</param>
		public static async Task InitializeAsync(AsyncPackage package)
		{
			// Switch to the main thread - the call to AddCommand in Command1's constructor requires
			// the UI thread.
			await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

			OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
			Instance = new Command1(package, commandService);
		}


		DependencyObject GetControlFromIndexList(DependencyObject parent, IEnumerable<int> l)
		{
			if (l == null || !l.Any())
				return parent;
			return GetControlFromIndexList(VisualTreeHelper.GetChild(parent, l.First()), l.Skip(1));
		}

		void OnCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			int zqdqd = 0;
		}
		object m_lastMenuController = null;
		System.Windows.Input.ICommand m_lastCommand = null;
		object m_lastAssociatedDataSource = null;
		object m_lastInnerSource = null;
		/// <summary>
		/// This function is the callback used to execute the command when the menu item is clicked.
		/// See the constructor to see how the menu item is associated with this function using
		/// OleMenuCommandService service and MenuCommand class.
		/// </summary>
		/// <param name="sender">Event sender.</param>
		/// <param name="e">Event args.</param>
		private void Execute(object sender, EventArgs e)
		{
			ThreadHelper.ThrowIfNotOnUIThread();

			List<int> indexes = new List<int>()
			{
				0, 0, 0, //(RootGrid)
				2, 0, 0, //(draggabledockpanel)
				1, 0, 0, //(VsToolbar)
				0, 1, 0, 0, 2, //(Part_toolbarpanel)
				15, //(VsButton)
				0, 0, 0, 0, //(PART_FocusTarget)
				0, 0, 1, 0 //(VsMenuControllerButton)
			};
			List<int> indexesController = new List<int>()
			{
				0, 0, 0, //(RootGrid)
				2, 0, 0, //(draggabledockpanel)
				1, 0, 0, //(VsToolbar)
				0, 1, 0, 0, 2, //(Part_toolbarpanel)
				15, //(VsButton)
				0, 0, 0, 0, //(PART_FocusTarget)
			};
			var debugButton = GetControlFromIndexList(System.Windows.Application.Current.MainWindow, indexes) as System.Windows.Controls.Button;
			var menuController = GetControlFromIndexList(System.Windows.Application.Current.MainWindow, indexesController) as System.Windows.Controls.MenuItem;
			menuController.Items.CurrentChanged += (s, ee) =>
			{
				int qqqd = 0;
			};
			if (menuController != m_lastMenuController)
			{
				int qzdjjqd = 0;
			}
			m_lastMenuController = menuController;
			(menuController.Items as System.Windows.Data.CollectionView as System.Collections.Specialized.INotifyCollectionChanged).CollectionChanged += OnCollectionChanged;
			//debugButton.Command = this;
			System.Windows.Input.ICommand c = debugButton.Command;
			if (m_lastCommand != c)
			{
				int qzdqzd = 0;
			}
			m_lastCommand = c;
			object associatedDataSource = c.GetType().GetField("associatedDataSource", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(c);
			if (associatedDataSource != m_lastAssociatedDataSource)
			{
				int qzdgggqd = 0;
			}
			m_lastAssociatedDataSource = associatedDataSource;
			object innerSource = associatedDataSource.GetType().BaseType.GetField("innerSource", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(associatedDataSource);
			if (innerSource != m_lastInnerSource)
			{
				int qzhhhhdqd = 0;
			}

			m_lastInnerSource = innerSource;
			IVsUIDataSource qzdqzq = innerSource as IVsUIDataSource;
			uint cookie = 0;
			int err = qzdqzq.AdvisePropertyChangeEvents(this, out cookie);
			//string s = TypeInformation.GetTypeName(innerSource);
			//HackInterop.ComHack.DumpComInfo((IVsUIDataSource)innerSource);
			int zqdzd = 0;
			/*string message = string.Format(CultureInfo.CurrentCulture, "Inside {0}.MenuItemCallback()", this.GetType().FullName);
			string title = "Command1";

			// Show a message box to prove we were here
			VsShellUtilities.ShowMessageBox(
				this.package,
				message,
				title,
				OLEMSGICON.OLEMSGICON_INFO,
				OLEMSGBUTTON.OLEMSGBUTTON_OK,
				OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);*/
		}

		public bool CanExecute(object parameter)
		{
			//throw new NotImplementedException();
			return true;
		}

		public void Execute(object parameter)
		{
			throw new NotImplementedException();
		}

		public int Disconnect(IVsUISimpleDataSource pSource)
		{
			return 0;
		}

		public int OnPropertyChanged(IVsUIDataSource pDataSource, string prop, IVsUIObject pVarOld, IVsUIObject pVarNew)
		{
			return 0;
		}
	}
}
