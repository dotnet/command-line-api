// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.Linq;
using System.Reflection;

namespace System.CommandLine
{
    /// <summary>
    /// Represents a standard option that indicates that version information should be displayed for the app.
    /// </summary>
    public sealed class VersionOption : Option
    {
        private CommandLineAction? _action;

        /// <summary>
        /// Initializes a new instance of <see cref="VersionOption" />.
        /// </summary>
        /// <remarks>
        /// When added to a <see cref="Command"/>, it enables the use of a <c>--version</c> option, which, when specified in command line input, short circuits normal command handling and instead writes out version information before exiting.
        /// </remarks>
        public VersionOption() : this("--version")
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="VersionOption" />.
        /// </summary>
        /// <remarks>
        /// When added to a <see cref="Command"/>, it enables the use of a provided option name and aliases, which, when specified in command line input, short circuits normal command handling and instead writes out version information before exiting.
        /// </remarks>
        public VersionOption(string name, params string[] aliases)
            : base(name, aliases)
        {
            Description = LocalizationResources.VersionOptionDescription();
            AddValidators();
            Arity = ArgumentArity.Zero;
        }

        /// <inheritdoc />
        public override CommandLineAction? Action
        {
            get => _action ??= new VersionOptionAction();
            set => _action = value ?? throw new ArgumentNullException(nameof(value));
        }

        private void AddValidators()
        {
            Validators.Add(static result =>
            {
                if (result.Parent is CommandResult parent &&
                    parent.Children.Any(r =>
                                            r is not OptionResult { Option: VersionOption } &&
                                            r is not OptionResult { Implicit: true }))
                {
                    result.AddError(LocalizationResources.VersionOptionCannotBeCombinedWithOtherArguments(result.IdentifierToken?.Value ?? result.Option.Name));
                }
            });
        }

        internal override bool Greedy => false;

        internal override Argument Argument => Argument.None;

        /// <inheritdoc />
        public override Type ValueType => typeof(void);

        private sealed class VersionOptionAction : SynchronousCommandLineAction
        {
            public override int Invoke(ParseResult parseResult)
            {
                parseResult.InvocationConfiguration.Output.WriteLine(GetExecutableVersion());
                return 0;
            }

            public override bool ClearsParseErrors => true;

            private static string GetExecutableVersion()
            {
                var assembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();

                var assemblyVersionAttribute = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();

                if (assemblyVersionAttribute is null)
                {
                    return assembly.GetName().Version?.ToString() ?? "";
                }
                else
                {
                    return assemblyVersionAttribute.InformationalVersion;
                }
            }
        }
    }
}
