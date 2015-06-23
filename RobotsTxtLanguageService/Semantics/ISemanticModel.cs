using RobotsTxtLanguageService.Syntax;

namespace RobotsTxtLanguageService.Semantics
{
    public interface ISemanticModel
    {
    }

    internal static class RobotsTxtSemanticModelExtensions
    {
        public static RobotsTxtFieldSymbol GetFieldSymbol(this ISemanticModel model, RobotsTxtLineSyntax line)
        {
            return new RobotsTxtFieldSymbol(line.NameToken.Value);
        }
    }
}
