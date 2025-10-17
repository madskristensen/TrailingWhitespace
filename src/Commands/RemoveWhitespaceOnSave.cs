using System;
using System.Collections.Generic;
using System.Linq;
using EnvDTE80;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;

namespace TrailingWhitespace
{
    internal class RemoveWhitespaceOnSave : WhitespaceBase
    {
        private HashSet<int> _modifiedLines;

        public RemoveWhitespaceOnSave(IVsTextView textViewAdapter, IWpfTextView view, DTE2 dte, ITextDocument document)
        {
            _ = textViewAdapter.AddCommandFilter(this, out _nextCommandTarget);
            _view = view;
            _dte = dte;
            _document = document;
            _modifiedLines = new HashSet<int>();
            view.TextBuffer.Changed += OnBufferChanged;
            view.Closed += OnViewClosed;
        }

        public override int Exec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            if (pguidCmdGroup == _cmdGgroup && _cmdId.Contains(nCmdID))
            {
                ITextBuffer buffer = _view.TextBuffer;

                if (buffer == null || !IsEnabled(buffer))
                {
                    return _nextCommandTarget.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
                }

                if (buffer != null && buffer.CheckEditAccess())
                {
                    RemoveTrailingWhitespace(buffer, _modifiedLines);
                    _modifiedLines.Clear();
                }
            }

            return _nextCommandTarget.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
        }

        private bool IsEnabled(ITextBuffer buffer)
        {
            if (VSPackage.Options != null)
            {
                return VSPackage.Options.RemoveWhitespaceOnSave && FileHelpers.IsFileSupported(buffer);
            }

            return false;
        }

        public override int QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
        {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            if (pguidCmdGroup == _cmdGgroup)
            {
                for (var i = 0; i < cCmds; i++)
                {
                    if (_cmdId.Contains(prgCmds[i].cmdID))
                    {
                        prgCmds[i].cmdf = (uint)(OLECMDF.OLECMDF_ENABLED | OLECMDF.OLECMDF_SUPPORTED);
                        return VSConstants.S_OK;
                    }
                }
            }

            return _nextCommandTarget.QueryStatus(ref pguidCmdGroup, cCmds, prgCmds, pCmdText);
        }
        private void OnViewClosed(object sender, EventArgs e)
        {
            _view.TextBuffer.Changed -= OnBufferChanged;
            _view.Closed -= OnViewClosed;
        }

        private void OnBufferChanged(object sender, TextContentChangedEventArgs e)
        {
            if (!VSPackage.Options.OnlyRemoveFromModifiedLines)
                return;

            // If the change was caused by our own edit, then ignore it
            if (WhitespaceBase.isRemovingWhitespace)
                return;

            foreach (var change in e.Changes)
            {
                int startLine = e.Before.GetLineNumberFromPosition(change.OldPosition);
                int endLine = e.Before.GetLineNumberFromPosition(change.OldPosition + change.OldLength);

                for (int i = startLine; i <= endLine; i++)
                {
                    _modifiedLines.Add(i);
                }
            }
        }
    }
}
