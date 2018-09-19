namespace System.CommandLine.Suggest
{
    public class RegistrationPair
    {
        public RegistrationPair(string commandPath, string suggestionCommand)
        {
            CommandPath = commandPath ?? throw new ArgumentNullException(nameof(commandPath));
            SuggestionCommand = suggestionCommand ?? throw new ArgumentNullException(nameof(suggestionCommand));
        }

        public RegistrationPair(string suggestionCompletionLine)
        {
            string[] keyValuePair = suggestionCompletionLine.Split(new[] { '=' }, 2);
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

    }
}
