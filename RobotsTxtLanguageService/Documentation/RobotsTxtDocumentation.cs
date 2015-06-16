using System;
using System.Collections.Generic;

namespace RobotsTxtLanguageService.Documentation
{
    internal static class RobotsTxtDocumentation
    {
        public static readonly IReadOnlyDictionary<string, string> BuiltInRecordDocumentations = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase)
        {
            { "user-agent", "Identifies to which specific robots the record applies." },
            { "allow", "Determines whether accessing a URL that matches the corresponding path is allowed or disallowed." },
            { "disallow", "Determines whether accessing a URL that matches the corresponding path is allowed or disallowed." },
        };

        public static readonly IReadOnlyDictionary<string, string> ExtensionRecordDocumentations = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase)
        {
            { "crawl-delay", "Sets the number of seconds to wait between successive requests to the same server." },
            { "host", "Allows websites with multiple mirrors to specify their preferred domain." },
            { "sitemap", "Specifies the location of the Sitemap." },
        };
    }
}
