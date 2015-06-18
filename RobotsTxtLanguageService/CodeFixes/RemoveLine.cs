using RobotsTxtLanguageService.CodeRefactorings;
using RobotsTxtLanguageService.Diagnostics;
using RobotsTxtLanguageService.Syntax;
using Microsoft.VisualStudio.Text;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;

namespace RobotsTxtLanguageService.CodeFixes
{
    [Export(typeof(ICodeFixProvider))]
    internal sealed class RemoveLine : ICodeFixProvider
    {
        private static readonly IReadOnlyCollection<string> FixableIds = new string[]
        {
            InformationDisclosure.Id
        };

        public IEnumerable<string> FixableDiagnosticIds
        {
            get { return FixableIds; }
        }

        public IEnumerable<CodeAction> GetFixes(SnapshotSpan span)
        {
            ITextBuffer buffer = span.Snapshot.TextBuffer;
            SyntaxTree syntax = buffer.GetSyntaxTree();
            RobotsTxtDocumentSyntax root = syntax.Root as RobotsTxtDocumentSyntax;
            
            // find declaration
            RobotsTxtLineSyntax line = root.Records
                .SelectMany(r => r.Lines)
                .First(s => s.Span.IntersectsWith(span));

            yield return new CodeAction(
                $"Remove line '{line.NameToken.Value}'",
                () => Fix(line)
            );
        }

        private ITextEdit Fix(RobotsTxtLineSyntax line)
        {
            ITextBuffer buffer = line.Record.Document.Snapshot.TextBuffer;

            ITextSnapshotLine textLine = line.Span.Start.GetContainingLine();
            
            ITextEdit edit = buffer.CreateEdit();
            edit.Delete(textLine.ExtentIncludingLineBreak);

            return edit;
        }
    }
}
