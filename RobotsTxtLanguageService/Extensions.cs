using System.Collections.Generic;
using System.Linq;

namespace RobotsTxtLanguageService
{
    internal static class Extensions
    {
        public static IEnumerable<T> SwitchToIfEmpty<T>(this IEnumerable<T> source, IEnumerable<T> second)
        {
            return source.Any()
                ? source
                : second
            ;
        }
    }
}
