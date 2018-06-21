// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Builder;
using System.Linq;
using System.Reflection;
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
                if (context.ParseResult.Tokens.FirstOrDefault() == "!parse")
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
                if (context.ParseResult.Tokens.FirstOrDefault() == "!suggest")
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
            IConsole console) =>
            await new InvocationPipeline(parser, parseResult)
                .InvokeAsync(console);

        public static async Task<int> InvokeAsync(
            this Parser parser,
            string commandLine,
            IConsole console) =>
            await new InvocationPipeline(parser, parser.Parse(commandLine))
                .InvokeAsync(console);

        public static async Task<int> InvokeAsync(
            this Parser parser,
            string[] args,
            IConsole console) =>
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

        public static CommandDefinitionBuilder OnExecute(
            this CommandDefinitionBuilder builder,
            MethodInfo method,
            object target = null)
        {
            var methodBinder = new MethodBinder(method, target);
            builder.ExecutionHandler = methodBinder;
            return builder;
        }
   
        public static CommandDefinitionBuilder OnExecute(
            this CommandDefinitionBuilder builder,
            Action action)
        {
            var methodBinder = new MethodBinder(action);
            builder.ExecutionHandler = methodBinder;
            return builder;
        }

        public static CommandDefinitionBuilder OnExecute<T>(
            this CommandDefinitionBuilder builder,
            Action<T> action)
        {
            var methodBinder = new MethodBinder(action);
            builder.ExecutionHandler = methodBinder;
            return builder;
        }

        public static CommandDefinitionBuilder OnExecute<T1, T2>(
            this CommandDefinitionBuilder builder,
            Action<T1, T2> action)
        {
            var methodBinder = new MethodBinder(action);
            builder.ExecutionHandler = methodBinder;
            return builder;
        }

        public static CommandDefinitionBuilder OnExecute<T1, T2, T3>(
            this CommandDefinitionBuilder builder,
            Action<T1, T2, T3> action)
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
                       .SymbolDefinitions
                       .FlattenBreadthFirst(s => s.SymbolDefinitions)
                       .SelectMany(s => s.RawAliases)
                       .Any(helpOptionAliases.Contains);
        }
    }
}
