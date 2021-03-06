﻿using RobotsTxtLanguageService.Syntax;
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
        public const string MissingFieldValue = nameof(MissingFieldValue);
        public const string UriNotWellFormed = nameof(UriNotWellFormed);
        public const string UniversalMatchInPath = nameof(UniversalMatchInPath);
        public const string InvalidNumber = nameof(InvalidNumber);

        public IEnumerable<ITagSpan<IErrorTag>> Analyze(RobotsTxtLineSyntax line)
        {
            if (line.DelimiterToken.IsMissing)
                yield break;

            // delimiter missing
            string name = line.NameToken.Value;
            string value = line.ValueToken.Value;

            // allow, disallow
            if (name.Equals("Allow", StringComparison.InvariantCultureIgnoreCase) ||
                name.Equals("Disallow", StringComparison.InvariantCultureIgnoreCase) ||
                name.Equals("Sitemap", StringComparison.InvariantCultureIgnoreCase))
            {
                if (line.ValueToken.IsMissing)
                {
                    if (name.Equals("Sitemap", StringComparison.InvariantCultureIgnoreCase))
                    {
                        yield return new TagSpan<IErrorTag>(
                            line.ValueToken.Span.Span,
                            new DiagnosticErrorTag(PredefinedErrorTypeNames.SyntaxError, MissingFieldValue, $"Line value expected")
                        );
                    }

                    yield break;
                }

                // not well-formed
                if (!Uri.IsWellFormedUriString(value, UriKind.Relative))
                {
                    yield return new TagSpan<IErrorTag>(
                        line.ValueToken.Span.Span,
                        new DiagnosticErrorTag(PredefinedErrorTypeNames.SyntaxError, UriNotWellFormed, $"'{value}' is not a well-formed URI")
                    );
                }

                // * in URI
                else if (value.Contains("*"))
                {
                    yield return new TagSpan<IErrorTag>(
                        line.ValueToken.Span.Span,
                        new DiagnosticErrorTag(PredefinedErrorTypeNames.Warning, UniversalMatchInPath, $"Different bots may interpret '*' in different ways")
                    );
                }
            }

            // crawl-delay
            else if (name.Equals("Crawl-Delay", StringComparison.InvariantCultureIgnoreCase))
            {
                int seconds;
                if (!Int32.TryParse(value, out seconds))
                {
                    yield return new TagSpan<IErrorTag>(
                        line.ValueToken.Span.Span,
                        new DiagnosticErrorTag(PredefinedErrorTypeNames.SyntaxError, InvalidNumber, $"Invalid number '{value}'")
                    );
                }
                else if (seconds <= 0)
                {
                    yield return new TagSpan<IErrorTag>(
                        line.ValueToken.Span.Span,
                        new DiagnosticErrorTag(PredefinedErrorTypeNames.SyntaxError, InvalidNumber, $"Crawl-delay seconds must be a positive integer.")
                    );
                }
            }

            // any other line
            else if (!RobotsTxtSyntaxFacts.WellKnownLineNames
                    .Union(RobotsTxtSyntaxFacts.ExtensionLineNames)
                    .Contains(name, StringComparer.InvariantCultureIgnoreCase))
            {
                if (line.ValueToken.IsMissing)
                {
                    yield return new TagSpan<IErrorTag>(
                        line.ValueToken.Span.Span,
                        new DiagnosticErrorTag(PredefinedErrorTypeNames.SyntaxError, MissingFieldValue, $"Line value expected")
                    );
                }
            }
        }
    }
}
