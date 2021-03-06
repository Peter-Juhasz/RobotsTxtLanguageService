﻿using Microsoft.VisualStudio.Text;
using System.Collections.Generic;
using System.Linq;

namespace RobotsTxtLanguageService.Syntax
{
    public class RobotsTxtRecordSyntax : SyntaxNode
    {
        public RobotsTxtRecordSyntax()
        {
            this.Lines = new List<RobotsTxtLineSyntax>();
            this.LeadingTrivia = new List<SnapshotToken>();
            this.TrailingTrivia = new List<SnapshotToken>();
        }

        public RobotsTxtDocumentSyntax Document { get; set; }

        public IList<RobotsTxtLineSyntax> Lines { get; set; }
        

        public IList<SnapshotToken> LeadingTrivia { get; set; }

        public IList<SnapshotToken> TrailingTrivia { get; set; }


        public override SnapshotSpan Span
        {
            get
            {
                return new SnapshotSpan(this.Lines.First().Span.Start, this.Lines.Last().Span.End);
            }
        }

        public override SnapshotSpan FullSpan
        {
            get
            {
                return new SnapshotSpan(
                    (this.LeadingTrivia.FirstOrDefault()
                        ?? this.Lines.FirstOrDefault()?.NameToken
                    ).Span.Span.Start,
                    (this.TrailingTrivia.LastOrDefault()
                        ?? this.Lines.LastOrDefault()?.TrailingTrivia.LastOrDefault()
                        ?? this.Lines.LastOrDefault()?.ValueToken
                    ).Span.Span.End
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
            return this.Lines;
        }


        public override IEnumerable<SnapshotToken> GetTokens()
        {
            foreach (SnapshotToken token in this.LeadingTrivia)
                yield return token;
            
            foreach (SnapshotToken token in this.Lines.SelectMany(p => p.GetTokens()))
                yield return token;

            foreach (SnapshotToken token in this.TrailingTrivia)
                yield return token;
        }
    }
}
