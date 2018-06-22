using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using static System.Environment;

namespace System.CommandLine
{
    public class RawHelpBuilder : HelpBuilder
    {
        public RawHelpBuilder(
            IConsole console,
            int? columnGutter = null,
            int? indentationSize = null,
            int? maxWidth = null)
            : base(console, columnGutter, indentationSize, maxWidth)
        {
        }

        /// <inheritdoc />
        protected override IReadOnlyCollection<string> SplitText(string text, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return new[] {text};
            }

            var lines = new List<string>();
            var builder = new StringBuilder();
            var index = 0;

            foreach (var item in Regex.Split(text, @"(\s+)"))
            {
                var nextLength = item.Length + builder.Length;

                if (nextLength > maxLength || item == NewLine)
                {
                    lines.Add(builder.ToString());
                    builder.Clear();
                    index = 0;
                }

                if (item == NewLine)
                {
                    continue;
                }

                builder.Append(item);
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
