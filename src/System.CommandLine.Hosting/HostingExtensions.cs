#nullable enable

using Microsoft.Extensions.Hosting;

namespace System.CommandLine.Hosting
{
    public static class HostingExtensions
    {
        public static ParseResult GetParseResult(this IHostBuilder hostBuilder)
        {
            _ = hostBuilder ?? throw new ArgumentNullException(nameof(hostBuilder));

            if (hostBuilder.Properties.TryGetValue(typeof(ParseResult), out var ctxObj) &&
                ctxObj is ParseResult invocationContext)
                return invocationContext;

            throw new InvalidOperationException("Host builder has no command-line parse result registered to it.");
        }

        public static ParseResult GetParseResult(this HostBuilderContext context)
        {
            _ = context ?? throw new ArgumentNullException(nameof(context));

            if (context.Properties.TryGetValue(typeof(ParseResult), out var ctxObj) &&
                ctxObj is ParseResult invocationContext)
                return invocationContext;

            throw new InvalidOperationException("Host builder context has no command-line parse result registered to it.");
        }
    }
}
