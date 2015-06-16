using RobotsTxtLanguageService.Syntax;
using Microsoft.VisualStudio.Text.Adornments;
using Microsoft.VisualStudio.Text.Tagging;
using System.Collections.Generic;
using System.Linq;
using System;

namespace RobotsTxtLanguageService.Diagnostics
{
    [ExportDiagnosticAnalyzer]
    internal sealed class RobotsTxtLineValueAnalyzer : ISyntaxNodeAnalyzer<RobotsTxtLineSyntax>
    {
        public const string MissingRecordValue = nameof(MissingRecordValue);
        public const string UriNotWellFormed = nameof(UriNotWellFormed);
        public const string UniversalMatchInPath = nameof(UniversalMatchInPath);
        public const string InvalidNumber = nameof(InvalidNumber);

        public IEnumerable<ITagSpan<IErrorTag>> Analyze(RobotsTxtLineSyntax record)
        {
            if (record.DelimiterToken.IsMissing)
                yield break;

            // delimiter missing
            string name = record.NameToken.Value;
            string value = record.ValueToken.Value;

            // allow, disallow
            if (name.Equals("Allow", StringComparison.InvariantCultureIgnoreCase) ||
                name.Equals("Disallow", StringComparison.InvariantCultureIgnoreCase))
            {
                if (record.ValueToken.IsMissing)
                    yield break;

                // not well-formed
                if (!Uri.IsWellFormedUriString(value, UriKind.Relative))
                {
                    yield return new TagSpan<IErrorTag>(
                        record.ValueToken.Span.Span,
                        new DiagnosticErrorTag(PredefinedErrorTypeNames.SyntaxError, UriNotWellFormed, $"'{value}' is not a well-formed URI")
                    );
                }

                // * in URI
                else if (value.Contains("*"))
                {
                    yield return new TagSpan<IErrorTag>(
                        record.ValueToken.Span.Span,
                        new DiagnosticErrorTag(PredefinedErrorTypeNames.Warning, UniversalMatchInPath, $"Different bots may interpret '*' in different ways")
                    );
                }
            }

            // crawl-delay
            else if (name.Equals("Crawl-Delay", StringComparison.InvariantCultureIgnoreCase))
            {
                int _;
                if (Int32.TryParse(value, out _))
                {
                    yield return new TagSpan<IErrorTag>(
                        record.ValueToken.Span.Span,
                        new DiagnosticErrorTag(PredefinedErrorTypeNames.SyntaxError, InvalidNumber, $"Crawl-delay seconds must be a positive integer.")
                    );
                }
                else
                {
                    yield return new TagSpan<IErrorTag>(
                        record.ValueToken.Span.Span,
                        new DiagnosticErrorTag(PredefinedErrorTypeNames.SyntaxError, InvalidNumber, $"Invalid number '{value}'")
                    );
                }
            }

            // any other record
            else if (!SyntaxFacts.WellKnownRecordNames
                    .Union(SyntaxFacts.ExtensionRecordNames)
                    .Contains(name, StringComparer.InvariantCultureIgnoreCase))
            {
                if (record.ValueToken.IsMissing)
                {
                    yield return new TagSpan<IErrorTag>(
                        record.ValueToken.Span.Span,
                        new DiagnosticErrorTag(PredefinedErrorTypeNames.SyntaxError, MissingRecordValue, $"Record value expected")
                    );
                }
            }
        }
    }
}
