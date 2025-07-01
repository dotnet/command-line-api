// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Parsing;
using System.Linq;

namespace System.CommandLine
{
    /// <summary>
    /// Represents the configuration used by the <see cref="CommandLineParser"/>.
    /// </summary>
    public class CommandLineConfiguration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CommandLineConfiguration"/> class.
        /// </summary>
        /// <param name="rootCommand">The root command for the parser.</param>
        public CommandLineConfiguration(Command rootCommand)
        {
            RootCommand = rootCommand ?? throw new ArgumentNullException(nameof(rootCommand));
        }

        internal bool HasDirectives =>
            RootCommand switch
            {
                RootCommand root => root.Directives.Count > 0,
                _ => false
            };

        /// <summary>
        /// Enables the parser to recognize and expand POSIX-style bundled options.
        /// </summary>
        /// <param name="value"><see langword="true"/> to parse POSIX bundles; otherwise, <see langword="false"/>.</param>
        /// <remarks>
        /// POSIX conventions recommend that single-character options be allowed to be specified together after a single <c>-</c> prefix. When <see cref="EnablePosixBundling"/> is set to <see langword="true"/>, the following command lines are equivalent:
        /// 
        /// <code>
        ///     &gt; myapp -a -b -c
        ///     &gt; myapp -abc
        /// </code>
        /// 
        /// If an argument is provided after an option bundle, it applies to the last option in the bundle. When <see cref="EnablePosixBundling"/> is set to <see langword="true"/>, all of the following command lines are equivalent:
        /// <code>
        ///     &gt; myapp -a -b -c arg
        ///     &gt; myapp -abc arg
        ///     &gt; myapp -abcarg
        /// </code>
        ///
        /// </remarks>
        public bool EnablePosixBundling { get; set; } = true;

        /// <summary>
        /// Response file token replacer, enabled by default.
        /// To disable response files support, this property needs to be set to null.
        /// </summary>
        /// <remarks>
        /// When enabled, any token prefixed with <code>@</code> can be replaced with zero or more other tokens. This is mostly commonly used to expand tokens from response files and interpolate them into a command line prior to parsing.
        /// </remarks>
        public TryReplaceToken? ResponseFileTokenReplacer { get; set; } = StringExtensions.TryReadResponseFile;

        /// <summary>
        /// Gets the root command.
        /// </summary>
        public Command RootCommand { get; }

        /// <summary>
        /// Throws an exception if the parser configuration is ambiguous or otherwise not valid.
        /// </summary>
        /// <remarks>Due to the performance cost of this method, it is recommended to be used in unit testing or in scenarios where the parser is configured dynamically at runtime.</remarks>
        /// <exception cref="CommandLineConfigurationException">Thrown if the configuration is found to be invalid.</exception>
        public void ThrowIfInvalid()
        {
            ThrowIfInvalid(RootCommand);

            static void ThrowIfInvalid(Command command)
            {
                if (command.Parents.FlattenBreadthFirst(c => c.Parents).Any(ancestor => ancestor == command))
                {
                    throw new CommandLineConfigurationException($"Cycle detected in command tree. Command '{command.Name}' is its own ancestor.");
                }

                int count = command.Subcommands.Count + command.Options.Count;
                for (var i = 0; i < count; i++)
                {
                    Symbol symbol1 = GetChild(i, command, out AliasSet? aliases1);
                    for (var j = i + 1; j < count; j++)
                    {
                        Symbol symbol2 = GetChild(j, command, out AliasSet? aliases2);

                        if (symbol1.Name.Equals(symbol2.Name, StringComparison.Ordinal)
                            || (aliases1 is not null && aliases1.Contains(symbol2.Name)))
                        {
                            throw new CommandLineConfigurationException($"Duplicate alias '{symbol2.Name}' found on command '{command.Name}'.");
                        }
                        else if (aliases2 is not null && aliases2.Contains(symbol1.Name))
                        {
                            throw new CommandLineConfigurationException($"Duplicate alias '{symbol1.Name}' found on command '{command.Name}'.");
                        }

                        if (aliases1 is not null && aliases2 is not null)
                        {
                            // take advantage of the fact that we are dealing with two hash sets
                            if (aliases1.Overlaps(aliases2))
                            {
                                foreach (string symbol2Alias in aliases2)
                                {
                                    if (aliases1.Contains(symbol2Alias))
                                    {
                                        throw new CommandLineConfigurationException($"Duplicate alias '{symbol2Alias}' found on command '{command.Name}'.");
                                    }
                                }
                            }
                        }
                    }

                    if (symbol1 is Command childCommand)
                    {
                        ThrowIfInvalid(childCommand);
                    }
                }
            }

            static Symbol GetChild(int index, Command command, out AliasSet? aliases)
            {
                if (index < command.Subcommands.Count)
                {
                    aliases = command.Subcommands[index]._aliases;
                    return command.Subcommands[index];
                }

                aliases = command.Options[index - command.Subcommands.Count]._aliases;
                return command.Options[index - command.Subcommands.Count];
            }
        }
    }
}