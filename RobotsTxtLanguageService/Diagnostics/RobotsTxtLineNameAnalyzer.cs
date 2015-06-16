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
        public const string UnknownLine = nameof(UnknownLine);
        
        public IEnumerable<ITagSpan<IErrorTag>> Analyze(RobotsTxtLineSyntax line)
        {
            // delimiter missing
            string name = line.NameToken.Value;

            if (!RobotsTxtSyntaxFacts.WellKnownLineNames
                    .Union(RobotsTxtSyntaxFacts.ExtensionLineNames)
                    .Contains(name, StringComparer.InvariantCultureIgnoreCase))
            {
                yield return new TagSpan<IErrorTag>(
                    line.NameToken.Span.Span,
                    new DiagnosticErrorTag(PredefinedErrorTypeNames.OtherError, UnknownLine, $"Unknown line '{name}'")
                );
            }
        }
    }
}
