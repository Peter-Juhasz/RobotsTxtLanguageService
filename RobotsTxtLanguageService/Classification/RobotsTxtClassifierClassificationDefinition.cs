using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace RobotsTxtLanguageService
{
    /// <summary>
    /// Classification type definition export for RobotsTxtClassifier
    /// </summary>
    internal static class RobotsTxtClassifierClassificationDefinition
    {
#pragma warning disable 169
        
        [Export(typeof(ClassificationTypeDefinition))]
        [Name("RobotsTxt/Delimiter")]
        private static ClassificationTypeDefinition delimiter;
        
        [Export(typeof(ClassificationTypeDefinition))]
        [Name("RobotsTxt/RecordName")]
        private static ClassificationTypeDefinition propertyName;

        [Export(typeof(ClassificationTypeDefinition))]
        [Name("RobotsTxt/RecordValue")]
        private static ClassificationTypeDefinition propertyValue;

#pragma warning restore 169
    }
}
