using RobotsTxtLanguageService.Syntax;
using Microsoft.VisualStudio.Text.Adornments;
using Microsoft.VisualStudio.Text.Tagging;
using System.Collections.Generic;
using System.Linq;
using System;

namespace RobotsTxtLanguageService.Diagnostics
{
    [ExportDiagnosticAnalyzer]
    internal sealed class MultipleDeclarationsOfUniqueField : ISyntaxNodeAnalyzer<RobotsTxtLineSyntax>
    {
        public const string Id = nameof(MultipleDeclarationsOfUniqueField);

        private static readonly IReadOnlyCollection<string> UniqueLines = new[] { "Crawl-delay", "Host" };
        
        public IEnumerable<ITagSpan<IErrorTag>> Analyze(RobotsTxtLineSyntax line)
        {
            string name = line.NameToken.Value;

            // check for uniqueness
            if (UniqueLines.Contains(name, StringComparer.InvariantCultureIgnoreCase))
            {
                // find all
                var all = line.Record.Lines
                    .Where(l => l.NameToken.Value.Equals(name, StringComparison.InvariantCultureIgnoreCase))
                    .ToList();

                // check for duplicates
                if (all.Count >= 2)
                {
                    return 
                        from duplicate in all.Skip(1)
                        select new TagSpan<IErrorTag> (
                            duplicate.NameToken.Span.Span,
                            new DiagnosticErrorTag(PredefinedErrorTypeNames.SyntaxError, Id, $"Duplicate declaration of unique field '{name}' in the same record")
                        )
                    ;
                }
            }

            return Enumerable.Empty<ITagSpan<IErrorTag>>();
        }
    }
}
