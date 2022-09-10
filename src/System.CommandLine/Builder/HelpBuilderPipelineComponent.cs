using System.CommandLine.Binding;
using System.CommandLine.Help;
using System.CommandLine.Invocation;

namespace System.CommandLine.Builder
{
    /// <summary>
    /// A sample pipeline component
    /// </summary>
    public class HelpBuilderPipelineComponent : PipelineComponent
    {

        /// <summary>
        /// Sets the HelpBuilder
        /// </summary>      
        public Func<BindingContext, HelpBuilder>? GetHelpBuilder { get; init; }


        /// <summary>
        /// Constructor to provide default values
        /// </summary>
        public HelpBuilderPipelineComponent()
        {
            MiddlewareOrder = 0; // Not used
        }

        /// <inheritdoc/>
        public override void Initialize(CommandLineBuilder builder)
        {
            if (GetHelpBuilder is not null)
            { builder.UseHelpBuilderFactory(GetHelpBuilder); }
        }

        /// <inheritdoc/>
        public override bool ShouldRun(InvocationContext context)
            => false;

        /// <inheritdoc/>
        public override InvocationContext RunIfNeeded(InvocationContext context)
            => context;

    }
}

