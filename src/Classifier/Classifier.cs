using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Projection;

namespace TrailingWhitespace
{
    internal class TrailingClassifier : IClassifier
    {
        private readonly IClassificationType _whitespace;
        private IWpfTextView _view;
        private readonly ITextBuffer _buffer;
        private ITrackingSpan _span;
        private SnapshotPoint _caret, _lastCaret;
        private static readonly IList<ClassificationSpan> _empty = new List<ClassificationSpan>();

        public TrailingClassifier(IClassificationTypeRegistryService registry, ITextBuffer buffer)
        {
            _whitespace = registry.GetClassificationType(TrailingClassificationTypes.Whitespace);
            _buffer = buffer;
            _buffer.Changed += OnSomethingChanged;
        }

        public IList<ClassificationSpan> GetClassificationSpans(SnapshotSpan span)
        {
            if (span.IsEmpty || _view == null)
            {
                return _empty;
            }

            IList<ClassificationSpan> list = new List<ClassificationSpan>();
            ITextSnapshotLine line = span.Snapshot.GetLineFromPosition(span.Start.Position);

            if (_caret > 0 && (line.Extent.Contains(_caret.Position) || line.Extent.End.Position == _caret.Position))
            {
                _span = span.Snapshot.CreateTrackingSpan(line.Extent, SpanTrackingMode.EdgeExclusive);
                return _empty;
            }

            if (span.Snapshot.TextBuffer is IProjectionBuffer projection)
            {
                SnapshotPoint point = projection.CurrentSnapshot.MapToSourceSnapshot(line.Start + line.Length);
                ITextSnapshotLine basePoint = point.Snapshot.GetLineFromPosition(point.Position);

                if (basePoint.Length > line.Length)
                {
                    return _empty;
                }
            }

            var text = line.GetText();
            var trimmed = text.TrimEnd();
            var diff = text.Length - trimmed.Length;

            if (diff > 0)
            {
                var ss = new SnapshotSpan(span.Snapshot, line.Start + line.Length - diff, diff);
                list.Add(new ClassificationSpan(ss, _whitespace));
            }

            return list;
        }


        public void SetTextView(IWpfTextView view)
        {
            if (_view != null)
            {
                return;
            }

            _view = view;
            _view.Caret.PositionChanged += OnSomethingChanged;
            _view.Closed += OnViewClosed;
            UpdateCaret();
        }

        private void OnViewClosed(object sender, EventArgs e)
        {
            var view = (ITextView)sender;
            view.Closed -= OnViewClosed;
            view.Caret.PositionChanged -= OnSomethingChanged;

            if (_buffer != null)
            {
                _buffer.Changed -= OnSomethingChanged;
            }
        }

        private void OnSomethingChanged(object sender, EventArgs e)
        {
            ThreadHelper.JoinableTaskFactory.StartOnIdle(() =>
            {
                UpdateCaret();
                if (_span != null)
                {
                    OnClassificationChanged(_span);
                }
                return Task.CompletedTask;
            }, VsTaskRunContext.UIThreadBackgroundPriority).FileAndForget(nameof(TrailingClassifier));

        }

        private void UpdateCaret()
        {
            if (_view == null)
            {
                return;
            }

            _lastCaret = _caret;
            SnapshotPoint position = _view.Caret.Position.BufferPosition;

            if (position == 0)
            {
                return;
            }

            _caret = _view.TextBuffer is IProjectionBuffer projection && position <= _view.TextBuffer.CurrentSnapshot.Length
                ? projection.CurrentSnapshot.MapToSourceSnapshot(position.Position)
                : position;
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