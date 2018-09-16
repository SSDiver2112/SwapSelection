using System;
using System.ComponentModel.Design;
using Microsoft;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using Task = System.Threading.Tasks.Task;

namespace SwapSelection
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class CommandSwap
    {
        private IWpfTextView m_textView;
        private ITextBuffer _buffer;

        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0100;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("92a816fe-90f6-4d55-86e7-fec2684b5a74");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly AsyncPackage package;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandSwap"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private CommandSwap(AsyncPackage package, OleMenuCommandService commandService)
        {
            //Removed Throws to make AppVeyor work
            this.package = package;//?? throw new ArgumentNullException(nameof(package));
                                   // commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new OleMenuCommand(this.Execute, menuCommandID);

             menuItem.BeforeQueryStatus += MyQueryStatus;
            commandService.AddCommand(menuItem);
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static CommandSwap Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private IServiceProvider ServiceProvider
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
            // Switch to the main thread - the call to AddCommand in CommandSwap's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync((typeof(IMenuCommandService))) as OleMenuCommandService;
            Instance = new CommandSwap(package, commandService);
        }

        private  void MyQueryStatus(object sender, EventArgs e)
        {
            OleMenuCommand button = (OleMenuCommand)sender;
            button.Visible = ValidateSelectionAsync();
        }

        private bool ValidateSelectionAsync()
        {
            m_textView = GetCurrentTextView();

            var mItems = m_textView.Selection.SelectedSpans;

            return mItems.Count == 2 && (mItems[0].GetText().Length > 0 && mItems[1].GetText().Length > 0);
        }

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
            ExecSwap();
        }

        private void ExecSwap()
        {
            m_textView = GetCurrentTextView();
            _buffer = m_textView.TextBuffer;

            var mItems = m_textView.Selection.SelectedSpans;
            if(mItems.Count == 2 && (mItems[0].GetText().Length > 0 && mItems[1].GetText().Length > 0))
            {
                var selected1 = mItems[0].GetText();
                var selected2 = mItems[1].GetText();
                var textEdit = _buffer.CreateEdit();
                textEdit.Replace(mItems[0], selected2);
                textEdit.Replace(mItems[1], selected1);
                textEdit.Apply();
            }
        }

        public IWpfTextView GetCurrentTextView()
        {
            return GetTextView();
        }

        public IWpfTextView GetTextView()
        {
            var compService = ServiceProvider.GetService(typeof(SComponentModel)) as IComponentModel;
            Assumes.Present(compService);
            IVsEditorAdaptersFactoryService editorAdapter = compService.GetService<IVsEditorAdaptersFactoryService>();
            return editorAdapter.GetWpfTextView(GetCurrentNativeTextView());
        }

        public IVsTextView GetCurrentNativeTextView()
        {
            var textManager = (IVsTextManager)ServiceProvider.GetService(typeof(SVsTextManager));
            Assumes.Present(textManager);
            ErrorHandler.ThrowOnFailure(textManager.GetActiveView(1, null, out IVsTextView activeView));
            return activeView;
        }
    }
}
