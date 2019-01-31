// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Invocation;
using System.IO;
using System.Reflection;

namespace System.CommandLine
{
    public class RootCommand : Command
    {
        public RootCommand(
            string description = "",
            IReadOnlyCollection<Symbol> symbols = null,
            Argument argument = null,
            bool treatUnmatchedTokensAsErrors = true,
            ICommandHandler handler = null,
            bool isHidden = false) :
            base(ExeName,
                 description,
                 symbols,
                 argument,
                 treatUnmatchedTokensAsErrors,
                 handler,
                 isHidden)
        {
        }

        public override string Name
        {
            get => ExeName;
            set => throw new NotSupportedException("The root command's name cannot be changed.");
        }

        private static readonly Lazy<string> executablePath = 
            new Lazy<string>(() => Assembly.GetEntryAssembly().Location);
  
        private static readonly Lazy<string> executableName =
            new Lazy<string>(() =>
                Path.GetFileNameWithoutExtension(
                    (Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly()).Location));

        public static string ExeName { get; } = executableName.Value;

        public static string ExePath { get; } = executablePath.Value;
    }
}
