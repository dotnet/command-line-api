using System.Collections.Generic;
using System.Linq;

namespace System.CommandLine
{
    public class HelpSection
    {
        private readonly HelpBuilder _builder;
        private readonly string _title;
        private IReadOnlyCollection<SymbolDefinition> _items;
        private readonly Func<SymbolDefinition, Tuple<string, string>> _formatter;

        public HelpSection(
            HelpBuilder builder,
            string title,
            IReadOnlyCollection<SymbolDefinition> items,
            Func<SymbolDefinition, Tuple<string, string>> formatter)
        {
            _builder = builder;
            _title = title;
            _items = items;
            _formatter = formatter;
        }

        internal virtual void Build()
        {
            _builder.AddBlankLine();
            AddHeading();
            _builder.Indent();
            AddContent();
            _builder.Dedent();
            _builder.AddBlankLine();
        }

        protected virtual void AddHeading()
        {
            if (string.IsNullOrWhiteSpace(_title))
            {
                return;
            }

            _builder.AddLine(_title);
        }

        protected virtual void AddContent()
        {
            var helpLines = _items
                .Select(item => _formatter(item));

            var maxWidth = helpLines
                .Select(line => line.Item1.Length)
                .OrderByDescending(textLength => textLength)
                .First();

            foreach (var line in helpLines)
            {
                _builder.AddSectionColumns(line.Item1, line.Item2, maxWidth);
            }
        }
    }
}
