using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Utilities;
using RobotsTxtLanguageService.Documentation;
using RobotsTxtLanguageService.Syntax;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows.Media;

namespace RobotsTxtLanguageService.CodeCompletion
{
    [Export(typeof(ICompletionSourceProvider))]
    [ContentType(RobotsTxtContentTypeNames.RobotsTxt)]
    [Name("RobotsTxtCompletion")]
    internal sealed class RobotsTxtCompletionSourceProvider : ICompletionSourceProvider
    {
#pragma warning disable 649

        [Import]
        private IGlyphService glyphService;

#pragma warning restore 649


        public ICompletionSource TryCreateCompletionSource(ITextBuffer textBuffer)
        {
            return new RobotsTxtCompletionSource(textBuffer, glyphService);
        }


        private sealed class RobotsTxtCompletionSource : ICompletionSource
        {
            public RobotsTxtCompletionSource(ITextBuffer buffer, IGlyphService glyphService)
            {
                _buffer = buffer;
                _glyph = glyphService.GetGlyph(StandardGlyphGroup.GlyphGroupProperty, StandardGlyphItem.GlyphItemPublic);
            }

            private readonly ITextBuffer _buffer;
            private readonly ImageSource _glyph;
            private bool _disposed = false;


            public void AugmentCompletionSession(ICompletionSession session, IList<CompletionSet> completionSets)
            {
                if (_disposed)
                    return;

                // get snapshot
                ITextSnapshot snapshot = _buffer.CurrentSnapshot;
                var triggerPoint = session.GetTriggerPoint(snapshot);
                if (triggerPoint == null)
                    return;

                // get or compute syntax tree
                SyntaxTree syntaxTree = snapshot.GetSyntaxTree();
                RobotsTxtDocumentSyntax root = syntaxTree.Root as RobotsTxtDocumentSyntax;

                // find line
                var lineSyntax = root.Records
                    .SelectMany(r => r.Lines)
                    .FirstOrDefault(l => l.Span.ContainsOrEndsWith(triggerPoint.Value));

                if (lineSyntax != null)
                {
                    // complete existing field
                    if (lineSyntax.NameToken.Span.Span.ContainsOrEndsWith(triggerPoint.Value))
                    {
                        IList<Completion> completions = new List<Completion>();

                        // applicable to
                        ITrackingSpan applicableTo = snapshot.CreateTrackingSpan(lineSyntax.NameToken.Span.Span, SpanTrackingMode.EdgeInclusive);

                        // find lines before
                        var before = lineSyntax.Record.Lines
                            .TakeWhile(l => l != lineSyntax)
                            .ToList();

                        // first line
                        if (!before.Any())
                        {
                            completions.Add(ToCompletion("User-agent"));
                        }

                        // right after User-agent
                        else if (before.All(l => l.NameToken.Value.Equals("User-agent", StringComparison.InvariantCultureIgnoreCase)))
                        {
                            completions.Add(ToCompletion("User-agent"));
                            completions.Add(ToCompletion("Allow"));
                            completions.Add(ToCompletion("Disallow"));
                            completions.Add(ToCompletion("Sitemap"));
                            completions.Add(ToCompletion("Host"));
                            completions.Add(ToCompletion("Crawl-delay"));
                        }

                        // any other case
                        else
                        {
                            completions.Add(ToCompletion("Allow"));
                            completions.Add(ToCompletion("Disallow"));
                            completions.Add(ToCompletion("Sitemap"));

                            if (!before.Any(l => l.NameToken.Value.Equals("Host", StringComparison.InvariantCultureIgnoreCase)))
                                completions.Add(ToCompletion("Host"));

                            if (!before.Any(l => l.NameToken.Value.Equals("Crawl-delay", StringComparison.InvariantCultureIgnoreCase)))
                                completions.Add(ToCompletion("Crawl-delay"));
                        }
                        
                        completionSets.Add(
                            new CompletionSet("All", "All", applicableTo, completions, Enumerable.Empty<Completion>())
                        );
                    }
                }
            }

            private Completion ToCompletion(string field)
            {
                string documentation;
                if (!RobotsTxtDocumentation.BuiltInRecordDocumentations.TryGetValue(field, out documentation))
                if (!RobotsTxtDocumentation.ExtensionRecordDocumentations.TryGetValue(field, out documentation))
                    documentation = null;

                return new Completion(field, field, documentation, _glyph, field);
            }

            public void Dispose()
            {
                _disposed = true;
            }
        }
    }
}
