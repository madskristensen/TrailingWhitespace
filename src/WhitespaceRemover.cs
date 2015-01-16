using System;
using System.ComponentModel.Composition;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Utilities;

namespace TrailingWhitespace
{
    [Export(typeof(IVsTextViewCreationListener))]
    [ContentType("code")]
    [TextViewRole(PredefinedTextViewRoles.Document)]
    [TextViewRole(PredefinedTextViewRoles.Editable)]
    class WhitespaceRemover : IVsTextViewCreationListener
    {
        [Import]
        public IVsEditorAdaptersFactoryService EditorAdaptersFactoryService { get; set; }

        [Import]
        public SVsServiceProvider serviceProvider { get; set; }

        public void VsTextViewCreated(IVsTextView textViewAdapter)
        {
            DTE2 dte = serviceProvider.GetService(typeof(DTE)) as DTE2;
            IWpfTextView textView = EditorAdaptersFactoryService.GetWpfTextView(textViewAdapter);

            textView.Properties.GetOrCreateSingletonProperty(() => new WhitespaceRemoverTarget(textViewAdapter, textView, dte));
        }
    }

    class WhitespaceRemoverTarget : IOleCommandTarget
    {
        private IOleCommandTarget _nextCommandTarget;
        private IWpfTextView _view;
        private DTE2 _dte;

        public WhitespaceRemoverTarget(IVsTextView textViewAdapter, IWpfTextView view, DTE2 dte)
        {
            textViewAdapter.AddCommandFilter(this, out _nextCommandTarget);
            _view = view;
            _dte = dte;
        }

        public int Exec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            if (pguidCmdGroup == new Guid("1496A755-94DE-11D0-8C3F-00C04FC2AAE2") && nCmdID == 64)
            {
                ITextBuffer buffer = _view.TextBuffer;

                if (!buffer.CheckEditAccess())
                    return _nextCommandTarget.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);

                ITextEdit edit = buffer.CreateEdit();
                ITextSnapshot snap = edit.Snapshot;

                foreach (ITextSnapshotLine line in snap.Lines)
                {
                    string text = line.GetText();
                    int length = text.Length;
                    while (--length >= 0 && Char.IsWhiteSpace(text[length])) ;
                    if (length < text.Length - 1)
                    {
                        int start = line.Start.Position;
                        edit.Delete(start + length + 1, text.Length - length - 1);
                    }
                }

                edit.Apply();
            }

            return _nextCommandTarget.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
        }

        private void RemoveTrailingWhitespace(ITextBuffer buffer)
        {
            TrailingClassifier classifier;

            if (!buffer.Properties.TryGetProperty(typeof(TrailingClassifier), out classifier))
                return;

            foreach (var line in buffer.CurrentSnapshot.Lines)
            {
                if (line.Start + line.LengthIncludingLineBreak > buffer.CurrentSnapshot.Length)
                    continue;

                var ss = new SnapshotSpan(buffer.CurrentSnapshot, line.Start, line.LengthIncludingLineBreak);
                var spans = classifier.GetClassificationSpans(ss);

                foreach (var span in spans)
                {
                    buffer.Delete(span.Span);
                }
            }
        }

        public int QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
        {
            return _nextCommandTarget.QueryStatus(ref pguidCmdGroup, cCmds, prgCmds, pCmdText);
        }
    }
}
