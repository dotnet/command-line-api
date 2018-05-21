using System.Collections.Generic;
using System.CommandLine.Builder;
using System.Linq;

namespace System.CommandLine.Invocation
{
    public delegate void InvocationDelegate(InvocationContext context);

    public static class InvocationExtensions
    {
        public static ParserBuilder AddInvocation(
            this ParserBuilder builder,
            InvocationDelegate action)
        {
            builder.AddInvocation(action);

            return builder;
        }

        public static void Invoke(
            this ParseResult parseResult,
            IConsole console)
        {
            if (parseResult.Configuration.InvocationList is IEnumerable<InvocationDelegate> invocations)
            {
                var context = new InvocationContext(parseResult, console) ;

                foreach (var invocation in invocations)
                {
                    invocation(context);
                    if (context.InvocationResult != null)
                    {
                       context.InvocationResult.Apply(context);
                       return;
                    }
                }
            }

            parseResult.CommandDefinition().ExecutionHandler?.Invoke(parseResult);
        }

        public static ParserBuilder AddHelp(this ParserBuilder builder)
        {
            builder.AddInvocation(context => {
                var helpOptions = new HashSet<string>();
                var prefixes = context.ParseResult.Configuration.Prefixes;
                if (prefixes == null)
                {
                    helpOptions.Add("-h");
                    helpOptions.Add("--help");
                    helpOptions.Add("-?");
                    helpOptions.Add("/?");
                }
                else
                {
                    string[] helpOptionNames = { "help", "h", "?" };
                    foreach (var helpOption in helpOptionNames)
                    {
                        foreach (var prefix in prefixes)
                        {
                            helpOptions.Add($"{prefix}{helpOption}");
                        }
                    }
                }

                ShowHelp(context, helpOptions);
            });
            return builder;
        }

        public static ParserBuilder AddHelp(
            this ParserBuilder builder,
            IReadOnlyCollection<string> helpOptionNames)
        {
            builder.AddInvocation(context => {
                ShowHelp(context, helpOptionNames);
            });
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

        private static void ShowHelp(
            InvocationContext context,
            IReadOnlyCollection<string> helpOptionAliases)
        {
            if (helpOptionAliases.Contains(context.ParseResult.UnmatchedTokens.LastOrDefault()))
            {
                context.InvocationResult = new HelpResult();
            }
        }
    }
}
