using System.Collections.Generic;
using System.Text;

namespace System.CommandLine.Rendering
{
    internal static class WrappingExtensions
    {
        public static IEnumerable<string> SplitIntoWordsForWrapping(this string text)
        {
            var sb = new StringBuilder();

            var foundWhitespace = false;

            for (var i = 0; i < text.Length; i++)
            {
                var c = text[i];

                if (char.IsWhiteSpace(c))
                {
                    sb.Append(c);

                    foundWhitespace = true;
                }
                else
                {
                    if (foundWhitespace)
                    {
                        foundWhitespace = false;
                        yield return sb.ToString();
                        sb.Clear();
                    }

                    sb.Append(c);
                }
            }

            if (sb.Length > 0)
            {
                yield return sb.ToString();
            }
        }
    }
}
