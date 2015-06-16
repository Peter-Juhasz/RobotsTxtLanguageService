using RobotsTxtLanguageService.Syntax;
using Microsoft.VisualStudio.Text.Adornments;
using Microsoft.VisualStudio.Text.Tagging;
using System.Collections.Generic;
using System.Linq;
using System;

namespace RobotsTxtLanguageService.Diagnostics
{
    [ExportDiagnosticAnalyzer]
    internal sealed class RobotsTxtDocumentSyntaxAnalyzer : ISyntaxNodeAnalyzer<RobotsTxtDocumentSyntax>
    {
        public const string NotStartsWithUserAgent = nameof(NotStartsWithUserAgent);

        public IEnumerable<ITagSpan<IErrorTag>> Analyze(RobotsTxtDocumentSyntax document)
        {
            // check for not empty
            if (document.Records.Any())
            {
                var first = document.Records.First();

                // check for first record is not user-agent
                if (!first.NameToken.Value.Equals("User-agent", StringComparison.InvariantCultureIgnoreCase))
                {
                    yield return new TagSpan<IErrorTag>(
                        first.NameToken.Span.Span,
                        new DiagnosticErrorTag(PredefinedErrorTypeNames.SyntaxError, NotStartsWithUserAgent, "Robots.txt document must start with a User-agent record.")
                    );
                }
            }
        }
    }
}
