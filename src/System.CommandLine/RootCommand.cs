// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.IO;
using System.Reflection;
using System.Threading;

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
        private static readonly Lazy<Assembly> _assembly =
            new(() => Assembly.GetEntryAssembly() ??
                      Assembly.GetExecutingAssembly(),
                LazyThreadSafetyMode.PublicationOnly);

        private static readonly Lazy<string> _executablePath =
            new(() => GetAssembly().Location,
                LazyThreadSafetyMode.PublicationOnly);

        private static readonly Lazy<string> _executableName =
            new(() =>
                {
                    var location = _executablePath.Value;
                    if (string.IsNullOrEmpty(location))
                    {
                        var commandLineArgs = Environment.GetCommandLineArgs();

                        if (commandLineArgs.Length > 0)
                        {
                            location = commandLineArgs[0];
                        }
                    }

                    return Path.GetFileNameWithoutExtension(location).Replace(" ", "");
                },
                LazyThreadSafetyMode.PublicationOnly);

        /// <param name="description">The description of the command, shown in help.</param>
        public RootCommand(string description = "") : base(ExecutableName, description)
        {
        }

        // FIX: (RootCommand) worthwhile to consolidate calls to Get*Assembly?
        internal static Assembly GetAssembly() => _assembly.Value;

        /// <summary>
        /// The name of the currently running executable.
        /// </summary>
        public static string ExecutableName => _executableName.Value;

        /// <summary>
        /// The path to the currently running executable.
        /// </summary>
        public static string ExecutablePath => _executablePath.Value;

        private protected override void RemoveAlias(string alias)
        {
            if (!string.Equals(alias, ExecutableName, StringComparison.Ordinal))
            {
                base.RemoveAlias(alias);
            }
        }
    }
}
