using System.CommandLine.Invocation;

namespace System.CommandLine.Help
{
    /// <summary>
    /// Provides command line help.
    /// </summary>
    public sealed class HelpAction : SynchronousCommandLineAction
    {
        private HelpBuilder? _builder;
        private int _maxWidth = -1;

        /// <summary>
        /// The maximum width in characters after which help output is wrapped.
        /// </summary>
        /// <remarks>It defaults to <see cref="Console.WindowWidth"/>.</remarks>
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
        /// Specifies an <see cref="Builder"/> to be used to format help output when help is requested.
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