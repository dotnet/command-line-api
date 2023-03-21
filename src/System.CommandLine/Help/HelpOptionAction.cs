using System.CommandLine.Invocation;
using System.CommandLine.IO;
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

        public override int Invoke(InvocationContext context)
        {
            var output = context.Console.Out.CreateTextWriter();

            var helpContext = new HelpContext(Builder,
                                              context.ParseResult.CommandResult.Command,
                                              output,
                                              context.ParseResult);

            Builder.Write(helpContext);

            return 0;
        }

        public override Task<int> InvokeAsync(InvocationContext context, CancellationToken cancellationToken = default)
            => cancellationToken.IsCancellationRequested
                ? Task.FromCanceled<int>(cancellationToken)
                : Task.FromResult(Invoke(context));
    }
}
