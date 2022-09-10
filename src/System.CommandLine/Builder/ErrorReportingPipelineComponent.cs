using System.CommandLine.Help;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.Linq;
using System.Reflection;

namespace System.CommandLine.Builder
{
    /// <summary>
    /// A sample pipeline component
    /// </summary>
    public class ErrorReportingPipelineComponent : PipelineComponent
    {

        /// <summary>
        /// Sets the aliases that are available for help, replacing the default aliases.
        /// </summary>      
        public int? ErrorExitCode { get; init; }        /// <summary>
                                                        /// Constructor to provide default values
                                                        /// </summary>
        public ErrorReportingPipelineComponent()
        {
            MiddlewareOrder = (int)MiddlewareOrderInternal.ParseErrorReporting; // HACK: to allow compiling
        }


        /// <inheritdoc/>
        public override void Initialize(CommandLineBuilder builder)
        { }


        /// <inheritdoc/>
        public override bool ShouldRun(InvocationContext context)
            => context.ParseResult.Errors.Count > 0;

        /// <inheritdoc/>
        public override InvocationContext RunIfNeeded(InvocationContext context)
        {

            context.InvocationResult = new ParseErrorResult(ErrorExitCode);
            context.TerminationRequested = true;
            return context;
        }

    }
}

