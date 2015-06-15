//------------------------------------------------------------------------------
// <copyright file="RobotsTxtClassifierFormat.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System.ComponentModel.Composition;
using System.Windows.Media;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace RobotsTxtLanguageService
{
    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = "RobotsTxt/Delimiter")]
    [Name("RobotsTxt/Delimiter")]
    [UserVisible(true)] // This should be visible to the end user
    [Order(Before = Priority.Default)] // Set the priority to be after the default classifiers
    internal sealed class RobotsTxtDelimiterClassificationFormat : ClassificationFormatDefinition
    {
        public RobotsTxtDelimiterClassificationFormat()
        {
            this.DisplayName = "Robots.Txt Delimiter"; // Human readable version of the name
            this.ForegroundColor = Colors.Blue;
        }
    }
    
    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = "RobotsTxt/RecordName")]
    [Name("RobotsTxt/RecordName")]
    [UserVisible(true)] // This should be visible to the end user
    [Order(Before = Priority.Default)] // Set the priority to be after the default classifiers
    internal sealed class RobotsTxtPropertyNameClassificationFormat : ClassificationFormatDefinition
    {
        public RobotsTxtPropertyNameClassificationFormat()
        {
            this.DisplayName = "Robots.Txt Record Name"; // Human readable version of the name
            this.ForegroundColor = Colors.Maroon;
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = "RobotsTxt/RecordValue")]
    [Name("RobotsTxt/RecordValue")]
    [UserVisible(true)] // This should be visible to the end user
    [Order(Before = Priority.Default)] // Set the priority to be after the default classifiers
    internal sealed class RobotsTxtPropertyValueClassificationFormat : ClassificationFormatDefinition
    {
        public RobotsTxtPropertyValueClassificationFormat()
        {
            this.DisplayName = "Robots.Txt Record Value"; // Human readable version of the name
            this.ForegroundColor = Colors.Blue;
        }
    }
}
