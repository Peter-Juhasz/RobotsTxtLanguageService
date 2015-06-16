using Microsoft.VisualStudio.Text;
using System.Collections.Generic;
using System;
using System.Linq;

namespace RobotsTxtLanguageService.Syntax
{
    public class RobotsTxtLineSyntax : SyntaxNode
    {
        public RobotsTxtLineSyntax()
        {
            this.LeadingTrivia = new List<SnapshotToken>();
            this.TrailingTrivia = new List<SnapshotToken>();
        }
        
        public RobotsTxtDocumentSyntax Document { get; set; }


        public SnapshotToken NameToken { get; set; }

        public SnapshotToken DelimiterToken { get; set; }

        public SnapshotToken ValueToken { get; set; }


        public IList<SnapshotToken> LeadingTrivia { get; set; }

        public IList<SnapshotToken> TrailingTrivia { get; set; }


        public override SnapshotSpan Span
        {
            get
            {
                return new SnapshotSpan(this.NameToken.Span.Span.Start, this.ValueToken.Span.Span.End);
            }
        }

        public override SnapshotSpan FullSpan
        {
            get
            {
                return new SnapshotSpan(
                    (this.LeadingTrivia.FirstOrDefault() ?? this.NameToken).Span.Span.Start,
                    (this.TrailingTrivia.LastOrDefault() ?? this.ValueToken).Span.Span.End
                );
            }
        }

        public override SyntaxNode Parent
        {
            get
            {
                return this.Document;
            }
        }

        public override IEnumerable<SyntaxNode> Descendants()
        {
            yield break;
        }


        public override IEnumerable<SnapshotToken> GetTokens()
        {
            foreach (SnapshotToken token in this.LeadingTrivia)
                yield return token;

            yield return this.NameToken;
            yield return this.DelimiterToken;
            yield return this.ValueToken;

            foreach (SnapshotToken token in this.TrailingTrivia)
                yield return token;
        }
    }
}
