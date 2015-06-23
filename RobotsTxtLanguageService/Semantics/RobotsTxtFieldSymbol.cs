using RobotsTxtLanguageService.Documentation;

namespace RobotsTxtLanguageService.Semantics
{
    internal class RobotsTxtFieldSymbol : ISymbol
    {
        public RobotsTxtFieldSymbol(string name)
        {
            this.Name = name;
        }

        public string Name { get; private set; }

        public bool IsExtension
        {
            get { return RobotsTxtDocumentation.ExtensionRecordDocumentations.ContainsKey(this.Name); }
        }

        public ISymbol ContainingSymbol { get; private set; }


        public override bool Equals(object obj)
        {
            RobotsTxtFieldSymbol other = obj as RobotsTxtFieldSymbol;
            return other != null && other.Name == this.Name;
        }

        public override int GetHashCode()
        {
            return this.Name.GetHashCode();
        }

        public override string ToString()
        {
            return this.Name;
        }
    }
}
