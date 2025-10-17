using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Differencing;

namespace TrailingWhitespace
{
    public static class TextBufferExtensions
    {
        public static ITextSnapshot GetLastSavedSnapshot(this ITextBuffer buffer)
        {
            if (buffer == null)
                return null;

            if (buffer.Properties.TryGetProperty(typeof(ITextDocument), out ITextDocument document))
            {
                return document.LastSavedSnapshot;
            }

            return null;
        }
    }
}