using System;
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
        protected static readonly Guid _cmdGgroup = typeof(VSConstants.VSStd97CmdID).GUID;
        protected static readonly uint[] _cmdId = new[]
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
            try
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
                            var quoteCount = 0;
                            for (var i = 0; i < text.Length; i++)
                            {
                                if (text[i] == '"')
                                {
                                    quoteCount++;
                                }
                            }

                            if (text.Contains("@\"") && quoteCount == 1)
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
                            var trailingStart = length;
                            while (trailingStart > 0 && char.IsWhiteSpace(text[trailingStart - 1]))
                            {
                                trailingStart--;
                            }

                            if (trailingStart < text.Length)
                            {
                                var start = line.Start.Position;
                                _ = edit.Delete(start + trailingStart, text.Length - trailingStart);
                            }
                        }
                    }
                    _ = edit.Apply();
                }
            }
            catch (Exception)
            {
                // Some weird cases causes an error with multiple edits
            }
        }

        public abstract int QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText);

        public abstract int Exec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut);
    }
}
