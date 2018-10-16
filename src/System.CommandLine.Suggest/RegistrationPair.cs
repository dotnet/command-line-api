namespace System.CommandLine.Suggest
{
    public struct RegistrationPair : IEquatable<RegistrationPair>
    {
        public RegistrationPair(string commandPath, string suggestionCommand)
        {
            CommandPath = commandPath ?? throw new ArgumentNullException(nameof(commandPath));
            SuggestionCommand = suggestionCommand ?? throw new ArgumentNullException(nameof(suggestionCommand));
        }

        public RegistrationPair(string suggestionCompletionLine)
        {
            var keyValuePair = suggestionCompletionLine.Split(new[] {'='}, 2);
            if (keyValuePair.Length < 2)
            {
                throw new ArgumentException(
                    $"Syntax for configuration of '{suggestionCompletionLine}' is not of the format '<command>=<value>'",
                    nameof(suggestionCompletionLine));
            }

            CommandPath = keyValuePair[0];
            SuggestionCommand = keyValuePair[1];
        }

        public string CommandPath { get; }
        public string SuggestionCommand { get; }

        public override bool Equals(object obj)
        {
            return obj is RegistrationPair pair && Equals(pair);
        }

        public bool Equals(RegistrationPair other)
        {
            return string.Equals(CommandPath, other.CommandPath, StringComparison.Ordinal) &&
                   string.Equals(SuggestionCommand, other.SuggestionCommand, StringComparison.Ordinal);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(StringComparer.OrdinalIgnoreCase.GetHashCode(CommandPath),
                StringComparer.OrdinalIgnoreCase.GetHashCode(SuggestionCommand));
        }

        public static bool operator ==(RegistrationPair pair1, RegistrationPair pair2)
        {
            return pair1.Equals(pair2);
        }

        public static bool operator !=(RegistrationPair pair1, RegistrationPair pair2)
        {
            return !(pair1 == pair2);
        }
    }
}
