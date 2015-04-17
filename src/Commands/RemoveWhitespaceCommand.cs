using System;
using System.ComponentModel.Composition;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Utilities;

namespace TrailingWhitespace
{
    class WhitespaceRemoverCommand : IOleCommandTarget
    {
        private IOleCommandTarget _nextCommandTarget;
        private IWpfTextView _view;
        private DTE2 _dte;

        public WhitespaceRemoverCommand(IVsTextView textViewAdapter, IWpfTextView view, DTE2 dte)
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

                if (buffer.CheckEditAccess())
                {
                    RemoveTrailingWhitespace(buffer);
                    return VSConstants.S_OK;
                }
            }

            return _nextCommandTarget.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
        }

        private void RemoveTrailingWhitespace(ITextBuffer buffer)
        {
            using (ITextEdit edit = buffer.CreateEdit())
            {
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
        }

        public int QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
        {
            return _nextCommandTarget.QueryStatus(ref pguidCmdGroup, cCmds, prgCmds, pCmdText);
        }
    }
}
