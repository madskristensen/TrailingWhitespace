using System;
using System.Linq;
using EnvDTE80;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace TrailingWhitespace
{
    internal abstract class WhitespaceBase : IOleCommandTarget
    {
        protected IOleCommandTarget _nextCommandTarget;
        protected IWpfTextView _view;
        protected ITextDocument _document;
        protected DTE2 _dte;
        protected static Guid _cmdGgroup = typeof(VSConstants.VSStd97CmdID).GUID;
        protected static uint[] _cmdId = new[]
        {
            (uint)VSConstants.VSStd97CmdID.SaveProjectItem,
            (uint)VSConstants.VSStd97CmdID.SaveSolution,
            (uint)VSConstants.VSStd97CmdID.BuildSln,
            (uint)VSConstants.VSStd97CmdID.RebuildSln,
            (uint)VSConstants.VSStd97CmdID.BuildSel,
            (uint)VSConstants.VSStd97CmdID.RebuildSel
        };

        protected static void RemoveTrailingWhitespace(ITextBuffer buffer)
        {
            using (ITextEdit edit = buffer.CreateEdit())
            {
                ITextSnapshot snap = edit.Snapshot;
                var isVerbatimString = false;
                foreach (ITextSnapshotLine line in snap.Lines)
                {
                    var text = line.GetText();
                    if (VSPackage.Options.IgnoreVerbatimString)
                    {
                        if (text.Contains("@\"") && text.Count(f => f == '"') == 1)
                        {
                            isVerbatimString = true;
                        }
                        else if (isVerbatimString && text.Contains("\""))
                        {
                            isVerbatimString = false;
                        }
                    }
                    if (!isVerbatimString)
                    {
                        var length = text.Length;
                        while (--length >= 0 && char.IsWhiteSpace(text[length]))
                        {
                            ;
                        }

                        if (length < text.Length - 1)
                        {
                            var start = line.Start.Position;
                            _ = edit.Delete(start + length + 1, text.Length - length - 1);
                        }
                    }
                }
                _ = edit.Apply();
            }
        }

        public abstract int QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText);

        public abstract int Exec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut);
    }
}
