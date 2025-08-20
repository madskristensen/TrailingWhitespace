using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.VisualStudio.Shell;

namespace TrailingWhitespace
{
    public class Options : DialogPage
    {
        public Options()
        {
            RemoveWhitespaceOnSave = true;
            IgnorePatterns = @"\node_modules\, \bower_components\, \typings\, \lib\, \Symbols\, .min., .md, .markdown, .designer.";
            IgnoreMiscFiles = false;
            IgnoreVerbatimString = true;
        }

        [Category("General")]
        [DisplayName("Remove whitespace on save")]
        [Description("Every time a code file is saved, any whitespace is removed first")]
        [DefaultValue(true)]
        public bool RemoveWhitespaceOnSave { get; set; }

        [Category("General")]
        [DisplayName("Ignore pattern")]
        [Description("A comma-separated list of strings. Any file containing one of the strings in the path will be ignored.")]
        [DefaultValue(@"\node_modules\, \bower_components\, \typings\, \lib\, .min., .md, .markdown, .designer.")]
        public string IgnorePatterns { get; set; }

        [Category("General")]
        [DisplayName("Ignore misc files")]
        [Description("When true, whitespace in files that don't belong to the project will not be shown.")]
        [DefaultValue(false)]
        public bool IgnoreMiscFiles { get; set; }

        [Category("General")]
        [DisplayName("Ignore Verbatim Strings")]
        [Description("When true, whitespace in verbatim strings will not be removed.")]
        [DefaultValue(true)]
        public bool IgnoreVerbatimString { get; set; }

        private static string[] _cachedPatterns;
        private static string _lastPatternString;
        public IEnumerable<string> GetIgnorePatterns()
        {
            if (_lastPatternString != IgnorePatterns || _cachedPatterns == null)
            {
                _cachedPatterns = IgnorePatterns.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                for (var i = 0; i < _cachedPatterns.Length; i++)
                {
                    _cachedPatterns[i] = _cachedPatterns[i].Trim();
                }

                _lastPatternString = IgnorePatterns;
            }
            return _cachedPatterns;
        }
    }
}
