using System.CommandLine.Help;
using System.CommandLine.Invocation;

namespace System.CommandLine.Builder
{
    /// <summary>
    /// A pipeline component to support a directive that diagrams input
    /// </summary>
    public class ParseDirectivePipelineComponent : PipelineComponent
    {

        /// <summary>
        /// Sets the aliases that are available for help, replacing the default aliases.
        /// </summary>      
        public int? ErrorExitCode { get; init; }

        /// <summary>
        /// Constructor to provide default values
        /// </summary>
        public ParseDirectivePipelineComponent()
        {
            MiddlewareOrder = (int)MiddlewareOrderInternal.ParseDirective; // HACK: to allow compiling
        }

        /// <inheritdoc/>
        public override void Initialize(CommandLineBuilder builder)
        { }

        /// <inheritdoc/>
        public override bool ShouldRun(InvocationContext context)
            => context.ParseResult.Directives.Contains("parse");

        /// <inheritdoc/>
        public override InvocationContext RunIfNeeded(InvocationContext context)
        {
            context.InvocationResult = new ParseDirectiveResult(ErrorExitCode);
            context.TerminationRequested = true;
            return context;
        }

    }
}

