// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Builder;
using System.IO;
using System.Linq;
using System.Reflection;
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
                ConsoleCancelEventHandler consoleHandler = (_, args) =>
                {
                    context.Cancel(out bool isCancelling);
                    if (isCancelling)
                    {
                        // Stop the process from terminating.
                        // Since the context was cancelled, the invocation should
                        // finish and Main will return.
                        args.Cancel = true;
                    }
                };
                var blockProcessExit = new ManualResetEventSlim(initialState: false);
                EventHandler processExitHandler = (_1, _2) =>
                {
                    // The process exits as soon as the event handler returns.
                    // We provide a return value using Environment.ExitCode
                    // because Main will not finish executing.
                    context.Cancel(out bool isCancelling);
                    if (isCancelling)
                    {
                        // Wait for the invocation to finish.
                        blockProcessExit.Wait();

                        Environment.ExitCode = context.ResultCode;
                    }
                    else
                    {
                        Environment.ExitCode = 1;
                    }
                };
                try
                {
                    Console.CancelKeyPress += consoleHandler;
                    AppDomain.CurrentDomain.ProcessExit += processExitHandler;
                    await next(context);
                }
                finally
                {
                    Console.CancelKeyPress -= consoleHandler;
                    AppDomain.CurrentDomain.ProcessExit -= processExitHandler;
                    blockProcessExit.Set();
                }
            }, CommandLineBuilder.MiddlewareOrder.ProcessExit);

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
                if (context.ParseResult.Tokens.Contains("[debug]"))
                {
                    var minusDirective = context.ParseResult
                                                .Tokens
                                                .Where(t => t != "[debug]")
                                                .ToArray();

                    context.ParseResult = context.Parser.Parse(minusDirective);

                    var processId = Diagnostics.Process.GetCurrentProcess().Id;

                    context.Console.Out.WriteLine($"Attach your debugger to process {processId} and then press any key.");

                    Console.ReadKey();
                }

                await next(context);
            }, CommandLineBuilder.MiddlewareOrder.Preprocessing);

            return builder;
        }

        public static CommandLineBuilder UseExceptionHandler(
            this CommandLineBuilder builder)
        {
            builder.AddMiddleware(async (context, next) => {
                try
                {
                    await next(context);
                }
                catch (Exception exception)
                {
                    context.Console.ResetColor();
                    context.Console.ForegroundColor = ConsoleColor.Red;
                    context.Console.Error.Write("Unhandled exception: ");
                    context.Console.Error.WriteLine(exception.ToString());
                    context.Console.ResetColor();
                    context.ResultCode = 1;
                }
            }, order: CommandLineBuilder.MiddlewareOrder.ExceptionHandler);

            return builder;
        }

        public static CommandLineBuilder UseParseDirective(
            this CommandLineBuilder builder)
        {
            builder.AddMiddleware(async (context, next) => {
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
            builder.AddMiddleware(async (context, next) => {
                if (context.ParseResult.Directives.Contains("suggest"))
                {
                    context.InvocationResult = new SuggestDirectiveResult();
                }
                else
                {
                    await next(context);
                }
            }, CommandLineBuilder.MiddlewareOrder.Preprocessing);

            return builder;
        }

        public static async Task<int> InvokeAsync(
            this Parser parser,
            ParseResult parseResult,
            IConsole console = null) =>
            await new InvocationPipeline(parser, parseResult)
                .InvokeAsync(console);

        public static async Task<int> InvokeAsync(
            this Parser parser,
            string commandLine,
            IConsole console = null) =>
            await new InvocationPipeline(parser, parser.Parse(commandLine))
                .InvokeAsync(console);

        public static async Task<int> InvokeAsync(
            this Parser parser,
            string[] args,
            IConsole console = null) =>
            await new InvocationPipeline(parser, parser.Parse(args))
                .InvokeAsync(console);

        public static CommandLineBuilder UseHelp(this CommandLineBuilder builder)
        {
            builder.AddMiddleware(async (context, next) => {
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
            builder.AddMiddleware(async (context, next) => {
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
            builder.AddMiddleware(async (context, next) => {
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
                            args: $"register --command-path \"{currentProcessFullPath}\" --suggestion-command \"{currentProcessFileNameWithoutExtension} [suggest]\"",
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

        public static TBuilder OnExecute<TBuilder>(
            this TBuilder builder,
            MethodInfo method,
            object target = null)
            where TBuilder : CommandBuilder
        {
            var methodBinder = new MethodBindingCommandHandler(method, target);
            builder.Handler = methodBinder;
            return builder;
        }

        public static TBuilder OnExecute<TBuilder>(
            this TBuilder builder,
            Action action)
            where TBuilder : CommandBuilder
        {
            var methodBinder = new MethodBindingCommandHandler(action);
            builder.Handler = methodBinder;
            return builder;
        }

        public static CommandBuilder OnExecute<T>(
            this CommandBuilder builder,
            Action<T> action)
        {
            var methodBinder = new MethodBindingCommandHandler(action);
            builder.Handler = methodBinder;
            return builder;
        }

        public static CommandBuilder OnExecute<T1, T2>(
            this CommandBuilder builder,
            Action<T1, T2> action)
        {
            var methodBinder = new MethodBindingCommandHandler(action);
            builder.Handler = methodBinder;
            return builder;
        }

        public static CommandBuilder OnExecute<T1, T2, T3>(
            this CommandBuilder builder,
            Action<T1, T2, T3> action)
        {
            var methodBinder = new MethodBindingCommandHandler(action);
            builder.Handler = methodBinder;
            return builder;
        }

        public static CommandBuilder OnExecute<T1, T2, T3, T4>(
            this CommandBuilder builder,
            Action<T1, T2, T3, T4> action)
        {
            var methodBinder = new MethodBindingCommandHandler(action);
            builder.Handler = methodBinder;
            return builder;
        }

        public static CommandBuilder OnExecute<T1, T2, T3, T4, T5>(
            this CommandBuilder builder,
            Action<T1, T2, T3, T4, T5> action)
        {
            var methodBinder = new MethodBindingCommandHandler(action);
            builder.Handler = methodBinder;
            return builder;
        }

        public static CommandBuilder OnExecute<T1, T2, T3, T4, T5, T6>(
            this CommandBuilder builder,
            Action<T1, T2, T3, T4, T5, T6> action)
        {
            var methodBinder = new MethodBindingCommandHandler(action);
            builder.Handler = methodBinder;
            return builder;
        }

        public static CommandBuilder OnExecute<T1, T2, T3, T4, T5, T6, T7>(
            this CommandBuilder builder,
            Action<T1, T2, T3, T4, T5, T6, T7> action)
        {
            var methodBinder = new MethodBindingCommandHandler(action);
            builder.Handler = methodBinder;
            return builder;
        }

        public static CommandLineBuilder OnExecute<T>(
            this CommandLineBuilder builder,
            Action<T> action)
        {
            var methodBinder = new MethodBindingCommandHandler(action);
            builder.Handler = methodBinder;
            return builder;
        }

        public static CommandLineBuilder OnExecute<T1, T2>(
            this CommandLineBuilder builder,
            Action<T1, T2> action)
        {
            var methodBinder = new MethodBindingCommandHandler(action);
            builder.Handler = methodBinder;
            return builder;
        }

        public static CommandLineBuilder OnExecute<T1, T2, T3>(
            this CommandLineBuilder builder,
            Action<T1, T2, T3> action)
        {
            var methodBinder = new MethodBindingCommandHandler(action);
            builder.Handler = methodBinder;
            return builder;
        }

        public static CommandLineBuilder OnExecute<T1, T2, T3, T4>(
            this CommandLineBuilder builder,
            Action<T1, T2, T3, T4> action)
        {
            var methodBinder = new MethodBindingCommandHandler(action);
            builder.Handler = methodBinder;
            return builder;
        }

        public static CommandLineBuilder OnExecute<T1, T2, T3, T4, T5>(
            this CommandLineBuilder builder,
            Action<T1, T2, T3, T4, T5> action)
        {
            var methodBinder = new MethodBindingCommandHandler(action);
            builder.Handler = methodBinder;
            return builder;
        }

        public static CommandLineBuilder OnExecute<T1, T2, T3, T4, T5, T6>(
            this CommandLineBuilder builder,
            Action<T1, T2, T3, T4, T5, T6> action)
        {
            var methodBinder = new MethodBindingCommandHandler(action);
            builder.Handler = methodBinder;
            return builder;
        }

        public static CommandLineBuilder OnExecute<T1, T2, T3, T4, T5, T6, T7>(
            this CommandLineBuilder builder,
            Action<T1, T2, T3, T4, T5, T6, T7> action)
        {
            var methodBinder = new MethodBindingCommandHandler(action);
            builder.Handler = methodBinder;
            return builder;
        }

        private static bool ShowHelp(
            InvocationContext context,
            IReadOnlyCollection<string> helpOptionAliases)
        {
            var lastToken = context.ParseResult.Tokens.LastOrDefault();

            if (helpOptionAliases.Contains(lastToken) &&
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
