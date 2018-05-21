using System.Collections.Generic;
using System.Linq;

namespace System.CommandLine
{
    public class HelpSection
    {
        private readonly HelpBuilder _builder;
        private readonly string _title;
        private readonly string _description;
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

        public HelpSection(
            HelpBuilder builder,
            string title,
            string description)
        {
            _builder = builder;
            _title = title;
            _description = description;
        }

        internal virtual void Build()
        {
            if (!ShouldBuild())
            {
                return;
            }

            _builder.AddBlankLine();
            AddHeading();
            _builder.Indent();
            AddDescription();
            AddItems();
            _builder.Dedent();
            _builder.AddBlankLine();
        }

        protected virtual bool ShouldBuild()
        {
            if (!string.IsNullOrWhiteSpace(_description))
            {
                return true;
            }

            return _items != null && _items.Any();
        }

        protected virtual void AddHeading()
        {
            if (string.IsNullOrWhiteSpace(_title))
            {
                return;
            }

            _builder.AddLine(_title);
        }

        protected virtual void AddDescription()
        {
            if (string.IsNullOrWhiteSpace(_description))
            {
                return;
            }

            _builder.AddLine(_description);
        }

        protected virtual void AddItems()
        {
            if (_items == null || !_items.Any())
            {
                return;
            }

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
