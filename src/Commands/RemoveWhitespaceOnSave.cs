using System;
using System.Collections;
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
        public RemoveWhitespaceOnSave(IVsTextView textViewAdapter, IWpfTextView view, DTE2 dte, ITextDocument document)
        {
            _ = textViewAdapter.AddCommandFilter(this, out _nextCommandTarget);
            _view = view;
            _dte = dte;
            _document = document;

            if (VSPackage.Options.TrimOnlyModifiedLines)
            {
                _view.Properties.GetOrCreateSingletonProperty("InitialSnapshot", () => _view.TextSnapshot);
            }
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
                    if (VSPackage.Options.TrimOnlyModifiedLines && _view.Properties.TryGetProperty("InitialSnapshot", out ITextSnapshot initialSnapshot))
                    {
                        var unchangedLines = GetUnchangedLines(initialSnapshot, buffer.CurrentSnapshot);
                        RemoveTrailingWhitespace(buffer, unchangedLines);
                    }
                    else
                    {
                        RemoveTrailingWhitespace(buffer);
                    }
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

        private BitArray GetUnchangedLines(ITextSnapshot snapA, ITextSnapshot snapB)
        {
            var linesA = snapA.Lines.Select(l => l.GetText()).ToArray();
            var linesB = snapB.Lines.Select(l => l.GetText()).ToArray();
            var unchangedIndicesInB = new BitArray(linesB.Length);

            PatienceDiffRecursive(linesA, 0, linesA.Length, linesB, 0, linesB.Length, unchangedIndicesInB);

            return unchangedIndicesInB;
        }

        private void PatienceDiffRecursive(string[] linesA, int startA, int endA, string[] linesB, int startB, int endB, BitArray unchangedIndicesInB)
        {
            if (startA >= endA || startB >= endB)
                return;

            // Find unique lines in range A
            var uniqueInA = new Dictionary<string, int>();
            var countsA = new Dictionary<string, int>();
            for (int i = startA; i < endA; i++)
            {
                var line = linesA[i];
                if (!countsA.ContainsKey(line)) countsA[line] = 0;
                countsA[line]++;
                uniqueInA[line] = i;
            }
            foreach (var key in countsA.Where(kvp => kvp.Value > 1).Select(kvp => kvp.Key).ToList()) uniqueInA.Remove(key);

            // Find unique lines in range B
            var uniqueInB = new Dictionary<string, int>();
            var countsB = new Dictionary<string, int>();
            for (int i = startB; i < endB; i++)
            {
                var line = linesB[i];
                if (!countsB.ContainsKey(line)) countsB[line] = 0;
                countsB[line]++;
                uniqueInB[line] = i;
            }
            foreach (var key in countsB.Where(kvp => kvp.Value > 1).Select(kvp => kvp.Key).ToList()) uniqueInB.Remove(key);

            // Find unique common lines
            var matchingLines = new List<Tuple<int, int>>();
            foreach (var kvp in uniqueInA)
            {
                if (uniqueInB.ContainsKey(kvp.Key))
                {
                    matchingLines.Add(new Tuple<int, int>(kvp.Value, uniqueInB[kvp.Key]));
                }
            }

            // Find LCS of unique common lines
            matchingLines.Sort((a, b) => a.Item1.CompareTo(b.Item1));
            var lcs = new List<Tuple<int, int>>();
            if (matchingLines.Count > 0)
            {
                var m_indices = new int[matchingLines.Count];
                var p_indices = new int[matchingLines.Count];
                var l = 0;

                for (var i = 0; i < matchingLines.Count; i++)
                {
                    var lo = 0;
                    var hi = l - 1;
                    while (lo <= hi)
                    {
                        var mid = lo + (hi - lo) / 2;
                        if (matchingLines[m_indices[mid]].Item2 < matchingLines[i].Item2) lo = mid + 1;
                        else hi = mid - 1;
                    }
                    var insertionPoint = lo;

                    if (insertionPoint == l) l++;
                    m_indices[insertionPoint] = i;

                    p_indices[i] = (insertionPoint > 0) ? m_indices[insertionPoint - 1] : -1;
                }

                if (l > 0)
                {
                    var current_index = m_indices[l - 1];
                    while (current_index != -1)
                    {
                        lcs.Add(matchingLines[current_index]);
                        current_index = p_indices[current_index];
                    }
                    lcs.Reverse();
                }
            }

            if (lcs.Count > 0)
            {
                foreach (var match in lcs)
                {
                    unchangedIndicesInB[match.Item2] = true;
                }

                var lastMatchA = startA - 1;
                var lastMatchB = startB - 1;

                foreach (var match in lcs)
                {
                    PatienceDiffRecursive(linesA, lastMatchA + 1, match.Item1, linesB, lastMatchB + 1, match.Item2, unchangedIndicesInB);
                    lastMatchA = match.Item1;
                    lastMatchB = match.Item2;
                }

                PatienceDiffRecursive(linesA, lastMatchA + 1, endA, linesB, lastMatchB + 1, endB, unchangedIndicesInB);
            }
            else // No unique anchors, fallback to standard LCS for the whole block
            {
                var standardLcsMatches = StandardLCS(linesA, startA, endA, linesB, startB, endB);
                foreach (var match in standardLcsMatches)
                {
                    unchangedIndicesInB[match.Item2] = true;
                }
            }
        }

        private List<Tuple<int, int>> StandardLCS(string[] linesA, int startA, int endA, string[] linesB, int startB, int endB)
        {
            var lenA = endA - startA;
            var lenB = endB - startB;
            var dp = new int[lenA + 1, lenB + 1];

            for (int i = 1; i <= lenA; i++)
            {
                for (int j = 1; j <= lenB; j++)
                {
                    if (linesA[startA + i - 1] == linesB[startB + j - 1])
                    {
                        dp[i, j] = dp[i - 1, j - 1] + 1;
                    }
                    else
                    {
                        dp[i, j] = Math.Max(dp[i - 1, j], dp[i, j - 1]);
                    }
                }
            }

            var lcs = new List<Tuple<int, int>>();
            int curA = lenA, curB = lenB;
            while (curA > 0 && curB > 0)
            {
                if (linesA[startA + curA - 1] == linesB[startB + curB - 1])
                {
                    lcs.Add(new Tuple<int, int>(startA + curA - 1, startB + curB - 1));
                    curA--;
                    curB--;
                }
                else if (dp[curA - 1, curB] > dp[curA, curB - 1])
                {
                    curA--;
                }
                else
                {
                    curB--;
                }
            }
            lcs.Reverse();
            return lcs;
        }
    }
}
