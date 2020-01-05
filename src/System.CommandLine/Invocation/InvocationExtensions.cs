// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Binding;
using System.CommandLine.Builder;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static System.Environment;

namespace System.CommandLine.Invocation
{
    public static class InvocationExtensions
    {
        public static CommandLineBuilder CancelOnProcessTermination(this CommandLineBuilder builder)
        {
            builder.AddMiddleware(async (context, next) =>
            {
                bool cancellationHandlingAdded = false;
                ManualResetEventSlim blockProcessExit = null;
                ConsoleCancelEventHandler consoleHandler = null;
                EventHandler processExitHandler = null;

                context.CancellationHandlingAdded += (CancellationTokenSource cts) =>
                {
                    cancellationHandlingAdded = true;
                    blockProcessExit = new ManualResetEventSlim(initialState: false);
                    consoleHandler = (_, args) =>
                    {
                        cts.Cancel();
                        // Stop the process from terminating.
                        // Since the context was cancelled, the invocation should
                        // finish and Main will return.
                        args.Cancel = true;
                    };
                    processExitHandler = (_1, _2) =>
                    {
                        cts.Cancel();
                        // The process exits as soon as the event handler returns.
                        // We provide a return value using Environment.ExitCode
                        // because Main will not finish executing.
                        // Wait for the invocation to finish.
                        blockProcessExit.Wait();
                        Environment.ExitCode = context.ResultCode;
                    };
                    Console.CancelKeyPress += consoleHandler;
                    AppDomain.CurrentDomain.ProcessExit += processExitHandler;
                };

                try
                {
                    await next(context);
                }
                finally
                {
                    if (cancellationHandlingAdded)
                    {
                        Console.CancelKeyPress -= consoleHandler;
                        AppDomain.CurrentDomain.ProcessExit -= processExitHandler;
                        blockProcessExit.Set();
                    }
                }
            }, CommandLineBuilder.MiddlewareOrder.ProcessExit);

            return builder;
        }

        public static CommandLineBuilder ConfigureConsole(
            this CommandLineBuilder builder,
            Func<BindingContext, IConsole> createConsole)
        {
            builder.AddMiddleware(async (context, next) =>
            {
                context.BindingContext.ConsoleFactory = new AnonymousConsoleFactory(createConsole);
                await next(context);
            }, CommandLineBuilder.MiddlewareOrder.Middle);

            return builder;
        }

        public static CommandLineBuilder UseMiddleware(
            this CommandLineBuilder builder,
            InvocationMiddleware middleware)
        {
            builder.AddMiddleware(
                middleware,
                CommandLineBuilder.MiddlewareOrder.Middle);

            return builder;
        }

        public static CommandLineBuilder UseMiddleware(
            this CommandLineBuilder builder,
            Action<InvocationContext> onInvoke)
        {
            builder.AddMiddleware(async (context, next) =>
            {
                onInvoke(context);
                await next(context);
            }, CommandLineBuilder.MiddlewareOrder.Middle);

            return builder;
        }

        public static CommandLineBuilder UseDebugDirective(
            this CommandLineBuilder builder)
        {
            builder.AddMiddleware(async (context, next) =>
            {
                if (context.ParseResult.Directives.Contains("debug"))
                {
                    var process = Diagnostics.Process.GetCurrentProcess();

                    var processId = process.Id;

                    context.Console.Out.WriteLine($"Attach your debugger to process {processId} ({process.ProcessName}).");

                    while (!Debugger.IsAttached)
                    {
                        await Task.Delay(500);
                    }
                }

                await next(context);
            }, CommandLineBuilder.MiddlewareOrder.ExceptionHandler - 1);

            return builder;
        }

        public static CommandLineBuilder UseExceptionHandler(
            this CommandLineBuilder builder,
            Action<Exception, InvocationContext> onException = null)
        {
            builder.AddMiddleware(async (context, next) =>
            {
                try
                {
                    await next(context);
                }
                catch (Exception exception)
                {
                    (onException ?? Default)(exception, context);
                }
            }, order: CommandLineBuilder.MiddlewareOrder.ExceptionHandler);

            return builder;

            void Default(Exception exception, InvocationContext context)
            {
                context.Console.ResetTerminalForegroundColor();
                context.Console.SetTerminalForegroundRed();

                context.Console.Error.Write("Unhandled exception: ");
                context.Console.Error.WriteLine(exception.ToString());

                context.Console.ResetTerminalForegroundColor();

                context.ResultCode = 1;
            }
        }

