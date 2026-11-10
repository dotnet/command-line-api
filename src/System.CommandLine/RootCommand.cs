// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Completions;
using System.CommandLine.Help;
using System.IO;
using System.Linq;
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
        private static string? _executablePath;
        private static string? _executableName;
        private static string? _toolCommandName;
        private static bool _toolCommandNameInitialized;
        private string? _helpName;
        private bool _helpNameSet;

        /// <param name="description">The description of the command, shown in help.</param>
        public RootCommand(string description = "") : base(ExecutableName, description)
        {
            Options.Add(new HelpOption());
            Options.Add(new VersionOption()); 
            Directives = new ChildSymbolList<Directive>(this)
            {
                new SuggestDirective()
            };
        }

        /// <summary>
        /// Gets or sets the name used for the root command in help output.
        /// </summary>
        /// <remarks>
        /// If not explicitly set, defaults to the <c>ToolCommandName</c> MSBuild property value
        /// (when available via assembly metadata), or <c>null</c> to fall back to <see cref="Symbol.Name"/>.
        /// </remarks>
        public string? HelpName
        {
            get => _helpNameSet ? _helpName : ToolCommandName;
            set
            {
                _helpName = value;
                _helpNameSet = true;
            }
        }
     
        /// <summary>
        /// Represents all of the directives that are valid under the root command.
        /// </summary>
        public IList<Directive> Directives { get; }

        /// <summary>
        /// Adds a <see cref="Directive"/> to the command.
        /// </summary>
        public void Add(Directive directive) => Directives.Add(directive);

        /// <summary>
        /// The name of the currently running executable.
        /// </summary>
        /// <remarks>
        /// The name is resolved in the following order:
        /// <list type="number">
        ///   <item><description>The <c>System.CommandLine.ExecutableName</c> value read from <see cref="AppContext"/>, when there is no managed entry point (<see cref="Assembly.GetEntryAssembly"/> returns <see langword="null"/>) - most notably when hosted as a NativeAOT native library. In that case <see cref="Environment.GetCommandLineArgs"/> reflects the host process rather than this component. The value is emitted into the app's <c>runtimeconfig.json</c> by the <c>System.CommandLine</c> build targets and defaults to the assembly name.</description></item>
        ///   <item><description>The file name (without extension) of the currently running executable, from <see cref="Environment.GetCommandLineArgs"/>.</description></item>
        ///   <item><description>The <c>System.CommandLine.ExecutableName</c> value from <see cref="AppContext"/>, if the executable path was unavailable.</description></item>
        ///   <item><description>The literal <c>"app"</c>, as a final fallback.</description></item>
        /// </list>
        /// </remarks>
        public static string ExecutableName
            => _executableName ??= GetExecutableName();

        /// <summary>
        /// The path to the currently running executable.
        /// </summary>
        public static string ExecutablePath => _executablePath ??= GetExecutablePath();

        private static string? ToolCommandName
        {
            get
            {
                if (!_toolCommandNameInitialized)
                {
                    _toolCommandName = Assembly.GetEntryAssembly()?
                        .GetCustomAttributes<AssemblyMetadataAttribute>()
                        .FirstOrDefault(a => a.Key == "System.CommandLine.ToolCommandName")?.Value;
                    _toolCommandNameInitialized = true;
                }

                return _toolCommandName;
            }
        }

        private static string GetExecutablePath()
        {
            var args = Environment.GetCommandLineArgs();
            return args.Length > 0 ? args[0] : string.Empty;
        }

        private static string GetExecutableName()
        {
            // When there is no managed entry point - most notably when hosted as a NativeAOT
            // native library - Environment.GetCommandLineArgs() reflects the host process (its
            // first element is the host executable, e.g. "dotnet") rather than this component.
            // In that case prefer the executable name injected by the build targets via
            // AppContext (issue #2812).
            if (Assembly.GetEntryAssembly() is null
                && AppContext.GetData("System.CommandLine.ExecutableName") is string injectedName
                && injectedName.Length > 0)
            {
                return injectedName;
            }

            var path = ExecutablePath;

            if (path.Length > 0)
            {
                return Path.GetFileNameWithoutExtension(path).Replace(" ", "");
            }

            if (AppContext.GetData("System.CommandLine.ExecutableName") is string name && name.Length > 0)
            {
                return name;
            }

            return "app";
        }
    }
}
