// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.IO;
using System.Linq;
using System.Reflection;

namespace System.CommandLine
{
    /// <summary>
    /// A command representing entry point of an application and used to Invoke the correct handlers.
    /// </summary>
    public class RootCommand : Command
    {
        /// <summary>
        /// Create a new instance of RootCommand
        /// </summary>
        /// <param name="description">The description of the command that could be used in a help display.</param>
        public RootCommand(string description = "") : base(ExeName, description)
        {
        }

        /// <summary>
        /// The name of the command. Defaults to the executable name.
        /// </summary>
        public override string Name
        {
            get => base.Name;
            set
            {
                base.Name = value;
                AddAlias(Name);
            }
        }

        private static readonly Lazy<string> executablePath = new Lazy<string>(() =>
        {
            return GetAssembly().Location;
        });

        private static readonly Lazy<string> executableName = new Lazy<string>(() =>
        {
            var location = executablePath.Value;
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
        /// The name of the executable currently running, or if missing the first string argument.
        /// </summary>
        public static string ExeName => executableName.Value;

        /// <summary>
        /// The path to the currently running executable.
        /// </summary>
        public static string ExePath => executablePath.Value;
    }
}
