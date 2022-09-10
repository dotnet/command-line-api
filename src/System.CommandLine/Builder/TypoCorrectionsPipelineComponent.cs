using System.CommandLine.Help;
using System.CommandLine.Invocation;
using System.Linq;

namespace System.CommandLine.Builder
{
    /// <summary>
    /// A pipeline component to support a directive that diagrams input
    /// </summary>
    public class TypoCorrectionsPipelineComponent : PipelineComponent
    {
        /// <summary>
        /// Sets the LevenshteinDistance which determines how close matches should be.
        /// </summary>      
        public int MaxLevenshteinDistance { get; init; } = 3;


        /// <summary>
        /// Constructor to provide default values
        /// </summary>
        public TypoCorrectionsPipelineComponent()
        {
            MiddlewareOrder = (int)MiddlewareOrderInternal.TypoCorrection; // HACK: to allow compiling
        }

        /// <inheritdoc/>
        public override void Initialize(CommandLineBuilder builder)
        { }

        /// <inheritdoc/>
        public override bool ShouldRun(InvocationContext context)
            => context.ParseResult.CommandResult.Command.TreatUnmatchedTokensAsErrors &&
                        context.ParseResult.UnmatchedTokens.Count > 0;

        /// <inheritdoc/>
        public override InvocationContext RunIfNeeded(InvocationContext context)
        {
            var typoCorrection = new TypoCorrection(MaxLevenshteinDistance);
            typoCorrection.ProvideSuggestions(context.ParseResult, context.Console);
            return context;
        }

    }
}

