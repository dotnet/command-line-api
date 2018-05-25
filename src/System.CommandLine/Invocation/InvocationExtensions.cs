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
        public static ParserBuilder AddMiddleware(
            this ParserBuilder builder,
            InvocationMiddleware onInvoke)
        {
            builder.AddMiddleware(onInvoke);
            return builder;
        }

        public static ParserBuilder AddMiddleware(
            this ParserBuilder builder,
            Action<InvocationContext> onInvoke)
        {
            builder.AddMiddleware(async (context, next) => {
                onInvoke(context);
                await next(context);
            });

            return builder;
        }

        public static ParserBuilder HandleAndDisplayExceptions(
            this ParserBuilder builder)
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
            });

            return builder;
        }

        public static ParserBuilder UseParseDirective(
            this ParserBuilder builder)
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
            });

            return builder;
        }

        public static ParserBuilder UseSuggestDirective(
            this ParserBuilder builder)
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
            });

            return builder;
        }

        public static async Task<int> InvokeAsync(
            this ParseResult parseResult,
            IConsole console) =>
            await new InvocationPipeline(parseResult)
                .InvokeAsync(console);

        public static ParserBuilder AddHelp(this ParserBuilder builder)
        {
            builder.AddMiddleware(async (context, next) => {
                var helpOptionTokens = new HashSet<string>();
                var prefixes = context.ParseResult.Parser.Configuration.Prefixes;
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
            });

            return builder;
        }

        public static ParserBuilder AddHelp(
            this ParserBuilder builder,
            IReadOnlyCollection<string> helpOptionTokens)
        {
            builder.AddMiddleware(async (context, next) => {
                if (!ShowHelp(context, helpOptionTokens))
                {
                    await next(context);
                }
            });
            return builder;
        }

        public static ParserBuilder AddParseErrorReporting(
            this ParserBuilder builder)
        {
            builder.AddMiddleware(async (context, next) => {
                if (context.ParseResult.Errors.Count > 0)
                {
                    context.InvocationResult = new ParseErrorResult();
                }

                await next(context);
            });
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
                context.ParseResult
                       .Parser
                       .Configuration
                       .SymbolDefinitions
                       .FlattenBreadthFirst(s => s.SymbolDefinitions)
                       .SelectMany(s => s.RawAliases)
                       .Any(helpOptionAliases.Contains);
        }
    }
}
