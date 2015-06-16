using RobotsTxtLanguageService.Syntax;
using Microsoft.VisualStudio.Text.Adornments;
using Microsoft.VisualStudio.Text.Tagging;
using System.Collections.Generic;
using System.Linq;
using System;

namespace RobotsTxtLanguageService.Diagnostics
{
    [ExportDiagnosticAnalyzer]
    internal sealed class UserAgentMustBeTheFirstLineOfARecord : ISyntaxNodeAnalyzer<RobotsTxtRecordSyntax>
    {
        public const string UserAgentInTheMiddleOfARecord = nameof(UserAgentInTheMiddleOfARecord);

        public IEnumerable<ITagSpan<IErrorTag>> Analyze(RobotsTxtRecordSyntax record)
        {
            var lines = record.Lines.Publish();

            var starters = lines
                .TakeWhile(l => l.NameToken.Value.Equals("User-agent", StringComparison.InvariantCultureIgnoreCase))
                .ToList();

            var middle = lines
                .FirstOrDefault(l => l.NameToken.Value.Equals("User-agent", StringComparison.InvariantCultureIgnoreCase));
            
            if (middle != null)
            {
                yield return new TagSpan<IErrorTag>(
                    middle.NameToken.Span.Span,
                    new DiagnosticErrorTag(PredefinedErrorTypeNames.SyntaxError, UserAgentInTheMiddleOfARecord, "Records must be separated by a blank line, or User-agent lines should be at the beginning of the record")
                );
            }
        }
    }
}
