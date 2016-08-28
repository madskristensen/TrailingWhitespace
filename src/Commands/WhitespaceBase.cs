using System;
using EnvDTE80;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace TrailingWhitespace
{
    abstract class WhitespaceBase : IOleCommandTarget
    {
        protected IOleCommandTarget _nextCommandTarget;
        protected IWpfTextView _view;
        protected ITextDocument _document;
        protected DTE2 _dte;
        protected static Guid _cmdGgroup = typeof(VSConstants.VSStd97CmdID).GUID;
        protected static uint[] _cmdId = new[] { (uint)VSConstants.VSStd97CmdID.SaveProjectItem, (uint)VSConstants.VSStd97CmdID.SaveSolution };

        protected static void RemoveTrailingWhitespace(ITextBuffer buffer)
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

        abstract public int QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText);

        abstract public int Exec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut);        
    }
}
