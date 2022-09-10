using System.CommandLine.Help;
using System.CommandLine.Invocation;
using System.Linq;

namespace System.CommandLine.Builder
{
    /// <summary>
    /// A pipeline component to support a directive that diagrams input
    /// </summary>
    public class SuggestPipelineComponent : PipelineComponent
    {
        private const string directiveName = "suggest";

        /// <summary>
        /// Constructor to provide default values
        /// </summary>
        public SuggestPipelineComponent()
        {
            MiddlewareOrder = (int)MiddlewareOrderInternal.SuggestDirective; // HACK: to allow compiling
        }

        /// <inheritdoc/>
        public override void Initialize(CommandLineBuilder builder)
        { }

        /// <inheritdoc/>
        public override bool ShouldRun(InvocationContext context)
            => context.ParseResult.Directives.Contains(directiveName);

        /// <inheritdoc/>
        public override InvocationContext RunIfNeeded(InvocationContext context)
        {
            if (context.ParseResult.Directives.TryGetValues(directiveName, out var values))
            {
                int position = values.FirstOrDefault() is { } positionString 
                    ? int.Parse(positionString) 
                    : context.ParseResult.CommandLineText?.Length ?? 0;
                context.InvocationResult = new SuggestDirectiveResult(position);

                context.TerminationRequested = true;
            }
            return context;
        }

    }
}

