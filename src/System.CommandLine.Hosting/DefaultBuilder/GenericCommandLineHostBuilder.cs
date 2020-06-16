// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace System.CommandLine.Hosting
{
    internal class GenericCommandLineHostBuilder
    {

        private const string ConfigurationDirectiveName = "config";
        public string[] Args { get; }
        private readonly IHostBuilder builder;
        private CommandLineBuilder commandLineBuilder;

        public GenericCommandLineHostBuilder(IHostBuilder builder, string[] args = default)
        {
            this.builder = builder;
            commandLineBuilder = new CommandLineBuilder();
            Args = args;
        }

        public void Configure(Action<CommandLineBuilder> configure)
        {
            configure(commandLineBuilder);
            Parser parser = commandLineBuilder.Build();
            ParseResult parseResult = parser.Parse(Args);
            var invocation = new InvocationContext(parseResult);
            AddSystemCommandLine(builder, invocation);
            builder.ConfigureHostConfiguration(config =>
            {
                config.AddCommandLineDirectives(invocation.ParseResult, ConfigurationDirectiveName);
            });
        }

        private static void AddSystemCommandLine(IHostBuilder host, InvocationContext invocation)
        {
            host.ConfigureServices(services =>
            {
                services.TryAddSingleton<InvocationContext>(invocation);
                services.AddSingleton<IConsole>(invocation.Console);
                // semantically it is transient dependency
                services.AddTransient<IInvocationResult>(_ => invocation.InvocationResult);
                services.AddTransient<ParseResult>(_ => invocation.ParseResult);
            });
        }

    }
}
