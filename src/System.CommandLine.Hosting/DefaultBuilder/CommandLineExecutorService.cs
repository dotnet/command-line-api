// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace System.CommandLine.Hosting
{
    internal class CommandLineExecutorService : IHostedService
    {
        private readonly ILoggerFactory loggerFactory;
        private readonly IHostApplicationLifetime appLifetime;
        private readonly IHost host;
        private readonly InvocationContext invocation;
        private readonly ParseResult parseResult;

        public CommandLineExecutorService(
            ILoggerFactory loggerFactory,
            IConfiguration configuration,
            IHostApplicationLifetime appLifetime,
            IHost host,
            InvocationContext invocation,
            ParseResult parseResult)
        {
            this.appLifetime = appLifetime;
            this.host = host;
            this.invocation = invocation;
            this.parseResult = parseResult;
            this.loggerFactory = loggerFactory;
            Configuration = configuration;
        }

        public ILogger Logger { get; }
        // Only for high level lifetime events
        public IConfiguration Configuration { get; }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            // TODO: consider how to make more robust
            invocation.BindingContext.AddService(typeof(IHost), _ => host);
            await parseResult.InvokeAsync();
            appLifetime.StopApplication();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
