using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using static System.Environment;

namespace System.CommandLine
{
    public class RawHelpBuilder : HelpBuilder
    {
        /// <inheritdoc />
        /// <summary>
        /// Unlike the base <see cref="HelpBuilder"/>, this derivation preserves the formatting of the incoming text.
        /// </summary>
        public RawHelpBuilder(
            IConsole console,
            int? columnGutter = null,
            int? indentationSize = null,
            int? maxWidth = null)
            : base(console, columnGutter, indentationSize, maxWidth)
        {
        }

        /// <inheritdoc />
        /// <summary>
        /// Takes a string of text and breaks it into lines of <see cref="maxLength"/>
        /// characters.
        /// This derivation preserves the formatting of the incoming text.
        /// </summary>
        protected override IReadOnlyCollection<string> SplitText(string text, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return new[] {text};
            }

            var lines = new List<string>();
            var builder = new StringBuilder();

            foreach (var item in Regex.Split(text, @"(\r\n|\s)"))
            {
                var nextLength = item.Length + builder.Length;

                if (nextLength > maxLength || item == NewLine)
                {
                    lines.Add(builder.ToString());
                    builder.Clear();
                }

                if (item == NewLine)
                {
                    continue;
                }

                builder.Append(item);
            }

            if (builder.Length > 0)
            {
                lines.Add(builder.ToString());
            }

            return lines;
        }
    }
}
