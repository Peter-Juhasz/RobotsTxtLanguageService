﻿using RobotsTxtLanguageService.Syntax;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using RobotsTxtLanguageService.Documentation;
using RobotsTxtLanguageService.Semantics;

namespace RobotsTxtLanguageService.QuickInfo
{
    [Export(typeof(IQuickInfoSourceProvider))]
    [Name("Robots.Txt Quick Info Provider")]
    [ContentType(RobotsTxtContentTypeNames.RobotsTxt)]
    internal sealed class RobotsTxtQuickInfoSourceProvider : IQuickInfoSourceProvider
    {
#pragma warning disable 649

        [Import]
        private IGlyphService glyphService;

        [Import]
        private IClassificationTypeRegistryService classificationRegistry;

        [Import]
        private IClassificationFormatMapService classificationFormatMapService;

#pragma warning restore 649


        public IQuickInfoSource TryCreateQuickInfoSource(ITextBuffer textBuffer)
        {
            return textBuffer.Properties.GetOrCreateSingletonProperty(
                creator: () => new RobotsTxtQuickInfoSource(
                    textBuffer,
                    glyphService,
                    classificationFormatMapService, 
                    classificationRegistry
                )
            );
        }


        private sealed class RobotsTxtQuickInfoSource : IQuickInfoSource
        {
            public RobotsTxtQuickInfoSource(
                ITextBuffer buffer,
                IGlyphService glyphService,
                IClassificationFormatMapService classificationFormatMapService,
                IClassificationTypeRegistryService classificationRegistry
            )
            {
                
                _buffer = buffer;
                _glyphService = glyphService;
                _classificationFormatMapService = classificationFormatMapService;
                _classificationRegistry = classificationRegistry;
            }

            private readonly ITextBuffer _buffer;
            private readonly IGlyphService _glyphService;
            private readonly IClassificationFormatMapService _classificationFormatMapService;
            private readonly IClassificationTypeRegistryService _classificationRegistry;

            private static readonly DataTemplate Template;


            public void AugmentQuickInfoSession(IQuickInfoSession session, IList<object> quickInfoContent, out ITrackingSpan applicableToSpan)
            {
                ITextSnapshot snapshot = _buffer.CurrentSnapshot;
                ITrackingPoint triggerPoint = session.GetTriggerPoint(_buffer);
                SnapshotPoint point = triggerPoint.GetPoint(snapshot);

                SyntaxTree syntax = snapshot.GetSyntaxTree();
                RobotsTxtDocumentSyntax root = syntax.Root as RobotsTxtDocumentSyntax;

                applicableToSpan = null;

                // find section
                RobotsTxtLineSyntax line = root.Records
                    .SelectMany(r => r.Lines)
                    .FirstOrDefault(s => s.NameToken.Span.Span.Contains(point));
                
                if (line != null)
                {
                    IClassificationFormatMap formatMap = _classificationFormatMapService.GetClassificationFormatMap(session.TextView);

                    string fieldName = line.NameToken.Value;
                    
                    // get glyph
                    var glyph = _glyphService.GetGlyph(StandardGlyphGroup.GlyphGroupProperty, StandardGlyphItem.GlyphItemPublic);
                    var classificationType = _classificationRegistry.GetClassificationType("RobotsTxt/RecordName");
                    var format = formatMap.GetTextProperties(classificationType);

                    // construct content
                    ISemanticModel model = syntax.GetSemanticModel();
                    var field = model.GetFieldSymbol(line);

                    var content = new QuickInfoContent
                    {
                        Glyph = glyph,
                        Signature = new Run(field.Name) { Foreground = format.ForegroundBrush },
                        Documentation = RobotsTxtDocumentation.GetDocumentation(field),
                    };
                    
                    // add to session
                    quickInfoContent.Add(
                        new ContentPresenter
                        {
                            Content = content,
                            ContentTemplate = Template,
                        }
                    );
                    applicableToSpan = snapshot.CreateTrackingSpan(line.NameToken.Span.Span, SpanTrackingMode.EdgeInclusive);
                    return;
                }
            }

            void IDisposable.Dispose()
            { }


            static RobotsTxtQuickInfoSource()
            {
                var resources = new ResourceDictionary { Source = new Uri("pack://application:,,,/RobotsTxtLanguageService;component/Themes/Generic.xaml", UriKind.RelativeOrAbsolute) };

                Template = resources.Values.OfType<DataTemplate>().First();
            }
        }
    }
}