        public static CommandLineBuilder UseParseDirective(
            this CommandLineBuilder builder)
        {
            builder.AddMiddleware(async (context, next) =>
            {
                if (context.ParseResult.Directives.Contains("parse"))
                {
                    context.InvocationResult = new ParseDirectiveResult();
                }
                else
                {
                    await next(context);
                }
            }, CommandLineBuilder.MiddlewareOrder.Preprocessing);

            return builder;
        }

        public static CommandLineBuilder UseSuggestDirective(
            this CommandLineBuilder builder)
        {
            builder.AddMiddleware(async (context, next) =>
            {
                if (context.ParseResult.Directives.TryGetValues("suggest", out var values))
                {
                    int position;

                    if (values.FirstOrDefault() is string positionString)
                    {
                        position = int.Parse(positionString);
                    }
                    else
                    {
                        position = context.ParseResult.RawInput?.Length ?? 0;
                    }

                    context.InvocationResult = new SuggestDirectiveResult(position);
                }
                else
                {
                    await next(context);
                }
            }, CommandLineBuilder.MiddlewareOrder.Preprocessing);

            return builder;
        }

        public static CommandLineBuilder UseTypoCorrections(
            this CommandLineBuilder builder, int maxLevenshteinDistance = 3)
        {
            builder.AddMiddleware(async (context, next) =>
            {
                if (context.ParseResult.UnmatchedTokens.Any() &&
                    context.ParseResult.CommandResult.Command.TreatUnmatchedTokensAsErrors)
                {
                    var typoCorrection = new TypoCorrection(maxLevenshteinDistance);
                    
                    typoCorrection.ProvideSuggestions(context.ParseResult, context.Console);
                }
                await next(context);
            }, CommandLineBuilder.MiddlewareOrder.Preprocessing);

            return builder;
        }

        public static async Task<int> InvokeAsync(
            this Parser parser,
            ParseResult parseResult,
            IConsole console = null) =>
            await new InvocationPipeline(parseResult)
                .InvokeAsync(console);

        public static Task<int> InvokeAsync(
            this Parser parser,
            string commandLine,
            IConsole console = null) =>
            parser.InvokeAsync(commandLine.SplitCommandLine().ToArray(), console);

        public static async Task<int> InvokeAsync(
            this Parser parser,
            string[] args,
            IConsole console = null) =>
            await parser.InvokeAsync(parser.Parse(args), console);

        public static Task<int> InvokeAsync(
            this Command command,
            string commandLine,
            IConsole console = null) =>
            command.InvokeAsync(commandLine.SplitCommandLine().ToArray(), console);

        public static async Task<int> InvokeAsync(
            this Command command,
            string[] args,
            IConsole console = null)
        {
            return await GetInvocationPipeline(command, args).InvokeAsync(console);
        }

        public static int Invoke(
            this Parser parser,
            ParseResult parseResult,
            IConsole console = null) =>
            new InvocationPipeline(parseResult).Invoke(console);

        public static int Invoke(
            this Parser parser,
            string commandLine,
            IConsole console = null) =>
            parser.Invoke(commandLine.SplitCommandLine().ToArray(), console);

        public static int Invoke(
            this Parser parser,
            string[] args,
            IConsole console = null) =>
            parser.Invoke(parser.Parse(args), console);

        public static int Invoke(
            this Command command,
            string commandLine,
            IConsole console = null) =>
            command.Invoke(commandLine.SplitCommandLine().ToArray(), console);

        public static int Invoke(
            this Command command,
            string[] args,
            IConsole console = null)
        {
            return GetInvocationPipeline(command, args).Invoke(console);
        }

        private static InvocationPipeline GetInvocationPipeline(Command command, string[] args)
        {
            var parser = new CommandLineBuilder(command)
                .UseDefaults()
                .Build();

            ParseResult parseResult = parser.Parse(args);

            return new InvocationPipeline(parseResult);
        }

