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
    public class VersionPipelineComponent : PipelineComponent
    {
        private VersionOption? versionOption;

        /// <summary>
        /// Sets the aliases that are available for help, replacing the default aliases.
        /// </summary>      
        public string[]? Aliases { get; init; }

        /// <summary>
        /// Constructor to provide default values
        /// </summary>
        public VersionPipelineComponent()
        {
            MiddlewareOrder = (int)MiddlewareOrderInternal.VersionOption; // HACK: to allow compiling
        }


        /// <inheritdoc/>
        public override void Initialize(CommandLineBuilder builder)
        {
            // @jonsequitor This approach to reentrancy does not support new components. Can the builder decide not to rerun, rather than components checking?
            if (builder.VersionOption is null)
            {
                versionOption = Aliases == null
                        ? new VersionOption(builder)
                        : new VersionOption(builder);
                builder.VersionOption = versionOption;
                builder.Command.AddOption(versionOption);
            }
        }

        /// <inheritdoc/>
        public override InvocationContext RunIfNeeded(InvocationContext context)
        {
            // @jonsequitor We could avoid a closure if we allowed lookup on types when they are not a plain option/arg. Lookup on VersionOption.
            if (versionOption is not null && context.ParseResult.FindResultFor(versionOption) is { })
            {
                if (context.ParseResult.Errors.Any(e => e.SymbolResult?.Symbol is VersionOption))
                {
                    context.InvocationResult = new ParseErrorResult(null);
                }
                else
                {
                    context.Console.Out.WriteLine(_assemblyVersion.Value);
                }
                context.TerminationRequested = true;
            }
            return context;
        }


        private static readonly Lazy<string> _assemblyVersion =
            new(() =>
            {
                var assembly = RootCommand.GetAssembly();

                var assemblyVersionAttribute = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();

                if (assemblyVersionAttribute is null)
                {
                    return assembly.GetName().Version?.ToString() ?? "";
                }
                else
                {
                    return assemblyVersionAttribute.InformationalVersion;
                }

            });
    }
}

