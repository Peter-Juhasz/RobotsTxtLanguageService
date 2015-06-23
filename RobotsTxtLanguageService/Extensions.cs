using Microsoft.VisualStudio.Text;

namespace RobotsTxtLanguageService
{
    internal static class Extensions
    {
        public static bool ContainsOrEndsWith(this SnapshotSpan span, SnapshotPoint point)
        {
            return span.Contains(point) || span.End == point;
        }
    }
}
