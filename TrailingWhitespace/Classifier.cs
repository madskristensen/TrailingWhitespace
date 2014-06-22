using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using System;
using System.Collections.Generic;

namespace TrailingWhitespace
{
    public class TrailingClassifier : IClassifier
    {
        private IClassificationType _whitespace;

        public TrailingClassifier(IClassificationTypeRegistryService registry)
        {
            _whitespace = registry.GetClassificationType(TrailingClassificationTypes.Whitespace);
        }

        public IList<ClassificationSpan> GetClassificationSpans(SnapshotSpan span)
        {
            IList<ClassificationSpan> list = new List<ClassificationSpan>();

            ITextSnapshotLine line = span.Snapshot.GetLineFromPosition(span.Start.Position);
            string text = span.Snapshot.GetText(line.Start.Position, line.Length);
            string trimmed = text.TrimEnd();
            int diff = text.Length - trimmed.Length;

            if (diff > 0)
            {
                SnapshotSpan ss = new SnapshotSpan(span.Snapshot, line.Start + line.Length - diff, diff);
                list.Add(new ClassificationSpan(ss, _whitespace));
            }

            return list;
        }

        public event EventHandler<ClassificationChangedEventArgs> ClassificationChanged
        {
            add { }
            remove { }
        }
    }
}