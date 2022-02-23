// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

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
    /// use any subcommands. See <see cref="Command"/> for applications with multiple actions.
    /// </remarks>
    public class RootCommand : Command
    {
        private static Assembly? _assembly;
        private static string? _executablePath;
        private static string? _executableName;

        /// <param name="description">The description of the command, shown in help.</param>
        public RootCommand(string description = "") : base(ExecutableName, description)
        {
        }

        internal static Assembly GetAssembly()
            => _assembly ??= (Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly());

        /// <summary>
        /// The name of the currently running executable.
        /// </summary>
        public static string ExecutableName
            => _executableName ??= Path.GetFileNameWithoutExtension(ExecutablePath).Replace(" ", "");

        /// <summary>
        /// The path to the currently running executable.
        /// </summary>
        public static string ExecutablePath => _executablePath ??= Environment.GetCommandLineArgs()[0];

        private protected override void RemoveAlias(string alias)
        {
            if (!string.Equals(alias, ExecutableName, StringComparison.Ordinal))
            {
                base.RemoveAlias(alias);
            }
        }
    }
}
