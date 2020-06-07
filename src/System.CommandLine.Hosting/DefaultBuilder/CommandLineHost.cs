using Microsoft.Extensions.Hosting;
using System.CommandLine.Builder;

namespace System.CommandLine.Hosting
{
    /// <summary>
    /// Provides convenience methods for creating instances of <see cref="IHostBuilder"/> and with pre-configured defaults.
    /// </summary>
    public static class CommandLineHost
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IHostBuilder"/> with pre-configured defaults.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <returns>The initialized <see cref="IHostBuilder"/>.</returns>
        public static IHostBuilder CreateDefaultBuilder() =>
            CreateDefaultBuilder(args: null);

        /// <summary>
        /// Initializes a new instance of the <see cref="IHostBuilder"/> with pre-configured defaults.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="args">The command line args.</param>
        /// <returns>The initialized <see cref="IHost"/>.</returns>
        public static IHostBuilder CreateDefaultBuilder(string[] args)
        {
            var builder = Host.CreateDefaultBuilder(args)
                .ConfigureCommandLineDefaults(cmdBuilder => cmdBuilder.UseDefaults(), args);
            return builder;
            // TODO: add remaining args parsing
            // var argsRemaining = invocation.ParseResult.UnparsedTokens.ToArray();
        }
    }
}
