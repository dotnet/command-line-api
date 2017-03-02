namespace Microsoft.DotNet.Cli.CommandLine.SampleParsers.Dotnet
{
    public static class ArgumentsRuleExtensions
    {
        public static ArgumentsRule Named(
            this ArgumentsRule rule,
            string name)
        {
            return rule;
        }

        public static ArgumentsRule Default(
            this ArgumentsRule rule,
            string name)
        {
            return rule;
        }

        public static ArgumentsRule Description(
            this ArgumentsRule rule,
            string description)
        {
            return rule;
        }
    }
}