using System;
using EnvDTE80;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;

namespace TrailingWhitespace
{
    class WhitespaceRemoverCommand : WhitespaceBase
    {
        public WhitespaceRemoverCommand(IVsTextView textViewAdapter, IWpfTextView view, DTE2 dte)
        {
            textViewAdapter.AddCommandFilter(this, out _nextCommandTarget);
            _view = view;
            _dte = dte;
        }

        override public int Exec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
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

        override public int QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
        {
            return _nextCommandTarget.QueryStatus(ref pguidCmdGroup, cCmds, prgCmds, pCmdText);
        }
    }
}
