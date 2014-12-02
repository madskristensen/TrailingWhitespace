using System.ComponentModel.Composition;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace TrailingWhitespace
{
    [Export(typeof(IClassifierProvider))]
    [ContentType("code")]
    public class TrailingClassifierProvider : IClassifierProvider
    {
        [Import]
        public IClassificationTypeRegistryService Registry { get; set; }

        [Import]
        public SVsServiceProvider serviceProvider { get; set; }

        public IClassifier GetClassifier(ITextBuffer textBuffer)
        {
            DTE2 dte = serviceProvider.GetService(typeof(DTE)) as DTE2;
            return textBuffer.Properties.GetOrCreateSingletonProperty(() => new TrailingClassifier(Registry, dte));
        }
    }
}