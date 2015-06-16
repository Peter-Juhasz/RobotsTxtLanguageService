using RobotsTxtLanguageService.Syntax;
using Microsoft.VisualStudio.Text.Adornments;
using Microsoft.VisualStudio.Text.Tagging;
using System.Collections.Generic;

namespace RobotsTxtLanguageService.Diagnostics
{
    [ExportDiagnosticAnalyzer]
    internal sealed class RobotsTxtLineSyntaxAnalyzer : ISyntaxNodeAnalyzer<RobotsTxtLineSyntax>
    {
        public const string MissingRecordNameValueDelimiter = "MissingRecordNameValueDelimiter";

        public IEnumerable<ITagSpan<IErrorTag>> Analyze(RobotsTxtLineSyntax property)
        {
            // delimiter missing
            if (property.DelimiterToken.IsMissing)
            {
                yield return new TagSpan<IErrorTag>(
                    property.DelimiterToken.Span.Span,
                    new DiagnosticErrorTag(PredefinedErrorTypeNames.SyntaxError, MissingRecordNameValueDelimiter, "':' expected")
                );
            }
        }
    }
}
