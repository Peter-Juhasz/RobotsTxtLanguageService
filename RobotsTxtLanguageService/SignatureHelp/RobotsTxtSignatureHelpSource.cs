using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Utilities;
using System;
using System.ComponentModel.Composition;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using RobotsTxtLanguageService.Syntax;
using RobotsTxtLanguageService.Semantics;
using RobotsTxtLanguageService.Documentation;

namespace RobotsTxtLanguageService.SignatureHelp
{
    [Export(typeof(ISignatureHelpSourceProvider))]
    [Name("RobotsTxt Signature Help Provider")]
    [ContentType(RobotsTxtContentTypeNames.RobotsTxt)]
    [Order(Before = "default")]
    internal sealed class RobotsTxtSignatureHelpSourceProvider : ISignatureHelpSourceProvider
    {
        public ISignatureHelpSource TryCreateSignatureHelpSource(ITextBuffer textBuffer)
        {
            return textBuffer.Properties.GetOrCreateSingletonProperty(
                () => new RobotsTxtSignatureHelpSource(textBuffer)
            );
        }


        private sealed class RobotsTxtSignatureHelpSource : ISignatureHelpSource
        {
            public RobotsTxtSignatureHelpSource(ITextBuffer textBuffer)
            {
                this._textBuffer = textBuffer;
            }

            private readonly ITextBuffer _textBuffer;


            public void AugmentSignatureHelpSession(ISignatureHelpSession session, IList<ISignature> signatures)
            {
                ITextSnapshot snapshot = _textBuffer.CurrentSnapshot;

                // trigger point
                SnapshotPoint? point = session.GetTriggerPoint(snapshot);
                if (point == null)
                    return;
                
                // get syntax tree
                SyntaxTree syntaxTree = snapshot.GetSyntaxTree();
                var root = syntaxTree.Root as RobotsTxtDocumentSyntax;

                // find line in syntax tree
                ITextSnapshotLine line = point.Value.GetContainingLine();
                RobotsTxtLineSyntax lineSyntax = root.Records.SelectMany(r => r.Lines)
                    .Where(l => !l.NameToken.IsMissing && !l.DelimiterToken.IsMissing)
                    .FirstOrDefault(l => l.Span.IntersectsWith(line.Extent));

                // found
                if (lineSyntax != null)
                {
                    if (lineSyntax.DelimiterToken.Span.Span.End <= point.Value)
                    {
                        // get semantic model
                        ISemanticModel model = syntaxTree.GetSemanticModel();

                        // add signature
                        signatures.Add(new RobotsTxtSignature(model, lineSyntax));
                    }
                }
            }

            public ISignature GetBestMatch(ISignatureHelpSession session)
            {
                return session.Signatures.First();
            }


            void IDisposable.Dispose()
            { }


            private sealed class RobotsTxtSignature : ISignature
            {
                private const string ParameterName = "value";

                public RobotsTxtSignature(ISemanticModel model, RobotsTxtLineSyntax lineSyntax)
                {
                    RobotsTxtFieldSymbol field = model.GetFieldSymbol(lineSyntax);

                    // calculate span
                    ITextSnapshotLine line = lineSyntax.Span.Start.GetContainingLine();

                    this.ApplicableToSpan = lineSyntax.Record.Document.Snapshot.CreateTrackingSpan(
                        new SnapshotSpan(
                            lineSyntax.NameToken.Span.Span.Start,
                            lineSyntax.TrailingTrivia.FirstOrDefault(t => t.Span.Span.IntersectsWith(line.Extent))?.Span.Span.Start ?? line.Extent.End
                        ),
                        SpanTrackingMode.EdgeInclusive
                    );

                    // content
                    string content = $"{field.Name}: {ParameterName}";

                    if (field.IsExtension)
                        content = $"(extension) {content}";

                    this.Content = content;

                    // parameters
                    this.Parameters = new ReadOnlyCollection<IParameter>(
                        new []
                        {
                            new RobotsTxtParameter(null, new Span(this.Content.LastIndexOf(ParameterName), ParameterName.Length), ParameterName, this)
                        }
                    );
                    this.CurrentParameter = this.Parameters.Single();

                    // documentation
                    this.Documentation = RobotsTxtDocumentation.GetDocumentation(field);
                }

                public ITrackingSpan ApplicableToSpan { get; private set; }

                public string Content { get; private set; }

                public IParameter CurrentParameter { get; private set; }

                public string Documentation { get; private set; }

                public ReadOnlyCollection<IParameter> Parameters { get; private set; }

                public string PrettyPrintedContent { get; private set; }

                public event EventHandler<CurrentParameterChangedEventArgs> CurrentParameterChanged;
            }

            private sealed class RobotsTxtParameter : IParameter
            {
                public RobotsTxtParameter(string documentation, Span locus, string name, ISignature signature)
                {
                    this.Documentation = documentation;
                    this.Locus = locus;
                    this.Name = name;
                    this.Signature = signature;
                }

                public string Documentation { get; private set; }
                public Span Locus { get; private set; }
                public string Name { get; private set; }
                public ISignature Signature { get; private set; }
                public Span PrettyPrintedLocus { get; private set; }
            }
        }
    }
}
