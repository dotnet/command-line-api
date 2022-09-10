using System;
using System.Collections.Generic;
using System.CommandLine.Help;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace System.CommandLine.Builder
{
    /// <summary>
    /// An invocation pipeline middleware commponent. This allows access to middleware components 
    /// outside the middleware pipeline, without altering the pipeline itself.
    /// </summary>
    public abstract class PipelineComponent
    {
        /// <summary>
        /// Allows components to add options and arguments to the Command
        /// </summary>
        /// <param name="builder">The current CommandLineBuilder, which offers access to the parser and root cmmand</param>
        public abstract void Initialize(CommandLineBuilder builder);

        /// <summary>
        /// Indicates whether the component should run based on the ParseResult and other InvocationContext details.
        /// </summary>
        /// <param name="context"></param>
        /// <returns>Returns `true` if the component should run, otherwise, false.</returns>
        public abstract bool ShouldRun(InvocationContext context);

        /// <summary>
        /// When overridden, this method should test whether the component should run, run it if needed, and 
        /// set the InvocationContext parameters TerminationRequested to true if  this should terminate the pipeline.
        /// </summary>
        /// <param name="context">The current invocation context, offering access to key data like the ParseResult.</param>
        /// <returns>The same InvocationContext that was received as a parameter.</returns>
        public abstract InvocationContext RunIfNeeded(InvocationContext context);

        // @jonsequitor How does the internal middleware order work with new components?
        /// <summary>
        /// The order in which middleware will be initialized and executed. This is an int as a hack because I cannot figure out how to make the Middleware enums work.
        /// </summary>
        public int MiddlewareOrder { get; protected set; } = 0;


    }
}

