using System.Collections.Generic;
using System.CommandLine.Parsing;
using System.Linq;

using Microsoft.Extensions.Configuration;

namespace System.CommandLine.Hosting
{
    public static class DirectiveConfigurationExtensions
    {
        public static IConfigurationBuilder AddCommandLineDirectives(
            this IConfigurationBuilder config, ParseResult commandline,
            CliDirective directive)
        {
            if (commandline is null)
                throw new ArgumentNullException(nameof(commandline));
            if (directive is null)
                throw new ArgumentNullException(nameof(directive));

            if (commandline.GetResult(directive) is not DirectiveResult result
                || result.Values.Count == 0)
            {
                return config;
            }

            var kvpSeparator = new[] { '=' };
            return config.AddInMemoryCollection(result.Values.Select(s =>
            {
                var parts = s.Split(kvpSeparator, count: 2);
                var key = parts[0];
                var value = parts.Length > 1 ? parts[1] : null;
                return new KeyValuePair<string, string>(key, value);
            }).ToList());
        }
    }
}
