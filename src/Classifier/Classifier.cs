using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Projection;
using System;
using System.Collections.Generic;
using System.Windows.Threading;
using System.Threading.Tasks;

namespace TrailingWhitespace
{
    class TrailingClassifier : IClassifier
    {
        private IClassificationType _whitespace;
        private IWpfTextView _view;
        private ITextBuffer _buffer;
        private ITrackingSpan _span;
        private SnapshotPoint _caret, _lastCaret;
        private static IList<ClassificationSpan> _empty = new List<ClassificationSpan>();

        public TrailingClassifier(IClassificationTypeRegistryService registry, ITextBuffer buffer)
        {
            _whitespace = registry.GetClassificationType(TrailingClassificationTypes.Whitespace);
            _buffer = buffer;
            _buffer.Changed += OnSomethingChanged;
        }

        public IList<ClassificationSpan> GetClassificationSpans(SnapshotSpan span)
        {
            if (span.IsEmpty || _view == null)
                return _empty;

            IList<ClassificationSpan> list = new List<ClassificationSpan>();
            ITextSnapshotLine line = span.Snapshot.GetLineFromPosition(span.Start.Position);

            if (_caret > 0 && (line.Extent.Contains(_caret.Position) || line.Extent.End.Position == _caret.Position))
            {
                _span = span.Snapshot.CreateTrackingSpan(line.Extent, SpanTrackingMode.EdgeExclusive);
                return _empty;
            }

            IProjectionBuffer projection = span.Snapshot.TextBuffer as IProjectionBuffer;
            if (projection != null)
            {
                SnapshotPoint point = projection.CurrentSnapshot.MapToSourceSnapshot(line.Start + line.Length);
                ITextSnapshotLine basePoint = point.Snapshot.GetLineFromPosition(point.Position);

                if (basePoint.Length > line.Length)
                    return _empty;
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


        public async Task SetTextView(IWpfTextView view)
        {
            if (_view != null)
                return;

            // Delay to allow Add Any File extension to place the caret
            await Task.Delay(100);

            if (_view == null)
            {
                _view = view;
                _view.Caret.PositionChanged += OnSomethingChanged;
                _view.Closed += OnViewClosed;
                UpdateCaret();
            }
        }

        private void OnViewClosed(object sender, EventArgs e)
        {
            ITextView view = (ITextView)sender;
            view.Closed -= OnViewClosed;
            view.Caret.PositionChanged -= OnSomethingChanged;

            if (_buffer != null)
            {
                _buffer.Changed -= OnSomethingChanged;
            }
        }

        private void OnSomethingChanged(object sender, EventArgs e)
        {
            Dispatcher.CurrentDispatcher.BeginInvoke(new Action(() =>
            {
                UpdateCaret();

                if (_span != null)
                    OnClassificationChanged(_span);

            }), DispatcherPriority.ApplicationIdle, null);
        }

        private void UpdateCaret()
        {
            if (_view == null)
                return;

            _lastCaret = _caret;
            var position = _view.Caret.Position.BufferPosition;

            if (position == 0)
                return;

            var projection = _view.TextBuffer as IProjectionBuffer;

            if (projection != null && position <= _view.TextBuffer.CurrentSnapshot.Length)
            {
                _caret = projection.CurrentSnapshot.MapToSourceSnapshot(position.Position);
            }
            else
            {
                _caret = position;
            }
        }

        private void OnClassificationChanged(ITrackingSpan span)
        {
            if ((_caret == 0 || _caret != _lastCaret) && ClassificationChanged != null)
            {
                ClassificationChanged(this, new ClassificationChangedEventArgs(span.GetSpan(span.TextBuffer.CurrentSnapshot)));
                _span = null;
            }
        }

        public event EventHandler<ClassificationChangedEventArgs> ClassificationChanged;
    }
}