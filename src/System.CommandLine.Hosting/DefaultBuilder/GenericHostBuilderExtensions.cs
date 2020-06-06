using System.CommandLine.Builder;
using Microsoft.Extensions.DependencyInjection;
// using System.CommandLine.Hosting;
using Microsoft.Extensions.Hosting;

namespace System.CommandLine.Hosting
{
    /// <summary>
    /// Extension methods for configuring the IWebHostBuilder.
    /// </summary>
    public static class GenericHostBuilderExtensions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IHostBuilder"/> class with pre-configured defaults.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="builder">The <see cref="IHostBuilder" /> instance to configure</param>
        /// <param name="configure">The configure callback</param>
        /// <returns>The <see cref="IHostBuilder"/> for chaining.</returns>
        public static IHostBuilder ConfigureCommandLineDefaults(
            this IHostBuilder builder,
            Action<CommandLineBuilder> configure,
            string[] args = default)
        {
            var host = new GenericCommandLineHostBuilder(builder, args);
            host.Configure(configure);
            builder.ConfigureServices((context, services) => 
                services.AddHostedService<CommandLineExecutorService>());
            return builder;
        }
    }
}
