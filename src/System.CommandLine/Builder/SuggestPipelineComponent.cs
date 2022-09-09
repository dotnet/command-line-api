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
        public override InvocationContext RunIfNeeded(InvocationContext context)
        {
            if (context.ParseResult.Directives.TryGetValues("suggest", out var values))
            {
                int position;

                if (values.FirstOrDefault() is { } positionString)
                {
                    position = int.Parse(positionString);
                }
                else
                {
                    position = context.ParseResult.CommandLineText?.Length ?? 0;
                }

                context.InvocationResult = new SuggestDirectiveResult(position);

                context.TerminationRequested = true;
            }
            return context;
        }

    }
}

