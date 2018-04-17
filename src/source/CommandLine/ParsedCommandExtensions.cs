using System;

namespace Microsoft.DotNet.Cli.CommandLine
{
    public static class ParsedCommandExtensions
    {
        public static object ValueForOption(
            this ParsedCommand parsedCommand, 
            string alias)
        {
            if (string.IsNullOrWhiteSpace(alias))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(alias));
            }

            return parsedCommand.Children[alias].Value();
        }

        public static T ValueForOption<T>(
            this ParsedCommand parsedCommand, 
            string alias)
        {
            if (string.IsNullOrWhiteSpace(alias))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(alias));
            }

            return parsedCommand.Children[alias].Value<T>();
        }
    }
}