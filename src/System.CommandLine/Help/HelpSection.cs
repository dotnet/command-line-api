using System.Collections.Generic;
using System.Linq;

namespace System.CommandLine
{
    public class HelpSection
    {
        private readonly HelpBuilder _builder;
        private readonly string _title;
        private readonly string _description;
        private readonly IReadOnlyCollection<SymbolDefinition> _usageItems;
        private readonly Func<SymbolDefinition, HelpItem> _formatter;

        public HelpSection(
            HelpBuilder builder,
            string title,
            IReadOnlyCollection<SymbolDefinition> usageItems,
            Func<SymbolDefinition, HelpItem> formatter)
        {
            _builder = builder;
            _title = title;
            _usageItems = usageItems;
            _formatter = formatter;
        }

        public HelpSection(HelpBuilder builder, string title, string description)
        {
            _builder = builder;
            _title = title;
            _description = description;
        }

        public void Build()
        {
            if (!ShouldBuild())
            {
                return;
            }

            AddHeading();
            _builder.Indent();
            AddDescription();
            AddUsage();
            _builder.Outdent();
        }

        protected virtual bool ShouldBuild()
        {
            if (!string.IsNullOrWhiteSpace(_description))
            {
                return true;
            }

            return _usageItems?.Any() == true;
        }

        protected virtual void AddHeading()
        {
            if (string.IsNullOrWhiteSpace(_title))
            {
                return;
            }

            _builder.AddHeading(_title);
            _builder.AddBlankLine();
        }

        protected virtual void AddDescription()
        {
            if (string.IsNullOrWhiteSpace(_description))
            {
                return;
            }

            _builder.AddLine(_description);
            _builder.AddBlankLine();
        }

        protected virtual void AddUsage()
        {
            if (_usageItems?.Any() != true)
            {
                return;
            }

            var helpItems = _usageItems
                .Select(item => _formatter(item));

            var maxWidth = helpItems
                .Select(line => line.Usage.Length)
                .OrderByDescending(textLength => textLength)
                .First();

            foreach (var helpItem in helpItems)
            {
                _builder.AddHelpItem(helpItem, maxWidth);
            }

            _builder.AddBlankLine();
        }
    }
}
