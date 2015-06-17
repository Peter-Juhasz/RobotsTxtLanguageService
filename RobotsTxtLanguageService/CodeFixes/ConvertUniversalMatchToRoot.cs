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
    internal sealed class ConvertUniversalMatchToRoot : ICodeFixProvider
    {
        private static readonly IReadOnlyCollection<string> FixableIds = new string[]
        {
            RobotsTxtLineValueAnalyzer.UniversalMatchInPath
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
                .First(s => s.ValueToken.Span.Span == span);

            string value = line.ValueToken.Value;

            // TODO: Fix multiple occurrences
            if (value.Count(c => c == '*') != 1)
                yield break;

            if (value == "*" || value.EndsWith("/*"))
            {
                yield return new CodeAction(
                    $"Change '*' to directory root",
                    () => Fix(line)
                );
            }
        }

        private ITextEdit Fix(RobotsTxtLineSyntax line)
        {
            ITextBuffer buffer = line.Record.Document.Snapshot.TextBuffer;

            string value = line.ValueToken.Value;

            // fix
            string newValue = value;

            // example: * -> /
            if (value == "*")
                newValue = "/";

            // example: /folder/* -> /folder/
            else if (value.EndsWith("/*"))
                newValue = value.Remove(value.Length - 1);

            ITextEdit edit = buffer.CreateEdit();
            edit.Replace(
                line.ValueToken.Span.Span,
                newValue
            );

            return edit;
        }
    }
}
