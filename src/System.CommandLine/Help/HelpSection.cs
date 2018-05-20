using System.Collections.Generic;
using System.Linq;

namespace System.CommandLine
{
    public class HelpSection
    {
        private readonly int _maxWidth;

        public HelpSection(int maxWidth)
        {
            _maxWidth = maxWidth;
        }

        public string Title { get; set; }

        public IReadOnlyCollection<HelpDefinition> HelpDefinitions { get; set; }

        public Func<HelpDefinition, string> NameFormatter { get; set; }

        public Func<HelpDefinition, int, string> DescriptionFormatter { get; set; }

        public virtual void BuildSection(HelpBuilder builder)
        {
            AddHeading(builder);
            AddContent(builder);
        }

        public virtual void AddHeading(HelpBuilder builder)
        {
            if (!string.IsNullOrWhiteSpace(Title))
            {
                builder.AddIndentedText(Title);
            }
        }

        public virtual void AddContent(HelpBuilder builder)
        {
            var helpText = HelpDefinitions.ToDictionary(helpDef => helpDef, NameFormatter);

            var maxWidth = helpText.Values
                .Select(helpName => helpName.Length)
                .OrderByDescending(textLength => textLength)
                .First();

            var availableWidth = builder.GetAvailableWidth();
            var descriptionWidth = availableWidth - maxWidth;

            foreach (var helpDefinition in HelpDefinitions)
            {
                var formattedName = helpText[helpDefinition];
                var paddingWidth = availableWidth - formattedName.Length;
                builder.AddLine(formattedName, paddingWidth, DescriptionFormatter(helpDefinition, descriptionWidth));
            }
        }
    }
}
