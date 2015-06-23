using RobotsTxtLanguageService.Syntax;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using RobotsTxtLanguageService.Semantics;

namespace RobotsTxtLanguageService
{
    [Export(typeof(IViewTaggerProvider))]
    [TagType(typeof(ITextMarkerTag))]
    [ContentType(RobotsTxtContentTypeNames.RobotsTxt)]
    internal sealed class RobotsTxtHighlightReferencesTaggerProvider : IViewTaggerProvider
    {
        public ITagger<T> CreateTagger<T>(ITextView textView, ITextBuffer buffer) where T : ITag
        {
            return textView.Properties.GetOrCreateSingletonProperty(
                creator: () => new RobotsTxtHighlightReferencesTagger(textView)
            ) as ITagger<T>;
        }


        private sealed class RobotsTxtHighlightReferencesTagger : ITagger<ITextMarkerTag>
        {
            public RobotsTxtHighlightReferencesTagger(ITextView view)
            {
                _view = view;

                _view.Caret.PositionChanged += OnCaretPositionChanged;
            }

            private readonly ITextView _view;

            private static readonly ITextMarkerTag Tag = new TextMarkerTag("MarkerFormatDefinition/HighlightedReference");


            private void OnCaretPositionChanged(object sender, CaretPositionChangedEventArgs e)
            {
                // TODO: optimize changed spans
                this.TagsChanged?.Invoke(this,
                    new SnapshotSpanEventArgs(new SnapshotSpan(e.TextView.TextBuffer.CurrentSnapshot, 0, e.TextView.TextBuffer.CurrentSnapshot.Length))
                );
            }
            
            public IEnumerable<ITagSpan<ITextMarkerTag>> GetTags(NormalizedSnapshotSpanCollection spans)
            {
                ITextBuffer buffer = spans.First().Snapshot.TextBuffer;

                // get syntax
                SyntaxTree syntax = buffer.GetSyntaxTree();
                RobotsTxtDocumentSyntax root = syntax.Root as RobotsTxtDocumentSyntax;

                // get semantics
                ISemanticModel model = syntax.GetSemanticModel();
                
                SnapshotPoint caret = _view.Caret.Position.BufferPosition;

                // find line
                RobotsTxtLineSyntax line = root.Records
                    .SelectMany(r => r.Lines)
                    .FirstOrDefault(s => s.NameToken.Span.Span.ContainsOrEndsWith(caret));

                // show references
                if (line != null)
                {
                    ISymbol field = model.GetFieldSymbol(line);

                    // find references
                    return
                        from r in root.Records
                        from l in r.Lines
                        where !l.NameToken.IsMissing
                        let f = model.GetFieldSymbol(l)
                        where f.Equals(field)
                        select new TagSpan<ITextMarkerTag>(l.NameToken.Span.Span, Tag)
                    ;
                }
                
                return Enumerable.Empty<TagSpan<ITextMarkerTag>>();
            }

            public event EventHandler<SnapshotSpanEventArgs> TagsChanged;
        }
    }
}
