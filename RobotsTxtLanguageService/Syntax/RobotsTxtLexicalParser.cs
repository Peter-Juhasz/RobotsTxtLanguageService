using Microsoft.VisualStudio.Language.StandardClassification;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;

namespace RobotsTxtLanguageService.Syntax
{
    [Export("RobotsTxt", typeof(ISyntacticParser))]
    internal sealed class RobotsTxtLexicalParser : ISyntacticParser
    {
        [ImportingConstructor]
        public RobotsTxtLexicalParser(IClassificationTypeRegistryService registry)
        {
            _commentType = registry.GetClassificationType(PredefinedClassificationTypeNames.Comment);
            _delimiterType = registry.GetClassificationType("RobotsTxt/Delimiter");
            _recordNameType = registry.GetClassificationType("RobotsTxt/RecordName");
            _recordValueType = registry.GetClassificationType("RobotsTxt/RecordValue");

        }

        private readonly IClassificationType _commentType;
        private readonly IClassificationType _delimiterType;
        private readonly IClassificationType _recordNameType;
        private readonly IClassificationType _recordValueType;


        public SyntaxTree Parse(ITextSnapshot snapshot)
        {
            RobotsTxtDocumentSyntax root = new RobotsTxtDocumentSyntax() { Snapshot = snapshot };

            List<SnapshotToken> leadingTrivia = new List<SnapshotToken>();
            RobotsTxtRecordSyntax currentRecord = new RobotsTxtRecordSyntax();
            bool lastLineWasBlankLine = false;
            
            foreach (ITextSnapshotLine line in snapshot.Lines)
            {
                bool isBlankLine = false;

                SnapshotPoint cursor = line.Start;
                snapshot.ReadWhiteSpace(ref cursor); // skip white space

                // skip blank lines
                if (cursor == line.End)
                {
                    if (currentRecord.Lines.Any())
                    {
                        root.Records.Add(currentRecord);
                        currentRecord = new RobotsTxtRecordSyntax { Document = root };
                    }

                    continue;
                }

                char first = cursor.GetChar();

                // comment
                if (first == RobotsTxtSyntaxFacts.Comment)
                {
                    SnapshotToken commentToken = new SnapshotToken(snapshot.ReadComment(ref cursor), _commentType);
                    leadingTrivia.Add(commentToken);
                }
                
                // record
                else if (Char.IsLetter(first))
                {
                    SnapshotToken name = new SnapshotToken(snapshot.ReadRecordName(ref cursor), _recordNameType);

                    // handle new record
                    if (lastLineWasBlankLine)
                    {
                        if (currentRecord.Lines.Any())
                        {
                            root.Records.Add(currentRecord);
                            currentRecord = new RobotsTxtRecordSyntax { Document = root };
                        }

                        isBlankLine = true;
                    }

                    snapshot.ReadWhiteSpace(ref cursor);
                    SnapshotToken delimiter = new SnapshotToken(snapshot.ReadDelimiter(ref cursor), _delimiterType);
                    snapshot.ReadWhiteSpace(ref cursor);
                    SnapshotToken value = new SnapshotToken(snapshot.ReadRecordValue(ref cursor), _recordValueType);
                    snapshot.ReadWhiteSpace(ref cursor);
                    SnapshotToken commentToken = new SnapshotToken(snapshot.ReadComment(ref cursor), _commentType);

                    IList<SnapshotToken> trailingTrivia = new List<SnapshotToken>();
                    if (!commentToken.IsMissing)
                        trailingTrivia.Add(commentToken);

                    RobotsTxtLineSyntax lineSyntax = new RobotsTxtLineSyntax()
                    {
                        Record = currentRecord,
                        LeadingTrivia = leadingTrivia,
                        NameToken = name,
                        DelimiterToken = delimiter,
                        ValueToken = value,
                        TrailingTrivia = trailingTrivia,
                    };
                    currentRecord.Lines.Add(lineSyntax);
                    leadingTrivia = new List<SnapshotToken>();
                }

                // error
                else
                    ; // TODO: report error

                lastLineWasBlankLine = isBlankLine;
            }

            if (currentRecord != null && leadingTrivia.Any())
                foreach (var trivia in leadingTrivia)
                    currentRecord.TrailingTrivia.Add(trivia);

            if (currentRecord.Lines.Any())
                root.Records.Add(currentRecord);
            
            return new SyntaxTree(snapshot, root);
        }
    }

    internal static class RobotsTxtScanner
    {
        public static SnapshotSpan ReadDelimiter(this ITextSnapshot snapshot, ref SnapshotPoint point)
        {
            if (point.Position == snapshot.Length)
                return new SnapshotSpan(point, 0);

            var @char = point.GetChar();

            if (@char != RobotsTxtSyntaxFacts.NameValueDelimiter)
                return new SnapshotSpan(point, 0);

            point = point + 1;
            return new SnapshotSpan(point - 1, 1);
        }
        public static SnapshotSpan ReadRecordName(this ITextSnapshot snapshot, ref SnapshotPoint point)
        {
            return snapshot.ReadToCommentOrLineEndWhile(ref point, c => c != RobotsTxtSyntaxFacts.NameValueDelimiter);
        }
        public static SnapshotSpan ReadRecordValue(this ITextSnapshot snapshot, ref SnapshotPoint point)
        {
            return snapshot.ReadToCommentOrLineEndWhile(ref point, _ => true);
        }

        public static SnapshotSpan ReadComment(this ITextSnapshot snapshot, ref SnapshotPoint point)
        {
            if (point.Position == snapshot.Length || point.GetChar() != RobotsTxtSyntaxFacts.Comment)
                return new SnapshotSpan(point, 0);

            return snapshot.ReadToLineEndWhile(ref point, _ => true);
        }

        public static SnapshotSpan ReadToCommentOrLineEndWhile(this ITextSnapshot snapshot, ref SnapshotPoint point, Predicate<char> predicate)
        {
            return snapshot.ReadToLineEndWhile(ref point, c => c != RobotsTxtSyntaxFacts.Comment && predicate(c));
        }
    }
}
