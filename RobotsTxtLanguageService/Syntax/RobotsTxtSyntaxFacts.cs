using System;
using System.Collections.Generic;

namespace RobotsTxtLanguageService.Syntax
{
    internal static class RobotsTxtSyntaxFacts
    {
        public static readonly char Comment = '#';

        public static readonly char NameValueDelimiter = ':';
        
        public static readonly IReadOnlyCollection<string> WellKnownLineNames = new[] { "user-agent", "allow", "disallow" };

        public static readonly IReadOnlyCollection<string> ExtensionLineNames = new[] { "crawl-delay", "host", "sitemap" };

        public static bool IsValidIdentifierCharacter(char ch)
        {
            return Char.IsLetterOrDigit(ch) || ch == '-';
        }
    }
}
