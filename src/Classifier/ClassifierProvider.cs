using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace TrailingWhitespace
{
    [Export(typeof(IClassifierProvider))]
    [ContentType("text")]
    [TextViewRole(PredefinedTextViewRoles.Interactive)]
    class TrailingClassifierProvider : IClassifierProvider
    {
        [Import]
        public IClassificationTypeRegistryService registryService { get; set; }

        public IClassifier GetClassifier(ITextBuffer textBuffer)
        {
            if (FileHelpers.IsFileSupported(textBuffer))
            {
                return textBuffer.Properties.GetOrCreateSingletonProperty(() => new TrailingClassifier(registryService, textBuffer));
            }

            return null;
        }
    }
}