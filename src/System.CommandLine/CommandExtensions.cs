using System;
using System.Linq;

namespace System.CommandLine
{
    public static class CommandExtensions
    {
        public static Command Subcommand(
            this Command command,
            string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(name));
            }

            return command.DefinedSymbols.OfType<Command>().Single(c => c.Name == name);
        }
    }
}
