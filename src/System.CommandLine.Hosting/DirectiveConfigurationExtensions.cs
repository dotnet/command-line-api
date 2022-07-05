using System.Collections.Generic;
using System.Linq;

using Microsoft.Extensions.Configuration;

namespace System.CommandLine.Hosting
{
    public static class DirectiveConfigurationExtensions
    {
        public static IConfigurationBuilder AddCommandLineDirectives(
            this IConfigurationBuilder config, ParseResult commandline,
            string name)
        {
            if (commandline is null)
                throw new ArgumentNullException(nameof(commandline));
            if (name is null)
                throw new ArgumentNullException(nameof(name));

            if (!commandline.Directives.TryGetValues(name, out var directives))
                return config;

            var kvpSeparator = new[] { '=' };
            return config.AddInMemoryCollection(directives.Select(s =>
            {
                var parts = s.Split(kvpSeparator, count: 2);
                var key = parts[0];
                var value = parts.Length > 1 ? parts[1] : null;
                return new KeyValuePair<string, string>(key, value);
            }).ToList());
        }
    }
}
