// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Text;

namespace System.CommandLine.Rendering
{
    internal static class WrappingExtensions
    {
        public static IEnumerable<string> SplitForWrapping(this string text)
        {
            var sb = new StringBuilder();

            var foundWhitespace = false;

            for (var i = 0; i < text.Length; i++)
            {
                var c = text[i];

                if (char.IsWhiteSpace(c))
                {
                    if (c == '\n')
                    {
                        if (sb.Length > 0)
                        {
                            foundWhitespace = false;
                            yield return sb.ToString();
                            sb.Clear();
                        }

                        yield return c.ToString();
                    }
                    else if (c == '\r' &&
                             text.Length > i &&
                             text[i + 1] == '\n')
                    {
                        if (sb.Length > 0)
                        {
                            foundWhitespace = false;
                            yield return sb.ToString();
                            sb.Clear();
                        }

                        yield return "\r\n";

                        i++;
                    }
                    else
                    {
                        sb.Append(c);

                        foundWhitespace = true;
                    }
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
