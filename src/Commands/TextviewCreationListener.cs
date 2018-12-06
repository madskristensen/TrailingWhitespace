using System.ComponentModel.Composition;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Utilities;

namespace TrailingWhitespace
{
    [Export(typeof(IVsTextViewCreationListener))]
    [Export(typeof(IWpfTextViewConnectionListener))]
    [ContentType("text")]
    [TextViewRole(PredefinedTextViewRoles.Document)]
    [TextViewRole(PredefinedTextViewRoles.Editable)]
    internal class TextviewCreationListener : IVsTextViewCreationListener, IWpfTextViewConnectionListener
    {
        [Import]
        public IVsEditorAdaptersFactoryService EditorAdaptersFactoryService { get; set; }

        [Import]
        public SVsServiceProvider ServiceProvider { get; set; }

        [Import]
        public ITextDocumentFactoryService TextDocumentFactoryService { get; set; }

        public void VsTextViewCreated(IVsTextView textViewAdapter)
        {
            var dte = ServiceProvider.GetService(typeof(DTE)) as DTE2;
            IWpfTextView textView = EditorAdaptersFactoryService.GetWpfTextView(textViewAdapter);

            textView.Properties.GetOrCreateSingletonProperty(() => new WhitespaceRemoverCommand(textViewAdapter, textView, dte));

            if (TextDocumentFactoryService.TryGetTextDocument(textView.TextDataModel.DocumentBuffer, out ITextDocument doc))
            {
                textView.Properties.GetOrCreateSingletonProperty(() => new RemoveWhitespaceOnSave(textViewAdapter, textView, dte, doc));
            }
        }

        public async void SubjectBuffersConnected(IWpfTextView textView, ConnectionReason reason, System.Collections.ObjectModel.Collection<ITextBuffer> subjectBuffers)
        {
            foreach (ITextBuffer buffer in subjectBuffers)
            {
                if (buffer.Properties.TryGetProperty(typeof(TrailingClassifier), out TrailingClassifier classifier))
                {
                    await classifier.SetTextViewAsync(textView);
                }
            }
        }

        public void SubjectBuffersDisconnected(IWpfTextView textView, ConnectionReason reason, System.Collections.ObjectModel.Collection<ITextBuffer> subjectBuffers)
        { }
    }
}
