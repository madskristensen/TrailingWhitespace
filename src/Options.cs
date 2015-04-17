using Microsoft.VisualStudio.Shell;
using System.ComponentModel;

namespace TrailingWhitespace
{
    public class Options : DialogPage
    {
        public Options()
        {
            RemoveWhitespaceOnSave = true;
            IgnoreFileExtensions = ".md; .markdown";
        }

        [Category("On document save")]
        [DisplayName("Remove whitespace on save")]
        [Description("Every time a code file is saved, any whitespace is removed first")]
        [DefaultValue(true)]
        public bool RemoveWhitespaceOnSave { get; set; }

        [Category("On document save")]
        [DisplayName("Ignore files")]
        [Description("A semicolon separated list of file extensions to ignore when saving")]
        [DefaultValue(".md; .markdown")]
        public string IgnoreFileExtensions { get; set; }
    }
}
