using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;
using RobotsTxtLanguageService.Syntax;
using System.IO;
using System;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Text.Editor;

namespace RobotsTxtLanguageService
{
    /// <summary>
    /// Classifier provider. It adds the classifier to the set of classifiers.
    /// </summary>
    [Export(typeof(IClassifierProvider))]
    [ContentType(RobotsTxtContentTypeNames.RobotsTxt)] // This classifier applies to all text files.
    [Order(Before = Priority.High)]
    internal sealed class RobotsTxtClassifierProvider : IClassifierProvider
    {
#pragma warning disable 649

        /// <summary>
        /// Classification registry to be used for getting a reference
        /// to the custom classification type later.
        /// </summary>
        [Import]
        private IClassificationTypeRegistryService classificationRegistry;

        [Import("RobotsTxt")]
        private ISyntacticParser syntacticParser;

#pragma warning restore 649


        /// <summary>
        /// Gets a classifier for the given text buffer.
        /// </summary>
        /// <param name="buffer">The <see cref="ITextBuffer"/> to classify.</param>
        /// <returns>A classifier for the text buffer, or null if the provider cannot do so in its current state.</returns>
        public IClassifier GetClassifier(ITextBuffer buffer)
        {
            return buffer.Properties.GetOrCreateSingletonProperty(
                creator: () => new RobotsTxtClassifier(buffer, syntacticParser, this.classificationRegistry)
            );
        }
    }

    [Export(typeof(IVsTextViewCreationListener))]
    [ContentType("plaintext")]
    [TextViewRole(PredefinedTextViewRoles.Document)]
    internal sealed class RobotsTxtVCL : IVsTextViewCreationListener
    {
        [Import]
        private ITextDocumentFactoryService textDocumentFactoryService;

        [Import]
        private IVsEditorAdaptersFactoryService editorAdaptersFactoryService;

        [Import]
        private IContentTypeRegistryService contentTypeRegistry;

        public void VsTextViewCreated(IVsTextView textViewAdapter)
        {
            var view = editorAdaptersFactoryService.GetWpfTextView(textViewAdapter);
            var buffer = view.TextBuffer;

            ITextDocument document = null;
            if (textDocumentFactoryService.TryGetTextDocument(buffer, out document))
            {
                string fileName = Path.GetFileName(document.FilePath);

                if (fileName.Equals("robots.txt", StringComparison.InvariantCultureIgnoreCase))
                {
                    var contentType = contentTypeRegistry.GetContentType(RobotsTxtContentTypeNames.RobotsTxt);
                    buffer.ChangeContentType(contentType, null);
                }
            }
        }
    }
}
