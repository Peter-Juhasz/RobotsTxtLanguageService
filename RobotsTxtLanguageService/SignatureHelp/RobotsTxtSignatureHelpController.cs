using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Utilities;
using RobotsTxtLanguageService.Syntax;
using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace RobotsTxtLanguageService.SignatureHelp
{
    [Export(typeof(IVsTextViewCreationListener))]
    [ContentType("plaintext")]
    [TextViewRole(PredefinedTextViewRoles.Interactive)]
    internal sealed class RobotsTxtSignatureHelpController : IVsTextViewCreationListener
    {
        [Import]
        private IVsEditorAdaptersFactoryService AdaptersFactory;

        [Import]
        private ISignatureHelpBroker SignatureHelpBroker;

        [Import]
        private ITextDocumentFactoryService TextDocumentFactoryService;

        public void VsTextViewCreated(IVsTextView textViewAdapter)
        {
            IWpfTextView view = AdaptersFactory.GetWpfTextView(textViewAdapter);
            Debug.Assert(view != null);

            ITextDocument document;
            if (!TextDocumentFactoryService.TryGetTextDocument(view.TextDataModel.DocumentBuffer, out document))
                return;

            string fileName = Path.GetFileName(document.FilePath);
            if (!fileName.Equals("robots.txt", StringComparison.InvariantCultureIgnoreCase))
                return;

            CommandFilter filter = new CommandFilter(view, SignatureHelpBroker);

            IOleCommandTarget next;
            ErrorHandler.ThrowOnFailure(textViewAdapter.AddCommandFilter(filter, out next));
            filter.Next = next;
        }


        private sealed class CommandFilter : IOleCommandTarget
        {
            private ICompletionSession _currentSession;

            public CommandFilter(IWpfTextView textView, ISignatureHelpBroker broker)
            {
                _currentSession = null;

                TextView = textView;
                Broker = broker;
            }

            public IWpfTextView TextView { get; private set; }
            public ISignatureHelpBroker Broker { get; private set; }
            public IOleCommandTarget Next { get; set; }

            private static char GetTypeChar(IntPtr pvaIn)
            {
                return (char)(ushort)Marshal.GetObjectForNativeVariant(pvaIn);
            }

            public int Exec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
            {
                int hresult = Next.Exec(pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);

                if (ErrorHandler.Succeeded(hresult))
                {
                    if (pguidCmdGroup == VSConstants.VSStd2K)
                    {
                        switch ((VSConstants.VSStd2KCmdID)nCmdID)
                        {
                            case VSConstants.VSStd2KCmdID.TYPECHAR:
                                char @char = GetTypeChar(pvaIn);

                                if (RobotsTxtSyntaxFacts.NameValueDelimiter == @char)
                                {
                                    this.Broker.TriggerSignatureHelp(this.TextView);
                                }
                                else if (RobotsTxtSyntaxFacts.Comment == @char)
                                {
                                    this.Broker.DismissAllSessions(this.TextView);
                                }
                                break;

                            case VSConstants.VSStd2KCmdID.RETURN:
                                this.Broker.DismissAllSessions(this.TextView);
                                break;
                        }
                    }
                }

                return hresult;
            }
            
            public int QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
            {
                if (pguidCmdGroup == VSConstants.VSStd2K)
                {
                    switch ((VSConstants.VSStd2KCmdID)prgCmds[0].cmdID)
                    {
                        case VSConstants.VSStd2KCmdID.PARAMINFO:
                            prgCmds[0].cmdf = (uint)OLECMDF.OLECMDF_ENABLED | (uint)OLECMDF.OLECMDF_SUPPORTED;
                            return VSConstants.S_OK;
                    }
                }

                return Next.QueryStatus(pguidCmdGroup, cCmds, prgCmds, pCmdText);
            }
        }
    }
}
