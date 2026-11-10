using System.CommandLine.Invocation;

namespace System.CommandLine.Help
{
    /// <summary>
    /// Provides command-line help.
    /// </summary>
    public sealed class HelpAction : SynchronousCommandLineAction
    {
        private HelpBuilder? _builder;
        private int _maxWidth = -1;

        /// <summary>
        /// Gets or sets the maximum width in characters after which help output is wrapped.
        /// </summary>
        /// <value>The maximum width in characters after which help output is wrapped. The default is <see cref="Console.WindowWidth"/>.</value>
        public int MaxWidth
        {
            get
            {
                if (_maxWidth < 0)
                {
                    try
                    {
                        _maxWidth = Console.IsOutputRedirected ? int.MaxValue : Console.WindowWidth;
                    }
                    catch (Exception)
                    {
                        _maxWidth = int.MaxValue;
                    }
                }

                return _maxWidth;
            }
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                _maxWidth = value;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="Builder"/> to use to format help output.
        /// </summary>
        internal HelpBuilder Builder
        {
            get => _builder ??= new HelpBuilder(MaxWidth);
            set => _builder = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <inheritdoc />
        public override int Invoke(ParseResult parseResult)
        {
            var output = parseResult.InvocationConfiguration.Output;

            var helpContext = new HelpContext(Builder,
                                              parseResult.CommandResult.Command,
                                              output);

            Builder.Write(helpContext);

            return 0;
        }

        public override bool ClearsParseErrors => true;
    }
}
