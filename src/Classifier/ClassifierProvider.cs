using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Shell.Interop;
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
            string fileName = GetFileName(textBuffer);

            // Check for file name so the classifier won't show up in tool windows etc.
            if (!string.IsNullOrWhiteSpace(fileName))
            {
                return textBuffer.Properties.GetOrCreateSingletonProperty(() => new TrailingClassifier(registryService, textBuffer));
            }

            return null;
        }

        public static string GetFileName(ITextBuffer buffer)
        {
            Microsoft.VisualStudio.TextManager.Interop.IVsTextBuffer bufferAdapter;
            buffer.Properties.TryGetProperty(typeof(Microsoft.VisualStudio.TextManager.Interop.IVsTextBuffer), out bufferAdapter);

            if (bufferAdapter != null)
            {
                var persistFileFormat = bufferAdapter as IPersistFileFormat;
                string ppzsFilename = null;
                uint iii;
                if (persistFileFormat != null) persistFileFormat.GetCurFile(out ppzsFilename, out iii);
                return ppzsFilename;
            }

            return null;
        }
    }
}