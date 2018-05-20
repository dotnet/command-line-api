using System.Collections.Generic;
using System.CommandLine.Builder;
using System.IO;
using System.Linq;

namespace System.CommandLine
{
    public delegate void Invocation(InvocationContext context);

    public class MethodBinder
    {
        private readonly Delegate _Delegate;
        private readonly string[] _OptionAliases;

        public MethodBinder(Delegate @delegate, params string[] optionAliases)
        {
            _Delegate = @delegate ?? throw new ArgumentNullException(nameof(@delegate));
            _OptionAliases = optionAliases;
        }

        public void Invoke(ParseResult result)
        {
            var arguments = new List<object>();
            var parameters = _Delegate.Method.GetParameters();
            for (var index = 0; index < parameters.Length; index++)
            {
                var argument = result.Command().ValueForOption(_OptionAliases[index]);
                arguments.Add(argument);
            }

            _Delegate.DynamicInvoke(arguments.ToArray());
        }
    }

    public class InvocationContext
    {
        public InvocationContext(ParseResult parseResult)
        {
            ParseResult = parseResult;
        }

        public ParseResult ParseResult { get; }

        public IInvocationResult InvocationResult { get; set; }

        public TextWriter Output { get; set; } = Console.Out;
    }

    public interface IInvocationResult
    {
    }

    public static class InvocationExtensions
    {
        public static ParserBuilder AddInvocation(
            this ParserBuilder builder,
            Invocation action)
        {
            builder.AddInvocation(action);

            return builder;
        }

        public static void Invoke(this ParseResult parseResult, TextWriter output = null)
        {
            if (parseResult.Configuration.InvocationList is List<Invocation> invocations)
            {
                var context = new InvocationContext(parseResult) {
                    Output = output
                };

                foreach (var invocation in invocations)
                {
                    invocation(context);
                    if (context.InvocationResult != null)
                    {
                        break;
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
                HelpInvocation(context, helpOptions);
            });
            return builder;
        }

        public static ParserBuilder AddHelp(this ParserBuilder builder, IReadOnlyCollection<string> helpOptionNames)
        {
            builder.AddInvocation(context => {
                HelpInvocation(context, helpOptionNames);
            });
            return builder;
        }

        private static void HelpInvocation(InvocationContext context, IReadOnlyCollection<string> helpOptionNames)
        {
            if (helpOptionNames.Contains(context.ParseResult.UnmatchedTokens.LastOrDefault()))
            {
                string helpView = context.ParseResult.Command().Definition.HelpView();
                context.Output.Write(helpView);
            }
        }
    }
}
