using System.CommandLine.Invocation;
using System.CommandLine.Parsing;

namespace System.CommandLine.Builder
{
    /// <summary>
    /// A sample pipeline component
    /// </summary>
    public class TokenReplacerPipelineComponent : PipelineComponent
    {

        /// <summary>
        /// Sets the HelpBuilder
        /// </summary>      
        public TryReplaceToken? TokenReplacer { get; init; }


        /// <summary>
        /// Constructor to provide default values
        /// </summary>
        public TokenReplacerPipelineComponent()
        {
            MiddlewareOrder = 0; // Not used
        }

        /// <inheritdoc/>
        public override void Initialize(CommandLineBuilder builder)
        {
            if (TokenReplacer is not null)
            { builder.TokenReplacer = TokenReplacer; }
        }

        /// <inheritdoc/>
        public override bool ShouldRun(InvocationContext context)
            => false;

        /// <inheritdoc/>
        public override InvocationContext RunIfNeeded(InvocationContext context)
            => context;

    }
}

