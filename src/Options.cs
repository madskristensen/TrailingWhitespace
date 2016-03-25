using Microsoft.VisualStudio.Shell;
using System.ComponentModel;
using System.Collections.Generic;
using System;

namespace TrailingWhitespace
{
    public class Options : DialogPage
    {
        public Options()
        {
            RemoveWhitespaceOnSave = true;
            IgnorePatterns = @"\node_modules\, \bower_components\, \typings\, \lib\, .min., .md, .markdown";
            IgnoreMiscFiles = false;
        }

        [Category("General")]
        [DisplayName("Remove whitespace on save")]
        [Description("Every time a code file is saved, any whitespace is removed first")]
        [DefaultValue(true)]
        public bool RemoveWhitespaceOnSave { get; set; }

        [Category("General")]
        [DisplayName("Ignore pattern")]
        [Description("A comma-separated list of strings. Any file containing one of the strings in the path will be ignored.")]
        [DefaultValue(@"\node_modules\, \bower_components\, \typings\, \lib\, .min., .md, .markdown")]
        public string IgnorePatterns { get; set; }

        [Category("General")]
        [DisplayName("Ignore misc files")]
        [Description("When true, whitespace in files that don't belong to the project will not be shown.")]
        [DefaultValue(false)]
        public bool IgnoreMiscFiles { get; set; }

        public IEnumerable<string> GetIgnorePatterns()
        {
            var raw = IgnorePatterns.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string pattern in raw)
            {
                yield return pattern.Trim();
            }
        }
    }
}
