using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Projection;
using Microsoft.VisualStudio.Utilities;

namespace TrailingWhitespace
{
    [Export(typeof(IClassifierProvider))]
    [ContentType("code")]
    [TextViewRole(PredefinedTextViewRoles.Editable)]
    class TrailingClassifierProvider : IClassifierProvider
    {
        [Import]
        public IClassificationTypeRegistryService registryService { get; set; }

        [Import]
        public ITextDocumentFactoryService documentService { get; set; }

        public IClassifier GetClassifier(ITextBuffer textBuffer)
        {
            ITextDocument document;

            if (documentService.TryGetTextDocument(textBuffer, out document))
            {
                return textBuffer.Properties.GetOrCreateSingletonProperty(() => new TrailingClassifier(registryService, document));
            }

            IProjectionBuffer projection = textBuffer as IProjectionBuffer;
            if (projection == null)
                return null;

            foreach (ITextBuffer sourceBuffer in projection.SourceBuffers)
            {
                if (documentService.TryGetTextDocument(sourceBuffer, out document))
                {
                    return textBuffer.Properties.GetOrCreateSingletonProperty(() => new TrailingClassifier(registryService, document));
                }
            }

            return null;
        }
    }
}