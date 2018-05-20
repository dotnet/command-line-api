using System.Collections.Generic;
using System.Linq;

namespace System.CommandLine
{
    public class HelpSection
    {
        private readonly int _maxWidth;

        protected HelpSection(int maxWidth)
        {
            _maxWidth = maxWidth;
        }

        protected string Title { get; set; }

        protected IReadOnlyCollection<HelpDefinition> HelpDefinitions { get; set; }

        protected Func<HelpDefinition, string> NameFormatter { get; set; }

        protected Func<HelpDefinition, int, string> DescriptionFormatter { get; set; }

        protected virtual void AddSection(HelpBuilder builder)
        {
            AddHeading(builder);
            builder.Indent();
            AddContent(builder);
            builder.Dedent();
        }

        protected virtual void AddHeading(HelpBuilder builder)
        {
            if (string.IsNullOrWhiteSpace(Title))
            {
                return;
            }

            builder.AddLine(Title);
        }

        protected virtual void AddContent(HelpBuilder builder)
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
                builder.AddSectionColumns(
                    formattedName,
                    paddingWidth,
                    DescriptionFormatter(helpDefinition, descriptionWidth));
            }
        }
    }
}
