// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Invocation;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

#if NETSTANDARD2_0
using IHostEnvironment = Microsoft.Extensions.Hosting.IHostingEnvironment;
using IHostApplicationLifetime = Microsoft.Extensions.Hosting.IApplicationLifetime;
#endif

namespace System.CommandLine.Hosting
{
    public class InvocationLifetime : IHostLifetime
    {
        private readonly CancellationToken invokeCancelToken;
        private CancellationTokenRegistration invokeCancelReg;
        private CancellationTokenRegistration appStartedReg;
        private CancellationTokenRegistration appStoppingReg;

        public InvocationLifetime(
            IOptions<InvocationLifetimeOptions> options,
            IHostEnvironment environment,
            IHostApplicationLifetime applicationLifetime,
            InvocationContext context = null,
            ILoggerFactory loggerFactory = null)
        {
            Options = options?.Value ?? throw new ArgumentNullException(nameof(options));
            Environment = environment
                ?? throw new ArgumentNullException(nameof(environment));
            ApplicationLifetime = applicationLifetime
                ?? throw new ArgumentNullException(nameof(applicationLifetime));

            // if InvocationLifetime is added outside of a System.CommandLine
            // invocation pipeline context will be null.
            // Use default cancellation token instead, and become a noop lifetime.
            invokeCancelToken = context?.GetCancellationToken() ?? default;

            Logger = (loggerFactory ?? NullLoggerFactory.Instance)
                .CreateLogger("Microsoft.Hosting.Lifetime");
        }

        public InvocationLifetimeOptions Options { get; }
        private ILogger Logger { get; }
        public IHostEnvironment Environment { get; }
        public IHostApplicationLifetime ApplicationLifetime { get; }

        public Task WaitForStartAsync(CancellationToken cancellationToken)
        {
            if (!Options.SuppressStatusMessages)
            {
                appStartedReg = ApplicationLifetime.ApplicationStarted.Register(state =>
                {
                    ((InvocationLifetime)state).OnApplicationStarted();
                }, this);
                appStoppingReg = ApplicationLifetime.ApplicationStopping.Register(state =>
                {
                    ((InvocationLifetime)state).OnApplicationStopping();
                }, this);
            }

            invokeCancelReg = invokeCancelToken.Register(state =>
            {
                ((InvocationLifetime)state).OnInvocationCancelled();
            }, this);

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            // There's nothing to do here
            return Task.CompletedTask;
        }

        private void OnInvocationCancelled()
        {
            ApplicationLifetime.StopApplication();
        }

        private void OnApplicationStarted()
        {
            Logger.LogInformation("Application started. Press Ctrl+C to shut down.");
            Logger.LogInformation("Hosting environment: {envName}", Environment.EnvironmentName);
            Logger.LogInformation("Content root path: {contentRoot}", Environment.ContentRootPath);
        }

        private void OnApplicationStopping()
        {
            Logger.LogInformation("Application is shutting down...");
        }

        public void Dispose()
        {
            invokeCancelReg.Dispose();
            appStartedReg.Dispose();
            appStoppingReg.Dispose();
        }
    }
}
