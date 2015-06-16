using RobotsTxtLanguageService.Syntax;
using Microsoft.VisualStudio.Text.Adornments;
using Microsoft.VisualStudio.Text.Tagging;
using System.Collections.Generic;
using System.Linq;
using System;

namespace RobotsTxtLanguageService.Diagnostics
{
    [ExportDiagnosticAnalyzer]
    internal sealed class RecordMustBeginWithUserAgent : ISyntaxNodeAnalyzer<RobotsTxtRecordSyntax>
    {
        public const string NotStartsWithUserAgent = nameof(NotStartsWithUserAgent);

        public IEnumerable<ITagSpan<IErrorTag>> Analyze(RobotsTxtRecordSyntax record)
        {
            var first = record.Lines.First();

            // check for first record is not user-agent
            if (!first.NameToken.Value.Equals("User-agent", StringComparison.InvariantCultureIgnoreCase))
            {
                yield return new TagSpan<IErrorTag>(
                    first.NameToken.Span.Span,
                    new DiagnosticErrorTag(PredefinedErrorTypeNames.SyntaxError, NotStartsWithUserAgent, "Records must start with a User-agent line.")
                );
            }
        }
    }
}
