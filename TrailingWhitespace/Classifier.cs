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

            string text = span.GetText();
            string trimmed = text.TrimEnd();
            int diff = text.Length - trimmed.Length;

            if (diff > 0)
            {
                SnapshotSpan ss = new SnapshotSpan(span.Snapshot, span.Start.Position + text.Length - diff, diff);
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