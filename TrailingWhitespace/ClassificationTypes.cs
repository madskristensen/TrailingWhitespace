using System.ComponentModel.Composition;
using System.Windows.Media;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace TrailingWhitespace
{
    public static class TrailingClassificationTypes
    {
        public const string Whitespace = "TrailingWhitespace";

        [Export, Name(TrailingClassificationTypes.Whitespace)]
        public static ClassificationTypeDefinition TrailingWhitespace { get; set; }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = TrailingClassificationTypes.Whitespace)]
    [Name(TrailingClassificationTypes.Whitespace)]
    [Order(After = Priority.Default)]
    [UserVisible(true)]
    internal sealed class TrailingWhitespaceFormatDefinition : ClassificationFormatDefinition
    {
        public TrailingWhitespaceFormatDefinition()
        {
            BackgroundColor = Color.FromRgb(255, 145, 145);
            DisplayName = "Trailing Whitespace";
        }
    }
}