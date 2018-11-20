namespace System.CommandLine.JackFruit
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property)]
    public sealed class AliasAttribute : Attribute
    {

        // This is a positional argument
        public AliasAttribute(params string[] aliases)
        {
            Aliases = aliases;
        }

        public string[] Aliases { get; }
    }
}
