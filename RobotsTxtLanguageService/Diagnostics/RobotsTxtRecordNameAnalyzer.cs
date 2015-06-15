using RobotsTxtLanguageService.Syntax;
using Microsoft.VisualStudio.Text.Adornments;
using Microsoft.VisualStudio.Text.Tagging;
using System.Collections.Generic;
using System.Linq;
using System;

namespace RobotsTxtLanguageService.Diagnostics
{
    [ExportDiagnosticAnalyzer]
    internal sealed class RobotsTxtRecordNameAnalyzer : ISyntaxNodeAnalyzer<RobotsTxtRecordSyntax>
    {
        public const string UnknownRecord = "UnknownRecord";

        private static readonly IReadOnlyCollection<string> WellKnownRecordNames = new[] { "user-agent", "allow", "disallow" };

        public IEnumerable<ITagSpan<IErrorTag>> Analyze(RobotsTxtRecordSyntax property)
        {
            // delimiter missing
            string name = property.NameToken.Value;

            if (!WellKnownRecordNames.Contains(name, StringComparer.InvariantCultureIgnoreCase))
            {
                yield return new TagSpan<IErrorTag>(
                    property.NameToken.Span.Span,
                    new DiagnosticErrorTag(PredefinedErrorTypeNames.OtherError, UnknownRecord, $"Unknown record '{name}'")
                );
            }
        }
    }
}
