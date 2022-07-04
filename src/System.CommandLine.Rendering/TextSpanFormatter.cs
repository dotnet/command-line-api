// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace System.CommandLine.Rendering
{
    public class TextSpanFormatter :
        ICustomFormatter,
        IFormatProvider
    {
        private static readonly Regex _formattableStringParser;

        private readonly Dictionary<Type, Func<object, TextSpan>> _formatters = new();

        static TextSpanFormatter()
        {
            _formattableStringParser = new Regex(
                @"(\s*{{\s*)
	|
(\s*}}\s*)
	|
(?<token> \{ [0-9]+ [^\}]* \} )
	|
(?<text> [^\{\}]* )",
                RegexOptions.Compiled |
                RegexOptions.IgnorePatternWhitespace);
        }

        public void AddFormatter<T>(Func<T, TextSpan> format)
        {
            _formatters.Add(typeof(T),
                            t =>
                            {
                                var span = format((T)t);

                                return span ?? TextSpan.Empty();
                            });
        }

        public TextSpan Format(object value)
        {
            string content;

            switch (value)
            {
                case null:
                    return TextSpan.Empty();
                case TextSpan span:
                    return span;
                case AnsiControlCode ansiCode:
                    return new ControlSpan(ansiCode.EscapeSequence, ansiCode);
                case FormattableString formattable:
                    content = ((IFormattable) formattable).ToString("", this);
                    break;
                default:
                    content = value.ToString();
                    break;
            }

            if (_formatters.TryGetValue(value.GetType(), out var formatter))
            {
                return formatter(value);
            }

            if (string.IsNullOrEmpty(content))
            {
                return TextSpan.Empty();
            }
            else
            {
                return new ContentSpan(content);
            }
        }

        public void AddFormatter<T>(Func<T, FormattableString> format)
        {
            _formatters.Add(typeof(T),
                            t => {
                                var formattableString = format((T)t);

                                return formattableString == null
                                           ? TextSpan.Empty()
                                           : ParseToSpan(formattableString);
                            });
        }

        object IFormatProvider.GetFormat(Type formatType) => this;

        string ICustomFormatter.Format(
            string format,
            object arg,
            IFormatProvider formatProvider)
        {

            return Format(arg).ToString();
        }

        public TextSpan ParseToSpan(FormattableString formattableString)
        {
            var formatted = formattableString.ToString();

            var args = formattableString.GetArguments();

            if (args.Length == 0)
            {
                return Format(formatted);
            }
            else
            {
                return new ContainerSpan(DestructureIntoSpans().ToArray());
            }
            
            IEnumerable<TextSpan> DestructureIntoSpans()
            {
                var partIndex = 0;


                foreach (Match match in _formattableStringParser.Matches(formattableString.Format))
                {
                    if (match.Value != "")
                    {
                        if (match.Value.StartsWith("{") &&
                            match.Value.EndsWith("}"))
                        {
                            var arg = args[partIndex++];

                            if (match.Value.Contains(":"))
                            {
                                var formatString = match.Value.Split(new[] { '{', ':', '}' }, 4)[2];

                                yield return Format(
                                    string.Format("{0:" + formatString + "}", arg));
                            }
                            else
                            {
                                yield return Format(arg);
                            }
                        }
                        else
                        {
                            yield return Format(match.Value);
                        }
                    }
                }
            }
        }
    }
}
