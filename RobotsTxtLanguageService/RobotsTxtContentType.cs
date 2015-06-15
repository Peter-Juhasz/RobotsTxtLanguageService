//------------------------------------------------------------------------------
// <copyright file="RobotsTxtClassifierClassificationDefinition.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Utilities;

namespace RobotsTxtLanguageService
{
    /// <summary>
    /// Classification type definition export for classifier
    /// </summary>
    internal static class RobotsTxtContentType
    {
        // This disables "The field is never used" compiler's warning. Justification: the field is used by MEF.
#pragma warning disable 649

        [Export]
        [Name(RobotsTxtContentTypeNames.RobotsTxt)]
        [BaseDefinition("code")]
        internal static ContentTypeDefinition iniContentTypeDefinition;
        
#pragma warning restore 649
    }
}
