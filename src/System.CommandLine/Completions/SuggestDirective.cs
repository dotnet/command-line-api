using System.Linq;
using System.CommandLine.Parsing;
using System.Threading.Tasks;
using System.Threading;

namespace System.CommandLine.Completions
{
    /// <summary>
    /// Enables the use of the <c>[suggest]</c> directive which when specified in command line input short circuits normal command handling and writes a newline-delimited list of suggestions suitable for use by most shells to provide command line completions.
    /// </summary>
    /// <remarks>The <c>dotnet-suggest</c> tool requires the suggest directive to be enabled for an application to provide completions.</remarks>
    public sealed class SuggestDirective : CliDirective
    {
        private CliAction? _action;

        public SuggestDirective() : base("suggest")
        {
        }

        /// <inheritdoc />
        public override CliAction? Action
        {
            get => _action ??= new SuggestDirectiveAction(this);
            set => _action = value ?? throw new ArgumentNullException(nameof(value));
        }

        private sealed class SuggestDirectiveAction : CliAction
        {
            private readonly SuggestDirective _directive;

            internal SuggestDirectiveAction(SuggestDirective suggestDirective) => _directive = suggestDirective;

            public override int Invoke(ParseResult parseResult)
            {
                string? parsedValues = parseResult.GetResult(_directive)!.Values.SingleOrDefault();
                string? rawInput = parseResult.CommandLineText;

                int position = !string.IsNullOrEmpty(parsedValues) ? int.Parse(parsedValues) : rawInput?.Length ?? 0;

                var commandLineToComplete = parseResult.Tokens.LastOrDefault(t => t.Type != CliTokenType.Directive)?.Value ?? "";

                var completionParseResult = parseResult.RootCommandResult.Command.Parse(commandLineToComplete, parseResult.Configuration);

                var completions = completionParseResult.GetCompletions(position);

                parseResult.Configuration.Output.WriteLine(
                    string.Join(
                        Environment.NewLine,
                        completions));

                return 0;
            }

            public override Task<int> InvokeAsync(ParseResult parseResult, CancellationToken cancellationToken = default)
                => cancellationToken.IsCancellationRequested
                    ? Task.FromCanceled<int>(cancellationToken)
                    : Task.FromResult(Invoke(parseResult));
        }
    }
}
