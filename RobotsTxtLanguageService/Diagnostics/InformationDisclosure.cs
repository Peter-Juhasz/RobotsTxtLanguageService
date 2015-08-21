using RobotsTxtLanguageService.Syntax;
using Microsoft.VisualStudio.Text.Adornments;
using Microsoft.VisualStudio.Text.Tagging;
using System.Collections.Generic;
using System.Linq;
using System;

namespace RobotsTxtLanguageService.Diagnostics
{
    [ExportDiagnosticAnalyzer]
    internal sealed class InformationDisclosure : ISyntaxNodeAnalyzer<RobotsTxtLineSyntax>
    {
        public const string Id = nameof(InformationDisclosure);

        private static readonly IReadOnlyCollection<string> SuspiciousSegments = new string[]
        {
            "admin", "backup", "install", "sql"
        };

        public IEnumerable<ITagSpan<IErrorTag>> Analyze(RobotsTxtLineSyntax line)
        {
            if (line.ValueToken.IsMissing)
                yield break;

            // delimiter missing
            string name = line.NameToken.Value;
            string value = line.ValueToken.Value;

            // disallow
            if (name.Equals("Disallow", StringComparison.InvariantCultureIgnoreCase))
            {
                if (SuspiciousSegments.Any(s => value.IndexOf(s, StringComparison.InvariantCultureIgnoreCase) != -1))
                {
                    yield return new TagSpan<IErrorTag>(
                        line.ValueToken.Span.Span,
                        new DiagnosticErrorTag(PredefinedErrorTypeNames.OtherError, Id, $"Possible information disclosure: '{value}'{Environment.NewLine}Do not expose private resources in robots.txt. Control indexing in the markup and directly in the search engine instead.")
                    );
                }
            }
        }
    }
}
