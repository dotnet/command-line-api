using System.Collections.Generic;

namespace JackFruit
{
    internal class DotnetJackFruitHelpTextProvider : IHelpDescription
    {
        private static Dictionary<string, string> toolDescriptions = new Dictionary<string, string>
        {
            [nameof(Tool)] = "Install or manage tools that extend the .NET experience.",
            [nameof(Tool) + "/" + nameof(Install)] = "Install a tool for use on the command line.",
            [nameof(Tool) + "/" + nameof(Install) + "/Global"] = "Install a tool for use on the command line.",
            ["More stuff"] = "More stuff"
        };

        public string GetHelp(string helpPath)
        {
            var notFound = "N/A";
            switch (helpPath)
            {
                case nameof(Tool):
                    return toolDescriptions.ValueOr(helpPath, notFound);
            }
            return notFound;
        }


    }

 
}
