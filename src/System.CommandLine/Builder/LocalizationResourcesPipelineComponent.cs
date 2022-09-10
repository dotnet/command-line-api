using System.CommandLine.Binding;
using System.CommandLine.Help;
using System.CommandLine.Invocation;

namespace System.CommandLine.Builder
{
    /// <summary>
    /// A sample pipeline component
    /// </summary>
    public class LocalizationResourcesPipelineComponent : PipelineComponent
    {

        /// <summary>
        /// Sets the resources used to localize validation error messages.
        /// </summary>      
        public LocalizationResources? ValidationMessages  { get; init; }


        /// <summary>
        /// Constructor to provide default values
        /// </summary>
        public LocalizationResourcesPipelineComponent()
        {
            MiddlewareOrder = 0; // Not used
        }

        /// <inheritdoc/>
        /// <inheritdoc/>
        public override bool ShouldRun(InvocationContext context)
            => false;

        public override void Initialize(CommandLineBuilder builder)
        {
            if (ValidationMessages is not null)
            { builder.LocalizationResources = ValidationMessages; }
        }

        /// <inheritdoc/>
        public override InvocationContext RunIfNeeded(InvocationContext context)
            => context;

    }
}

