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

        public Span Format(object value)
        {
            if (value == null)
            {
                return new ContentSpan("");
            }
            else if (value is Span span)
            {
                return span;
            }
            else if (_formatters.TryGetValue(value.GetType(), out var formatter))
            {
                return formatter(value);
            }
            else if (value is FormattableString formattable)
            {
                var formatted = ((IFormattable)formattable).ToString("", this);

                return new ContentSpan(formatted);
            }
            else
            {
                return new ContentSpan(value.ToString());
            }
        }

        public void AddFormatter<T>(Func<T, string> format)
        {
            _formatters.Add(typeof(T),
                            t => {
                                var formatted = format((T)t);

                                if (formatted == null)
                                {
                                    return new ContentSpan("");
                                }

                                return new ContentSpan(formatted);
                            });
        }

        object IFormatProvider.GetFormat(Type formatType) => this;

        string ICustomFormatter.Format(
            string format,
            object arg,
            IFormatProvider formatProvider)
        {
            if (arg == null)
            {
                return "";
            }

            return Format(arg).ToString();
        }

        public Span ParseToSpan(FormattableString formattableString)
        {
            var formatProvider = new ArgumentCapturingFormatProvider();

            var formatted = ((IFormattable)formattableString).ToString("", formatProvider);

            if (formatProvider.Parts.Count == 0)
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
                            yield return Format(formatProvider.Parts[partIndex++]);
                        }
                        else
                        {
                            yield return new ContentSpan(match.Value);
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
                Parts.Add(arg);

                return "";
            }

            public List<object> Parts { get; } = new List<object>();
        }
    }
}
