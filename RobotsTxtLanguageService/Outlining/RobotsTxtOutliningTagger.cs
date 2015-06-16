using RobotsTxtLanguageService.Syntax;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;

namespace RobotsTxtLanguageService
{
    [Export(typeof(ITaggerProvider))]
    [TagType(typeof(IOutliningRegionTag))]
    [ContentType(RobotsTxtContentTypeNames.RobotsTxt)]
    internal sealed class RobotsTxtOutliningTaggerProvider : ITaggerProvider
    {
        public ITagger<T> CreateTagger<T>(ITextBuffer buffer) where T : ITag
        {
            return buffer.Properties.GetOrCreateSingletonProperty(
                creator: () => new RobotsTxtOutliningTagger(buffer)
            ) as ITagger<T>;
        }


        private sealed class RobotsTxtOutliningTagger : ITagger<IOutliningRegionTag>
        {
            public RobotsTxtOutliningTagger(ITextBuffer buffer)
            {
                _buffer = buffer;
                _buffer.ChangedLowPriority += OnBufferChanged;
            }

            private readonly ITextBuffer _buffer;
            
            private void OnBufferChanged(object sender, TextContentChangedEventArgs e)
            {
                if (e.After != _buffer.CurrentSnapshot)
                    return;

                SnapshotSpan? changedSpan = null;

                // examine old version
                SyntaxTree oldSyntaxTree = e.Before.GetSyntaxTree();
                RobotsTxtDocumentSyntax oldRoot = oldSyntaxTree.Root as RobotsTxtDocumentSyntax;

                // find affected sections
                IReadOnlyCollection<RobotsTxtRecordSyntax> oldChangedRecords = (
                    from change in e.Changes
                    from record in oldRoot.Records
                    where record.Span.IntersectsWith(change.OldSpan)
                    orderby record.Span.Start
                    select record
                ).ToList();

                if (oldChangedRecords.Any())
                {
                    // compute changed span
                    changedSpan = new SnapshotSpan(
                        oldChangedRecords.First().Span.Start,
                        oldChangedRecords.Last().Span.End
                    );

                    // translate to new version
                    changedSpan = changedSpan.Value.TranslateTo(e.After, SpanTrackingMode.EdgeInclusive);
                }

                // examine current version
                SyntaxTree syntaxTree = e.After.GetSyntaxTree();
                RobotsTxtDocumentSyntax root = syntaxTree.Root as RobotsTxtDocumentSyntax;

                // find affected sections
                IReadOnlyCollection<RobotsTxtRecordSyntax> changedRecords = (
                    from change in e.Changes
                    from record in root.Records
                    where record.Span.IntersectsWith(change.NewSpan)
                    orderby record.Span.Start
                    select record
                ).ToList();
                
                if (changedRecords.Any())
                {
                    // compute changed span
                    SnapshotSpan newChangedSpan = new SnapshotSpan(
                        changedRecords.First().Span.Start,
                        changedRecords.Last().Span.End
                    );

                    changedSpan = changedSpan == null
                        ? newChangedSpan
                        : new SnapshotSpan(
                            changedSpan.Value.Start < newChangedSpan.Start ? changedSpan.Value.Start : newChangedSpan.Start,
                            changedSpan.Value.End > newChangedSpan.End ? changedSpan.Value.End : newChangedSpan.End
                        )
                    ;
                }

                // notify if any change affects outlining
                if (changedSpan != null)
                    this.TagsChanged?.Invoke(this, new SnapshotSpanEventArgs(changedSpan.Value));
            }
            

            public IEnumerable<ITagSpan<IOutliningRegionTag>> GetTags(NormalizedSnapshotSpanCollection spans)
            {
                ITextBuffer buffer = spans.First().Snapshot.TextBuffer;
                SyntaxTree syntax = buffer.GetSyntaxTree();
                RobotsTxtDocumentSyntax root = syntax.Root as RobotsTxtDocumentSyntax;

                return
                    from record in root.Records
                    where record.Lines.Count() >= 2
                    where spans.IntersectsWith(record.Span)
                    let first = record.Lines.First()
                    where first.NameToken.Value.Equals("User-agent", StringComparison.InvariantCultureIgnoreCase)
                    let last = record.Lines.Last()
                    let collapsibleSpan = new SnapshotSpan(
                        first.Span.End,
                        (last.TrailingTrivia.LastOrDefault() ?? last.ValueToken).Span.Span.End
                    )
                    select new TagSpan<IOutliningRegionTag>(
                        collapsibleSpan,
                        new OutliningRegionTag(
                            collapsedForm: "...",
                            collapsedHintForm: collapsibleSpan.GetText().Trim()
                        )
                    )
                ;
            }

            public event EventHandler<SnapshotSpanEventArgs> TagsChanged;
        }
    }
}
