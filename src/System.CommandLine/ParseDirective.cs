using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.CommandLine.Parsing;

namespace System.CommandLine
{
    public sealed class ParseDirective : Directive
    {
        public ParseDirective() : base("parse", syncHandler: SyncHandler)
        {
        }

        private static void SyncHandler(InvocationContext context)
        {
            var parseResult = context.ParseResult;
            context.Console.Out.WriteLine(parseResult.Diagram());
            context.ExitCode = parseResult.Errors.Count == 0
                                     ? 0
                                     : context.Parser.Configuration.ParseDirectiveExitCode!.Value;
        }
    }
}
