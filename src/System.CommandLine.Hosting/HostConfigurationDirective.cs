using System.Collections.Generic;
using System.Linq;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

using System.CommandLine.Parsing;

namespace System.CommandLine.Hosting;

public class HostConfigurationDirective() : Directive(Name)
{
    public new const string Name = "config";

    internal static void ConfigureHostBuilder(IHostBuilder hostBuilder)
    {
        var parseResult = hostBuilder.GetParseResult();
        if (parseResult.Configuration.RootCommand is RootCommand rootCommand &&
            rootCommand.Directives.FirstOrDefault(IsConfigDirective)
                is Directive configDirective &&
            parseResult.GetResult(configDirective)
                is DirectiveResult configResult
            )
        {
            var configKvps = configResult.Values.Select(GetKeyValuePair)
                .ToList();
            hostBuilder.ConfigureHostConfiguration(
                (config) => config.AddInMemoryCollection(configKvps)
                );
        }

        static bool IsConfigDirective(Directive directive) => 
            string.Equals(directive.Name, Name, StringComparison.OrdinalIgnoreCase);

        [Diagnostics.CodeAnalysis.SuppressMessage(
            "Style",
            "IDE0057: Use range operator",
            Justification = ".NET Standard 2.0"
            )]
        static KeyValuePair<string, string?> GetKeyValuePair(string configDirective)
        {
            ReadOnlySpan<char> kvpSpan = configDirective.AsSpan();
            int eqlIdx = kvpSpan.IndexOf('=');
            string key;
            string? value = default;
            if (eqlIdx < 0)
                key = kvpSpan.Trim().ToString();
            else
            {
                key = kvpSpan.Slice(0, eqlIdx).Trim().ToString();
                value = kvpSpan.Slice(eqlIdx + 1).Trim().ToString();
            }
            return new KeyValuePair<string, string?>(key, value);
        }
    }
}
