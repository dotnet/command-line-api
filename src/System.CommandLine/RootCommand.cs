// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.IO;
using System.Linq;
using System.Reflection;

namespace System.CommandLine
{
    /// <summary>
    /// A command representing an application entry point.
    /// </summary>
    public class RootCommand : Command
    {
        /// <summary>
        /// Create a new instance of RootCommand
        /// </summary>
        /// <param name="description">The description of the command shown in help.</param>
        public RootCommand(string description = "") : base(ExecutableName, description)
        {
        }

        /// <summary>
        /// The name of the command. Defaults to the executable name.
        /// </summary>
        public override string Name
        {
            // FIX: (Name) devirtualize
            get => base.Name;
            set
            {
                base.Name = value;
                AddAlias(Name);
            }
        }

        private static readonly Lazy<string> _executablePath = new Lazy<string>(() =>
        {
            return GetAssembly().Location;
        });

        private static readonly Lazy<string> _executableName = new Lazy<string>(() =>
        {
            var location = _executablePath.Value;
            if (string.IsNullOrEmpty(location))
            {
                location = Environment.GetCommandLineArgs().FirstOrDefault();
            }
            return Path.GetFileNameWithoutExtension(location).Replace(" ", "");
        });

        private static Assembly GetAssembly() =>
            Assembly.GetEntryAssembly() ??
            Assembly.GetExecutingAssembly();

        /// <summary>
        /// The name of the currently running executable.
        /// </summary>
        public static string ExecutableName => _executableName.Value;

        /// <summary>
        /// The path to the currently running executable.
        /// </summary>
        public static string ExecutablePath => _executablePath.Value;
    }
}
