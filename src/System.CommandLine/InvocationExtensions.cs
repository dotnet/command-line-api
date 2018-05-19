using System.Collections.Generic;
using System.CommandLine.Builder;
using System.IO;
using System.Linq;

namespace System.CommandLine
{
    public delegate void Invocation(InvocationContext context);

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
        }

        public static ParserBuilder AddHelp(this ParserBuilder builder)
        {
            string[] helpTokens = new[] { "--help" };
            builder.AddInvocation(context => {
                if (helpTokens.Contains(context.ParseResult.Tokens.LastOrDefault()))
                {
                    string helpView = context.ParseResult.Command().Definition.HelpView();
                    context.Output.Write(helpView);
                }
            });
            return builder;
        }
    }
}
