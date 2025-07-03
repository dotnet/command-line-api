// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Invocation;

namespace System.CommandLine.Help
{
    /// <summary>
    /// Provides command line help.
    /// </summary>
    public sealed class CustomHelpAction : SynchronousCommandLineAction
    {
        private HelpBuilder? _builder;

        /// <summary>
        /// Specifies an <see cref="Builder"/> to be used to format help output when help is requested.
        /// </summary>
        internal HelpBuilder Builder
        {
            get => _builder ??= new HelpBuilder(Console.IsOutputRedirected ? int.MaxValue : Console.WindowWidth);
            set => _builder = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <inheritdoc />
        public override int Invoke(ParseResult parseResult)
        {
            var output = parseResult.InvocationConfiguration.Output;

            var helpContext = new HelpContext(Builder,
                                              parseResult.CommandResult.Command,
                                              output);

            Builder.Write(helpContext);

            return 0;
        }
    }
}