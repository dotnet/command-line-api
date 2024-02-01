// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace System.CommandLine
{
    /// <summary>
    /// Represents the main action that the application performs.
    /// </summary>
    /// <remarks>
    /// Use the RootCommand object without any subcommands for applications that perform one action. Add subcommands 
    /// to the root for applications that require actions identified by specific strings. For example, `dir` does not 
    /// use any subcommands. See <see cref="CliCommand"/> for applications with multiple actions.
    /// </remarks>
    public class CliRootCommand : CliCommand
    {
        /// <param name="description">The description of the command, shown in help.</param>
        public CliRootCommand(/*string description = "" */)
            : base(CliExecutable.ExecutableName/*, description*/)
        {
            /*
            Options.Add(new HelpOption());
            Options.Add(new VersionOption());
            Directives = new ChildSymbolList<CliDirective>(this)
            {
                new SuggestDirective()
            };
            */
        }

// TODO: directives
/*
        /// <summary>
        /// Represents all of the directives that are valid under the root command.
        /// </summary>
        public IList<CliDirective> Directives { get; }

        /// <summary>
        /// Adds a <see cref="CliDirective"/> to the command.
        /// </summary>
        public void Add(CliDirective directive) => Directives.Add(directive);
*/
    }
}
