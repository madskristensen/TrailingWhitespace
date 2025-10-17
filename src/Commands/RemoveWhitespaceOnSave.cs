using System;
using System.Collections.Generic;
using System.Linq;
using EnvDTE80;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Differencing;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using TrailingWhitespace;

namespace TrailingWhitespace
{
    internal class RemoveWhitespaceOnSave : WhitespaceBase
    {
        private readonly ITextDifferencingSelectorService _textDifferencingSelectorService;

        public RemoveWhitespaceOnSave(IVsTextView textViewAdapter, IWpfTextView view, DTE2 dte, ITextDocument document, IVsEditorAdaptersFactoryService editorAdaptersFactoryService, ITextDifferencingSelectorService textDifferencingSelectorService, IDifferenceBufferFactoryService differenceBufferFactoryService)
        {
            _ = textViewAdapter.AddCommandFilter(this, out _nextCommandTarget);
            _view = view;
            _dte = dte;
            _document = document;
            _textDifferencingSelectorService = textDifferencingSelectorService;
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
                    RemoveTrailingWhitespace(buffer, GetModifiedLines(buffer));
                }
            }

            return _nextCommandTarget.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
        }

        private IEnumerable<int> GetModifiedLines(ITextBuffer buffer)
        {
            if (!VSPackage.Options.OnlyRemoveFromModifiedLines)
                return null;

            var snapshot = buffer.CurrentSnapshot;
            var uneditedSnapshot = buffer.GetLastSavedSnapshot();
            if (snapshot == uneditedSnapshot)
                return Enumerable.Empty<int>();

            var diffService = _textDifferencingSelectorService.GetTextDifferencingService(buffer.ContentType);
            var diff = diffService.DiffSnapshots(uneditedSnapshot, snapshot);

            return diff.Differences.SelectMany(d =>
            {
                var span = d.Right;
                int startLine = snapshot.GetLineNumberFromPosition(span.Start);
                int endLine = snapshot.GetLineNumberFromPosition(span.End);
                return Enumerable.Range(startLine, endLine - startLine + 1);
            }).Distinct();
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
            _view.Closed -= OnViewClosed;
        }
    }
}
