using EnvDTE80;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using System;
using System.IO;
using System.Linq;

namespace TrailingWhitespace
{
    class RemoveWhitespaceOnSave : IOleCommandTarget
    {
        private IOleCommandTarget _nextCommandTarget;
        private IWpfTextView _view;
        private ITextDocument _document;
        private DTE2 _dte;
        private static Guid _cmdGgroup = typeof(VSConstants.VSStd97CmdID).GUID;
        private static uint[] _cmdId = new[] { (uint)VSConstants.VSStd97CmdID.SaveProjectItem, (uint)VSConstants.VSStd97CmdID.SaveSolution };

        public RemoveWhitespaceOnSave(IVsTextView textViewAdapter, IWpfTextView view, DTE2 dte, ITextDocument document)
        {
            textViewAdapter.AddCommandFilter(this, out _nextCommandTarget);
            _view = view;
            _dte = dte;
            _document = document;
        }

        public int Exec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            if (pguidCmdGroup == _cmdGgroup && _cmdId.Contains(nCmdID))
            {
                if (!IsEnabled())
                    return _nextCommandTarget.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);

                ITextBuffer buffer = _view.TextBuffer;

                if (buffer != null && buffer.CheckEditAccess())
                {
                    RemoveTrailingWhitespace(buffer);
                }
            }

            return _nextCommandTarget.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
        }

        private bool IsEnabled()
        {
            if (!VSPackage.Options.RemoveWhitespaceOnSave)
                return false;

            string[] extensions = VSPackage.Options.IgnoreFileExtensions.Split(new [] {';'}, StringSplitOptions.RemoveEmptyEntries);
            string fileExtension = Path.GetExtension(_document.FilePath);

            return !extensions.Any(ext => ext.Equals(fileExtension, StringComparison.OrdinalIgnoreCase));
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
