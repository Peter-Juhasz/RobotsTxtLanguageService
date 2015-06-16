using RobotsTxtLanguageService.Syntax;
using Microsoft.VisualStudio.Text.Adornments;
using Microsoft.VisualStudio.Text.Tagging;
using System.Collections.Generic;
using System.Linq;
using System;

namespace RobotsTxtLanguageService.Diagnostics
{
    [ExportDiagnosticAnalyzer]
    internal sealed class RobotsTxtLineNameAnalyzer : ISyntaxNodeAnalyzer<RobotsTxtLineSyntax>
    {
        public const string UnknownRecord = "UnknownRecord";
        
        public IEnumerable<ITagSpan<IErrorTag>> Analyze(RobotsTxtLineSyntax property)
        {
            // delimiter missing
            string name = property.NameToken.Value;

            if (!SyntaxFacts.WellKnownRecordNames
                    .Union(SyntaxFacts.ExtensionRecordNames)
                    .Contains(name, StringComparer.InvariantCultureIgnoreCase))
            {
                yield return new TagSpan<IErrorTag>(
                    property.NameToken.Span.Span,
                    new DiagnosticErrorTag(PredefinedErrorTypeNames.OtherError, UnknownRecord, $"Unknown record '{name}'")
                );
            }
        }
    }
}
