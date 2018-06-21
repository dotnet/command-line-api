using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using static System.Environment;

namespace System.CommandLine
{
    public class RawHelpBuilder : HelpBuilder
    {
        public RawHelpBuilder(int? columnGutter = null, int? indentationSize = null, int? maxWidth = null)
            : base(columnGutter, indentationSize, maxWidth)
        {
        }

        /// <inheritdoc />
        protected override IReadOnlyCollection<string> SplitText(string text, int maxLength)
        {
            var textLength = text.Length;

            if (string.IsNullOrWhiteSpace(text) || textLength < maxLength)
            {
                return new[] {text};
            }

            var lines = new List<string>();
            var builder = new StringBuilder();
            var index = 0;

            foreach (var item in Regex.Split(text, @"(\s+)"))
            {
                var length = item.Length + builder.Length;
                Debug.WriteLine(item);

                if (length > maxLength || item == NewLine)
                {
                    lines.Add(builder.ToString());
                    builder.Clear();
                    index = 0;
                }

                if (item != NewLine)
                {
                    builder.Append(item);
                }

                index += 1;
            }

            if (index != 0)
            {
                lines.Add(builder.ToString());
            }

            return lines;
        }
    }
}
