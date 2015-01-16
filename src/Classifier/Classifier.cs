using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Projection;

namespace TrailingWhitespace
{
    class TrailingClassifier : IClassifier
    {
        private IClassificationType _whitespace;
        private static IList<ClassificationSpan> _empty = new List<ClassificationSpan>();

        public TrailingClassifier(IClassificationTypeRegistryService registry)
        {
            _whitespace = registry.GetClassificationType(TrailingClassificationTypes.Whitespace);
        }

        public IList<ClassificationSpan> GetClassificationSpans(SnapshotSpan span)
        {
            if (span.IsEmpty)
                return _empty;

            IList<ClassificationSpan> list = new List<ClassificationSpan>();
            ITextSnapshotLine line = span.Snapshot.GetLineFromPosition(span.Start.Position);

            IProjectionBuffer projection = span.Snapshot.TextBuffer as IProjectionBuffer;
            if (projection != null)
            {
                SnapshotPoint point = projection.CurrentSnapshot.MapToSourceSnapshot(line.Start + line.Length);
                ITextSnapshotLine basePoint = point.Snapshot.GetLineFromPosition(point.Position);

                if (basePoint.Length > line.Length)
                    return list;
            }

            string text = line.GetText();
            string trimmed = text.TrimEnd();
            int diff = text.Length - trimmed.Length;

            if (diff > 0)
            {
                SnapshotSpan ss = new SnapshotSpan(span.Snapshot, line.Start + line.Length - diff, diff);
                list.Add(new ClassificationSpan(ss, _whitespace));
            }

            return list;
        }

        public event EventHandler<ClassificationChangedEventArgs> ClassificationChanged { add { } remove { } }
    }
}