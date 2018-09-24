using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace System.CommandLine.Rendering
{
    public class SpanFormatter :
        ICustomFormatter,
        IFormatProvider
    {
        private static readonly Regex _formattableStringParser =
            new Regex(@"
(\s*{{\s*)
	|
(\s*}}\s*)
	|
(?<token> \{ [0-9]+ [^\}]* \} )
	|
(?<text> [^\{\}]* )",
                      RegexOptions.Compiled |
                      RegexOptions.IgnorePatternWhitespace);

        private readonly Dictionary<Type, Func<object, Span>> _formatters = new Dictionary<Type, Func<object, Span>>();

        public void AddFormatter<T>(Func<T, Span> format)
        {
            _formatters.Add(typeof(T),
                            t =>
                            {
                                var span = format((T)t);

                                return span ?? Span.Empty();
                            });
        }

        public Span Format(object value)
        {
            if (value is null)
            {
                return Span.Empty();
            }

            if (value is Span span)
            {
                return span;
            }

            string content;

            if (_formatters.TryGetValue(value.GetType(), out var formatter))
            {
                return formatter(value);
            }

            if (value is FormattableString formattable)
            {
                content = ((IFormattable)formattable).ToString("", this);
            }
            else
            {
                content = value.ToString();
            }

            if (string.IsNullOrEmpty(content))
            {
                return Span.Empty();
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
                                           ? Span.Empty()
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

        public Span ParseToSpan(FormattableString formattableString)
        {
            var formatProvider = new ArgumentCapturingFormatProvider();

            var formatted = ((IFormattable)formattableString).ToString("", formatProvider);

            if (formatProvider.Args.Count == 0)
            {
                return Format(formatted);
            }
            else
            {
                return new ContainerSpan(DestructureIntoSpans().ToArray());
            }

            IEnumerable<Span> DestructureIntoSpans()
            {
                var partIndex = 0;

                foreach (Match match in _formattableStringParser.Matches(formattableString.Format))
                {
                    if (match.Value != "")
                    {
                        if (match.Value.StartsWith("{") &&
                            match.Value.EndsWith("}"))
                        {
                            var arg = formatProvider.Args[partIndex++];

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

        private class ArgumentCapturingFormatProvider :
            ICustomFormatter,
            IFormatProvider
        {
            public object GetFormat(Type formatType) => this;

            public string Format(
                string format,
                object arg,
                IFormatProvider formatProvider)
            {
                Args.Add(arg);

                return "";
            }

            public List<object> Args { get; } = new List<object>();
        }
    }
}
