// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Builder;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace System.CommandLine.Invocation
{
    public static class InvocationExtensions
    {
        public static CommandLineBuilder UseMiddleware(
            this CommandLineBuilder builder,
            Action<InvocationContext> onInvoke)
        {
            builder.AddMiddleware(async (context, next) => {
                onInvoke(context);
                await next(context);
            }, CommandLineBuilder.MiddlewareOrder.Middle);

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
                if (context.ParseResult.Tokens.FirstOrDefault() == "[parse]")
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
                if (context.ParseResult.Tokens.FirstOrDefault() == "[suggest]")
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

        public static CommandLineBuilder UseAutoRegisterSuggest(
            this CommandLineBuilder builder)
        {
            builder.AddMiddleware(async (context, next) => {
                var sentinelFile = Path.Combine(Path.GetTempPath(), "system.commandline-sentinel-files", Assembly.GetEntryAssembly().FullName);
                Process process = Process.GetCurrentProcess();
                var processPath = process.MainModule.FileName;
                Directory.CreateDirectory(Path.GetDirectoryName(sentinelFile));
                if (!File.Exists(sentinelFile))
                {
                    var processInfo = RegistrationProcessInfoMaker.GetProcessStartInfoForRegistration(processPath);
                    Process.Start(processInfo).WaitForExit();
                    File.Create(sentinelFile);
                }

                await next(context);
            }, CommandLineBuilder.MiddlewareOrder.Preprocessing);
            return builder;
        }

        public static string ToETag(this string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }
            var inputBytes = Encoding.ASCII.GetBytes(value);
            byte[] hash;
            using (var md5 = new MD5CryptoServiceProvider())
            {
                hash = md5.ComputeHash(inputBytes);
            }
            return System.Convert.ToBase64String(hash);
        }

        public static TBuilder OnExecute<TBuilder>(
            this TBuilder builder,
            MethodInfo method,
            object target = null)
            where TBuilder : CommandBuilder
        {
            var methodBinder = new MethodBinder(method, target);
            builder.ExecutionHandler = methodBinder;
            return builder;
        }
   
        public static TBuilder OnExecute<TBuilder>(
            this TBuilder builder,
            Action action)
            where TBuilder : CommandBuilder
        {
            var methodBinder = new MethodBinder(action);
            builder.ExecutionHandler = methodBinder;
            return builder;
        }

        public static CommandBuilder OnExecute<T>(
            this CommandBuilder builder,
            Action<T> action)
        {
            var methodBinder = new MethodBinder(action);
            builder.ExecutionHandler = methodBinder;
            return builder;
        }

        public static CommandBuilder OnExecute<T1, T2>(
            this CommandBuilder builder,
            Action<T1, T2> action)
        {
            var methodBinder = new MethodBinder(action);
            builder.ExecutionHandler = methodBinder;
            return builder;
        }

        public static CommandBuilder OnExecute<T1, T2, T3>(
            this CommandBuilder builder,
            Action<T1, T2, T3> action)
        {
            var methodBinder = new MethodBinder(action);
            builder.ExecutionHandler = methodBinder;
            return builder;
        }

        public static CommandBuilder OnExecute<T1, T2, T3, T4>(
            this CommandBuilder builder,
            Action<T1, T2, T3, T4> action)
        {
            var methodBinder = new MethodBinder(action);
            builder.ExecutionHandler = methodBinder;
            return builder;
        }

        public static CommandBuilder OnExecute<T1, T2, T3, T4, T5>(
            this CommandBuilder builder,
            Action<T1, T2, T3, T4, T5> action)
        {
            var methodBinder = new MethodBinder(action);
            builder.ExecutionHandler = methodBinder;
            return builder;
        }

        public static CommandBuilder OnExecute<T1, T2, T3, T4, T5, T6>(
            this CommandBuilder builder,
            Action<T1, T2, T3, T4, T5, T6> action)
        {
            var methodBinder = new MethodBinder(action);
            builder.ExecutionHandler = methodBinder;
            return builder;
        }

        public static CommandBuilder OnExecute<T1, T2, T3, T4, T5, T6, T7>(
            this CommandBuilder builder,
            Action<T1, T2, T3, T4, T5, T6, T7> action)
        {
            var methodBinder = new MethodBinder(action);
            builder.ExecutionHandler = methodBinder;
            return builder;
        }

        public static CommandLineBuilder OnExecute<T>(
            this CommandLineBuilder builder,
            Action<T> action)
        {
            var methodBinder = new MethodBinder(action);
            builder.ExecutionHandler = methodBinder;
            return builder;
        }

        public static CommandLineBuilder OnExecute<T1, T2>(
            this CommandLineBuilder builder,
            Action<T1, T2> action)
        {
            var methodBinder = new MethodBinder(action);
            builder.ExecutionHandler = methodBinder;
            return builder;
        }

        public static CommandLineBuilder OnExecute<T1, T2, T3>(
            this CommandLineBuilder builder,
            Action<T1, T2, T3> action)
        {
            var methodBinder = new MethodBinder(action);
            builder.ExecutionHandler = methodBinder;
            return builder;
        }

        public static CommandLineBuilder OnExecute<T1, T2, T3, T4>(
            this CommandLineBuilder builder,
            Action<T1, T2, T3, T4> action)
        {
            var methodBinder = new MethodBinder(action);
            builder.ExecutionHandler = methodBinder;
            return builder;
        }

        public static CommandLineBuilder OnExecute<T1, T2, T3, T4, T5>(
            this CommandLineBuilder builder,
            Action<T1, T2, T3, T4, T5> action)
        {
            var methodBinder = new MethodBinder(action);
            builder.ExecutionHandler = methodBinder;
            return builder;
        }

        public static CommandLineBuilder OnExecute<T1, T2, T3, T4, T5, T6>(
            this CommandLineBuilder builder,
            Action<T1, T2, T3, T4, T5, T6> action)
        {
            var methodBinder = new MethodBinder(action);
            builder.ExecutionHandler = methodBinder;
            return builder;
        }

        public static CommandLineBuilder OnExecute<T1, T2, T3, T4, T5, T6, T7>(
            this CommandLineBuilder builder,
            Action<T1, T2, T3, T4, T5, T6, T7> action)
        {
            var methodBinder = new MethodBinder(action);
            builder.ExecutionHandler = methodBinder;
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
                       .FlattenBreadthFirst(s => s.Symbols)
                       .SelectMany(s => s.RawAliases)
                       .Any(helpOptionAliases.Contains);
        }
    }
}
