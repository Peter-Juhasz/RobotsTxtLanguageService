using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Utilities;
using RobotsTxtLanguageService.Documentation;
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
                
                List<Completion> completions = new List<Completion>();
                foreach (var item in RobotsTxtDocumentation.BuiltInRecordDocumentations)
                {
                    completions.Add(new Completion(item.Key, item.Key, item.Value, _glyph, item.Key));
                }

                ITextSnapshot snapshot = _buffer.CurrentSnapshot;
                var triggerPoint = session.GetTriggerPoint(snapshot);

                if (triggerPoint == null)
                    return;

                var line = triggerPoint.Value.GetContainingLine();
                string text = line.GetText();
                int index = text.IndexOf(':');
                int hash = text.IndexOf('#');
                SnapshotPoint start = triggerPoint.Value;

                if (hash > -1 && hash < triggerPoint.Value.Position || (index > -1 && (start - line.Start.Position) > index))
                    return;

                while (start > line.Start && !char.IsWhiteSpace((start - 1).GetChar()))
                {
                    start -= 1;
                }

                var applicableTo = snapshot.CreateTrackingSpan(new SnapshotSpan(start, triggerPoint.Value), SpanTrackingMode.EdgeInclusive);

                completionSets.Add(
                    new CompletionSet("All", "All", applicableTo, completions, Enumerable.Empty<Completion>())
                );
            }

            public void Dispose()
            {
                _disposed = true;
            }
        }
    }
}
