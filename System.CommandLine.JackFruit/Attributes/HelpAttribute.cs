namespace System.CommandLine.JackFruit
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.Method)]
    public sealed class HelpAttribute : Attribute
    {
        // This is a positional argument
        public HelpAttribute(string helpText)
        {
            HelpText = helpText;
        }

        public string HelpText { get; }
    }
}
