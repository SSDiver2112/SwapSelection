using System;
using System.ComponentModel.Design;
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

            // menuItem.BeforeQueryStatus += MyQueryStatusAsync;
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
            // Switch to the main thread - the call to AddCommand in CommandSwap's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync((typeof(IMenuCommandService))) as OleMenuCommandService;
            Instance = new CommandSwap(package, commandService);
        }


        //TODO: Not always working before menu opens
        //private async void MyQueryStatusAsync(object sender, EventArgs e)
        //{
        //    OleMenuCommand button = (OleMenuCommand)sender;
        //    button.Visible = await ValidateSelectionAsync();
        //}

        //private async Task<bool> ValidateSelectionAsync()
        //{
        //    await GetWpfViewAsync();

        //    // Make the button invisible by default

        //    var mItems = m_textView.Selection.SelectedSpans;
        //    // Show the button only if a supported file is selected
        //    return mItems.Count == 2;
        //}

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
            var w = ExecSwapAsync();
        }

        private async Task ExecSwapAsync()
        {
            //DTE2 dte = await GetDTE();
            //Assumes.Present(dte);

            await GetWpfViewAsync();

            // var items = dte.ActiveDocument.Object("TextDocument") as TextDocument;
            var mItems = m_textView.Selection.SelectedSpans;
            if (mItems.Count == 2)
            {
                var selected1 = mItems[0].GetText();
                var selected2 = mItems[1].GetText();
                var textEdit = _buffer.CreateEdit();
                textEdit.Replace(mItems[0], selected2);
                textEdit.Replace(mItems[1], selected1);
                textEdit.Apply();
            }
        }

        //private async Task<DTE2> GetDTE()
        //{
        //    return await this.ServiceProvider.GetServiceAsync(typeof(DTE)) as DTE2;
        //}

        private async Task GetWpfViewAsync()
        {
            var txtMgr = await ServiceProvider.GetServiceAsync(typeof(SVsTextManager));
            var txtManager = (IVsTextManager)txtMgr;

            var componentMod = await ServiceProvider.GetServiceAsync(typeof(SComponentModel));
            var componentModel = (IComponentModel)componentMod;
            var editor = componentModel.GetService<IVsEditorAdaptersFactoryService>();

            IVsTextView textViewCurrent;//Cannot be inline with out because causes error on AppVeyor Build
            txtManager.GetActiveView(1, null, out textViewCurrent);
            m_textView = editor.GetWpfTextView(textViewCurrent);
            _buffer = m_textView.TextBuffer;
        }
    }
}
