// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.CommandLine.Builder;
using Microsoft.Extensions.DependencyInjection;
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
            AddGenericCommandLineHostBuilder(builder, args, out var cmdHostBuilder);
            cmdHostBuilder.Configure(configure);
            builder.ConfigureServices((context, services) =>
                services.AddHostedService<CommandLineExecutorService>());
            return builder;
        }

        /// <summary>
        /// Adds <see cref="AddGenericCommandLineHostBuilder"/> to <paramref name="builder"/>.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="args"></param>
        /// <param name="cmdHostBuilder"></param>
        /// <returns><see langword="true"/> when new builder is intialized</returns>
        private static bool AddGenericCommandLineHostBuilder(IHostBuilder builder, string[] args, out GenericCommandLineHostBuilder cmdHostBuilder)
        {
            var hostBuilderState = builder.Properties;
            var cacheKey = nameof(GenericCommandLineHostBuilder);
            hostBuilderState.TryGetValue(cacheKey, out var initializedHost);
            cmdHostBuilder = initializedHost as GenericCommandLineHostBuilder;
            if (cmdHostBuilder is object)
            {
                return false;
            }
            cmdHostBuilder = new GenericCommandLineHostBuilder(builder, args);
            hostBuilderState[cacheKey] = cmdHostBuilder;
            return true;
        }
    }
}
