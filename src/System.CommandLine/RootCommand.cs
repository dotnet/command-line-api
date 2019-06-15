// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.IO;
using System.Reflection;

namespace System.CommandLine
{
    public class RootCommand : Command
    {
        public RootCommand(string description = "") : base(ExeName, description)
        {
        }

        public override string Name
        {
            get => base.Name;
            set
            {
                base.Name = value;
                AddAlias(Name);
            }
        }

        private static readonly Lazy<string> executablePath =
            new Lazy<string>(() =>
                                 GetAssembly().Location);

        private static readonly Lazy<string> executableName =
            new Lazy<string>(() =>
                                 Path.GetFileNameWithoutExtension(GetAssembly().Location));

        private static Assembly GetAssembly() =>
            Assembly.GetEntryAssembly() ??
            Assembly.GetExecutingAssembly();

        public static string ExeName => executableName.Value;

        public static string ExePath => executablePath.Value;
    }
}
