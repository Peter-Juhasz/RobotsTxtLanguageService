using System.Collections.Generic;

namespace RobotsTxtLanguageService.Syntax
{
    internal static class SyntaxFacts
    {
        public static readonly char Comment = '#';

        public static readonly char NameValueDelimiter = ':';
        
        public static readonly IReadOnlyCollection<string> WellKnownRecordNames = new[] { "user-agent", "allow", "disallow" };

        public static readonly IReadOnlyCollection<string> ExtensionRecordNames = new[] { "crawl-delay", "host", "sitemap" };
    }
}
