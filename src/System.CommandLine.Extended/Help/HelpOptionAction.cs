using System.CommandLine.Invocation;

namespace System.CommandLine.Help
{
    /// <summary>
    /// Provides command line help.
    /// </summary>
    public sealed class HelpAction : SynchronousCliAction
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

        /// <inheritdoc />
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
    }
}