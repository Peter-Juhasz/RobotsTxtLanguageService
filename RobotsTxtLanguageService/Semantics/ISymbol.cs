namespace RobotsTxtLanguageService.Semantics
{
    public interface ISymbol
    {
        string Name { get; }

        ISymbol ContainingSymbol { get; }
    }
}
