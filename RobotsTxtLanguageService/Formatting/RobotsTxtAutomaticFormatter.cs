using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Editor;
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
using System.Linq;
using System.Runtime.InteropServices;

namespace RobotsTxtLanguageService.Formatting
{
    [Export(typeof(IVsTextViewCreationListener))]
    [ContentType("plaintext")]
    [TextViewRole(PredefinedTextViewRoles.Interactive)]
    internal sealed class RobotsTxtAutomaticFormatter : IVsTextViewCreationListener
    {
#pragma warning disable 169

        [Import]
        private IVsEditorAdaptersFactoryService AdaptersFactory;

        [Import]
        private ITextDocumentFactoryService TextDocumentFactoryService;

#pragma warning restore 169


        public void VsTextViewCreated(IVsTextView textViewAdapter)
        {
            IWpfTextView view = AdaptersFactory.GetWpfTextView(textViewAdapter);
            Debug.Assert(view != null);

            // check whether the document is named robots.txt
            ITextDocument document;
            if (!TextDocumentFactoryService.TryGetTextDocument(view.TextDataModel.DocumentBuffer, out document))
                return;

            string fileName = Path.GetFileName(document.FilePath);
            if (!fileName.Equals("robots.txt", StringComparison.InvariantCultureIgnoreCase))
                return;
            
            // register command filter
            CommandFilter filter = new CommandFilter(view);

            IOleCommandTarget next;
            ErrorHandler.ThrowOnFailure(textViewAdapter.AddCommandFilter(filter, out next));
            filter.Next = next;
        }


        private sealed class CommandFilter : IOleCommandTarget
        {
            public CommandFilter(ITextView view)
            {
                _textView = view;
            }

            private readonly ITextView _textView;


            public void OnCharTyped(char @char)
            {
                // format on ':'
                if (@char == ':')
                {
                    ITextBuffer buffer = _textView.TextBuffer;

                    SyntaxTree syntaxTree = buffer.GetSyntaxTree();
                    RobotsTxtDocumentSyntax root = syntaxTree.Root as RobotsTxtDocumentSyntax;

                    // find in syntax tree
                    var caret = _textView.Caret.Position.BufferPosition;
                    RobotsTxtLineSyntax lineSyntax = root.Records
                        .SelectMany(r => r.Lines)
                        .FirstOrDefault(p => p.DelimiterToken.Span.Span.End == caret);

                    if (lineSyntax != null)
                    {
                        using (ITextEdit edit = buffer.CreateEdit())
                        {
                            // fix indent
                            // find property before
                            RobotsTxtLineSyntax before = lineSyntax.Record.Lines
                            .TakeWhile(p => p != lineSyntax)
                            .LastOrDefault();

                            // reference point
                            if (before != null)
                            {
                                SnapshotPoint referencePoint = before.NameToken.Span.Span.Start;

                                // compare
                                ITextSnapshotLine referenceLine = referencePoint.GetContainingLine();
                                ITextSnapshotLine line = lineSyntax.DelimiterToken.Span.Span.End.GetContainingLine();

                                SnapshotSpan referenceIndent = new SnapshotSpan(referenceLine.Start, referencePoint);
                                SnapshotSpan indent = new SnapshotSpan(line.Start, lineSyntax.NameToken.Span.Span.Start);

                                if (indent.GetText() != referenceIndent.GetText())
                                    edit.Replace(indent, referenceIndent.GetText());
                            }

                            // remove white space before ':'
                            if (lineSyntax.NameToken.Span.Span.End != lineSyntax.DelimiterToken.Span.Span.Start)
                                edit.Delete(new SnapshotSpan(lineSyntax.NameToken.Span.Span.End, lineSyntax.DelimiterToken.Span.Span.Start));

                            edit.Apply();
                        }
                    }
                }
            }

            
            public IOleCommandTarget Next { get; set; }

            public int Exec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
            {
                int hresult = VSConstants.S_OK;

                hresult = Next.Exec(pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);

                if (ErrorHandler.Succeeded(hresult))
                {
                    if (pguidCmdGroup == VSConstants.VSStd2K)
                    {
                        switch ((VSConstants.VSStd2KCmdID)nCmdID)
                        {
                            case VSConstants.VSStd2KCmdID.TYPECHAR:
                                char @char = GetTypeChar(pvaIn);
                                OnCharTyped(@char);
                                break;
                        }
                    }
                }

                return hresult;
            }

            public int QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
            {
                return Next.QueryStatus(pguidCmdGroup, cCmds, prgCmds, pCmdText);
            }

            private static char GetTypeChar(IntPtr pvaIn)
            {
                return (char)(ushort)Marshal.GetObjectForNativeVariant(pvaIn);
            }
        }
    }
}
