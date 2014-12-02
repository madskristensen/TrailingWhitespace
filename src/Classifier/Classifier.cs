using System;
using System.Collections.Generic;
using System.IO;
using EnvDTE80;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Projection;

namespace TrailingWhitespace
{
    public class TrailingClassifier : IClassifier
    {
        private IClassificationType _whitespace;
        private DTE2 _dte;

        public TrailingClassifier(IClassificationTypeRegistryService registry, DTE2 dte)
        {
            _whitespace = registry.GetClassificationType(TrailingClassificationTypes.Whitespace);
            _dte = dte;
        }

        public IList<ClassificationSpan> GetClassificationSpans(SnapshotSpan span)
        {
            IList<ClassificationSpan> list = new List<ClassificationSpan>();

            ITextSnapshotLine line = span.Snapshot.GetLineFromPosition(span.Start.Position);
            string text = line.GetText();
            string trimmed = text.TrimEnd();
            int diff = text.Length - trimmed.Length;

            if (diff > 0 && IsEmptyLinesSupported(trimmed.Length, span.Snapshot.TextBuffer))
            {
                SnapshotSpan ss = new SnapshotSpan(span.Snapshot, line.Start + line.Length - diff, diff);
                list.Add(new ClassificationSpan(ss, _whitespace));
            }

            return list;
        }

        private readonly List<string> _ext = new List<string>
        {
            ".cshtml",
            ".vbhtml",
            ".html",
            ".htm",
            ".aspx",
            ".ascx",
            ".master",
        };

        private bool IsEmptyLinesSupported(int length, ITextBuffer buffer)
        {
            if (length > 0)
                return true;

            var doc = _dte.ActiveDocument;
            if (doc == null)
                return false;

            return buffer.ContentType.IsOfType("html") ||  // Web Forms
                   buffer.ContentType.IsOfType("htmlx") || // HTML/Razor
                   buffer.ContentType.IsOfType("markdown") ||
                   !_ext.Contains(Path.GetExtension(doc.FullName));
        }

        public event EventHandler<ClassificationChangedEventArgs> ClassificationChanged
        {
            add { }
            remove { }
        }
    }
}