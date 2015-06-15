using Microsoft.VisualStudio.Text;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RobotsTxtLanguageService.Syntax
{
    public class RobotsTxtDocumentSyntax : SyntaxNode
    {
        public RobotsTxtDocumentSyntax()
        {
            this.Records = new List<RobotsTxtRecordSyntax>();
        }

        public ITextSnapshot Snapshot { get; set; }

        public IList<RobotsTxtRecordSyntax> Records { get; set; }


        public override SnapshotSpan Span
        {
            get
            {
                if (!this.Records.Any())
                    return new SnapshotSpan(this.Snapshot, 0, 0);

                return new SnapshotSpan(
                    this.Records.First().Span.Start,
                    this.Records.Last().Span.End
                );
            }
        }

        public override SnapshotSpan FullSpan
        {
            get
            {
                return new SnapshotSpan(this.Snapshot, 0, this.Snapshot.Length);
            }
        }

        public override SyntaxNode Parent
        {
            get
            {
                return null;
            }
        }

        public override IEnumerable<SyntaxNode> Descendants()
        {
            return this.Records.SelectMany(s => s.DescendantsAndSelf());
        }


        public override IEnumerable<SnapshotToken> GetTokens()
        {
            return this.Records.SelectMany(s => s.GetTokens());
        }
    }
}
