using System.Threading;
using System.Threading.Tasks;

namespace System.CommandLine.Help
{
    public sealed class HelpAction : CliAction
    {
        private HelpBuilder? _builder;

        /// <summary>
        /// Specifies an <see cref="Builder"/> to be used to format help output when help is requested.
        /// </summary>
        public HelpBuilder Builder
        {
            get => _builder ??= new HelpBuilder(Console.IsOutputRedirected ? int.MaxValue : Console.WindowWidth);
            set => _builder = value ?? throw new ArgumentNullException(nameof(value));
        }

        public override int Invoke(ParseResult parseResult)
        {
            var output = parseResult.Configuration.Output;

            var helpContext = new HelpContext(Builder,
                                              parseResult.CommandResult.Command,
                                              output,
                                              parseResult);

            Builder.Write(helpContext);

            return 0;
        }

        public override Task<int> InvokeAsync(ParseResult parseResult, CancellationToken cancellationToken = default)
            => cancellationToken.IsCancellationRequested
                ? Task.FromCanceled<int>(cancellationToken)
                : Task.FromResult(Invoke(parseResult));
    }
}
