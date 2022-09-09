using System.CommandLine.Help;
using System.CommandLine.Invocation;

namespace System.CommandLine.Builder
{
    /// <summary>
    /// Pipeline component that supplies help, using other Help things like HelpBuilderFactory
    /// </summary>
    public class HelpPipelineComponent : PipelineComponent
    {
        private HelpOption? helpOption;

        /// <summary>
        /// Sets the aliases that are available for help, replacing the default aliases.
        /// </summary>
        public string[]? Aliases { get; init; }

        /// <summary>
        /// Set ths masimum help width
        /// </summary>
        public int? MaxHelpWidth { get; init; }

        /// <summary>
        /// Allows setting of the help values in the builder object 
        /// </summary>
        public Action<HelpContext>? Customize { get; init; }

        /// <summary>
        /// Constructor to provide default values
        /// </summary>
        public HelpPipelineComponent()
        {
            MiddlewareOrder = (int)MiddlewareOrderInternal.HelpOption; // HACK: to allow compiling
        }



        /// <inheritdoc/>
        public override void Initialize(CommandLineBuilder builder)
        {
            // @jonsequitor This approach to reentrancy does not support new components. Can the builder decide not to rerun, rather than components checking?
            if (helpOption is null)
            {
                helpOption = Aliases == null
                        ? new HelpOption(() => builder.LocalizationResources)
                        : new HelpOption(Aliases, () => builder.LocalizationResources);
                builder.MaxHelpWidth = MaxHelpWidth;
                builder.HelpOption = helpOption;
                builder.Command.AddGlobalOption(helpOption);
                if (Customize is not null)
                { builder.CustomizeHelpLayout(Customize); }
            }
        }

        /// <inheritdoc/>
        public override InvocationContext RunIfNeeded(InvocationContext context)
        {
            // @jonsequitor We could avoid a closure if we allowed lookup on types when they are not a plain option/arg. Lookup on VersionOption.
            if (helpOption is not null && context.ParseResult.FindResultFor(helpOption) is { })
            {
                if (context.ParseResult.FindResultFor(helpOption) is { })
                {
                    context.InvocationResult = new HelpResult();
                    context.TerminationRequested = true;
                }
            }
            return context;
        }
    }
}