        public static CommandLineBuilder UseHelp(this CommandLineBuilder builder)
        {
            builder.AddMiddleware(async (context, next) =>
            {
                var helpOptionTokens = new HashSet<string>();
                var prefixes = context.Parser.Configuration.Prefixes;
                if (prefixes == null)
                {
                    helpOptionTokens.Add("-h");
                    helpOptionTokens.Add("/h");
                    helpOptionTokens.Add("--help");
                    helpOptionTokens.Add("-?");
                    helpOptionTokens.Add("/?");
                }
                else
                {
                    string[] helpOptionNames = { "help", "h", "?" };
                    foreach (var helpOption in helpOptionNames)
                    {
                        foreach (var prefix in prefixes)
                        {
                            helpOptionTokens.Add($"{prefix}{helpOption}");
                        }
                    }
                }

                if (!ShowHelp(context, helpOptionTokens))
                {
                    await next(context);
                }
            }, CommandLineBuilder.MiddlewareOrder.Preprocessing);

            return builder;
        }

        public static CommandLineBuilder UseHelp(
            this CommandLineBuilder builder,
            IReadOnlyCollection<string> helpOptionTokens)
        {
            builder.AddMiddleware(async (context, next) =>
            {
                if (!ShowHelp(context, helpOptionTokens))
                {
                    await next(context);
                }
            }, CommandLineBuilder.MiddlewareOrder.Preprocessing);
            return builder;
        }

        public static CommandLineBuilder UseParseErrorReporting(
            this CommandLineBuilder builder)
        {
            builder.AddMiddleware(async (context, next) =>
            {
                if (context.ParseResult.Errors.Count > 0)
                {
                    context.InvocationResult = new ParseErrorResult();
                }
                else
                {
                    await next(context);
                }
            }, CommandLineBuilder.MiddlewareOrder.AfterPreprocessing);
            return builder;
        }

        public static CommandLineBuilder RegisterWithDotnetSuggest(
            this CommandLineBuilder builder)
        {
            builder.AddMiddleware(async (context, next) =>
            {
                var feature = new FeatureRegistration("dotnet-suggest-registration");

                await feature.EnsureRegistered(async () =>
                {
                    try
                    {
                        var currentProcessFullPath = Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
                        var currentProcessFileNameWithoutExtension = Path.GetFileNameWithoutExtension(currentProcessFullPath);

                        var stdOut = new StringBuilder();
                        var stdErr = new StringBuilder();

                        var dotnetSuggestProcess = Process.StartProcess(
                            command: "dotnet-suggest",
                            args: $"register --command-path \"{currentProcessFullPath}\" --suggestion-command \"{currentProcessFileNameWithoutExtension}\"",
                            stdOut: value => stdOut.Append(value),
                            stdErr: value => stdOut.Append(value));

                        await dotnetSuggestProcess.CompleteAsync();

                        return $"{dotnetSuggestProcess.StartInfo.FileName} exited with code {dotnetSuggestProcess.ExitCode}{NewLine}OUT:{NewLine}{stdOut}{NewLine}ERR:{NewLine}{stdErr}";
                    }
                    catch (Exception exception)
                    {
                        return $"Exception during registration:{NewLine}{exception}";
                    }
                });

                await next(context);
            }, CommandLineBuilder.MiddlewareOrder.Configuration);

            return builder;
        }

        private static bool ShowHelp(
            InvocationContext context,
            IReadOnlyCollection<string> helpOptionAliases)
        {
            var lastToken = context.ParseResult.Tokens.LastOrDefault();

            if (helpOptionAliases.Contains(lastToken?.Value) &&
                !TokenIsDefinedInSyntax())
            {
                context.InvocationResult = new HelpResult();
                return true;
            }

            return false;

            bool TokenIsDefinedInSyntax() =>
                context.Parser
                       .Configuration
                       .Symbols
                       .FlattenBreadthFirst(s => s.Children)
                       .SelectMany(s => s.RawAliases)
                       .Any(helpOptionAliases.Contains);
        }
    }
}
