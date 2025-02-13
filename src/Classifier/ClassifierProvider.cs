using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

using System.ComponentModel.Composition;

namespace TrailingWhitespace
{
    [Export(typeof(IClassifierProvider))]
    [ContentType("text")]
    [TextViewRole(PredefinedTextViewRoles.PrimaryDocument)]
    class TrailingClassifierProvider : IClassifierProvider
    {
        [Import]
        public IClassificationTypeRegistryService RegistryService { get; set; }

        public IClassifier GetClassifier(ITextBuffer textBuffer)
        {
            if (!textBuffer.Properties.ContainsProperty(typeof(TrailingClassifier)) && FileHelpers.IsFileSupported(textBuffer))
            {
                return textBuffer.Properties.GetOrCreateSingletonProperty(() => new TrailingClassifier(RegistryService, textBuffer));
            }

            return null;
        }
    }
}