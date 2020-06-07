// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace System.CommandLine.Hosting
{
    internal class CommandLineExecutorService : IHostedService
    {
        private readonly ILogger<CommandLineExecutorService> logger;
        private readonly IHostApplicationLifetime appLifetime;
        private readonly IHost host;
        private readonly InvocationContext invocation;
        private readonly ParseResult parseResult;

        public CommandLineExecutorService(
            ILogger<CommandLineExecutorService> logger,
            IHostApplicationLifetime appLifetime,
            IHost host,
            InvocationContext invocation,
            ParseResult parseResult)
        {
            this.logger = logger;
            this.appLifetime = appLifetime;
            this.host = host;
            this.invocation = invocation;
            this.parseResult = parseResult;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            invocation.BindingContext.AddService(typeof(IHost), _ => host);
            if(parseResult.Errors.Any()) 
            {
                logger.LogWarning($"Executing {nameof(Parser)} with errors: {parseResult.Errors.Count}");
            }
            await parseResult.InvokeAsync();
            appLifetime.StopApplication();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
