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
    internal sealed class SeparateRecordsOrMoveUserAgentToTop : ICodeFixProvider
    {
        private static readonly IReadOnlyCollection<string> FixableIds = new string[]
        {
            UserAgentMustBeTheFirstLineOfARecord.Id
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

            // find section
            RobotsTxtRecordSyntax record = root.Records
                .First(s => s.Span.IntersectsWith(span));
            
            // find first declaration
            RobotsTxtLineSyntax line = record.Lines
                .First(s => s.NameToken.Span.Span == span);
            
            yield return new CodeAction(
                $"Separate records by a blank line",
                () => FixBySeparation(line)
            );
            yield return new CodeAction(
                $"Move line to the top of the record",
                () => FixByMoving(line, record)
            );
        }

        private ITextEdit FixBySeparation(RobotsTxtLineSyntax line)
        {
            ITextBuffer buffer = line.Record.Document.Snapshot.TextBuffer;

            ITextEdit edit = buffer.CreateEdit();
            edit.Insert(
                line.Span.Start.GetContainingLine().Start,
                Environment.NewLine
            );

            return edit;
        }

        private ITextEdit FixByMoving(RobotsTxtLineSyntax line, RobotsTxtRecordSyntax record)
        {
            ITextBuffer buffer = line.Record.Document.Snapshot.TextBuffer;

            // default insertion point at record start
            SnapshotPoint insertionPoint = record.Span.Start;

            // find last User-agent line
            var last = record.Lines
                .TakeWhile(l => l.NameToken.Value.Equals("User-agent", StringComparison.InvariantCultureIgnoreCase))
                .LastOrDefault();

            if (last != null) // override insertion point
                insertionPoint = last.Span.End.GetContainingLine().EndIncludingLineBreak;

            // move line up
            ITextEdit edit = buffer.CreateEdit();
            edit.Insert(
                insertionPoint,
                line.Span.Start.GetContainingLine().GetTextIncludingLineBreak()
            );
            edit.Delete(line.Span.Start.GetContainingLine().ExtentIncludingLineBreak);

            return edit;
        }
    }
}
