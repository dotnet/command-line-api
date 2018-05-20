using System.Collections.Generic;

namespace System.CommandLine.DragonFruit
{
    internal class CommandHelpMetadata
    {
        private readonly Dictionary<string, string> _parameters = new Dictionary<string, string>();

        public string Description { get; set; }
        public string Name { get; set; }

        public void AddParameter(string parameterName, string description)
        {
            if (string.IsNullOrWhiteSpace(description))
            {
                description = null;
            }

            _parameters.Add(parameterName, description);
        }

        public bool TryGetParameterDescription(string parameterName, out string description)
            => _parameters.TryGetValue(parameterName, out description);
    }
}
