using System.Collections.Generic;
using System.CommandLine.Parsing;
using System.Linq;

using Microsoft.Extensions.Configuration;

namespace System.CommandLine.Hosting
{
    public static class DirectiveConfigurationExtensions
    {
        public static IConfigurationBuilder AddCommandLineDirectives(
            this IConfigurationBuilder config, ParseResult commandLine,
            string name)
        {
            if (commandLine is null)
                throw new ArgumentNullException(nameof(commandLine));
            if (name is null)
                throw new ArgumentNullException(nameof(name));

            if (!commandLine.Directives.TryGetValues(name, out var directives))
                return config;

            var kvpSeparator = new[] { '=' };
            return config.AddInMemoryCollection(directives.Select(s =>
            {
                var parts = s.Split(kvpSeparator, count: 2);
                var key = parts[0];
                var value = parts.Length > 1 ? parts[1] : null;
                return new KeyValuePair<string, string?>(key, value);
            }));
        }
    }
}
