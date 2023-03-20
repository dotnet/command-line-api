// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine;
using System.CommandLine.Help;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.CommandLine.Parsing;
using System.Threading;

namespace System.CommandLine
{
    public partial class CommandLineBuilder
    {
        /// <summary>
        /// Enables signaling and handling of process termination via a <see cref="CancellationToken"/> that can be passed to a <see cref="CliAction"/> during invocation.
        /// </summary>
        /// <param name="timeout">
        /// Optional timeout for the command to process the exit cancellation.
        /// If not passed, a default timeout of 2 seconds is enforced.
        /// If positive value is passed - command is forcefully terminated after the timeout with exit code 130 (as if <see cref="CancelOnProcessTermination"/> was not called).
        /// Host enforced timeout for ProcessExit event cannot be extended - default is 2 seconds: https://docs.microsoft.com/en-us/dotnet/api/system.appdomain.processexit?view=net-6.0.
        /// </param>
        /// <returns>The reference to this <see cref="CommandLineBuilder"/> instance.</returns>
        public CommandLineBuilder CancelOnProcessTermination(TimeSpan? timeout = null)
        {
            ProcessTerminationTimeout = timeout ?? TimeSpan.FromSeconds(2);

            return this;
        }

        /// <summary>
        /// Enables the parser to recognize and expand POSIX-style bundled options.
        /// </summary>
        /// <param name="value"><see langword="true"/> to parse POSIX bundles; otherwise, <see langword="false"/>.</param>
        /// <returns>The reference to this <see cref="CommandLineBuilder"/> instance.</returns>
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
        public CommandLineBuilder EnablePosixBundling(bool value = true)
        {
            EnablePosixBundlingFlag = value;

            return this;
        }

        /// <inheritdoc cref="EnvironmentVariablesDirective"/>
        /// <returns>The reference to this <see cref="CommandLineBuilder"/> instance.</returns>
        public CommandLineBuilder UseEnvironmentVariableDirective()
        {
            Directives.Add(new EnvironmentVariablesDirective());

            return this;
        }

        /// <summary>
        /// Uses the default configuration.
        /// </summary>
        /// <remarks>Calling this method is the equivalent to calling:
        /// <code>
        ///   builder
        ///     .UseVersionOption()
        ///     .UseHelp()
        ///     .UseEnvironmentVariableDirective()
        ///     .UseParseDirective()
        ///     .UseSuggestDirective()
        ///     .RegisterWithDotnetSuggest()
        ///     .UseTypoCorrections()
        ///     .UseParseErrorReporting()
        ///     .UseExceptionHandler()
        ///     .CancelOnProcessTermination();
        /// </code>
        /// </remarks>
        /// <returns>The reference to this <see cref="CommandLineBuilder"/> instance.</returns>
        public CommandLineBuilder UseDefaults()
        {
            return UseVersionOption()
                   .UseHelp()
                   .UseEnvironmentVariableDirective()
                   .UseParseDirective()
                   .UseSuggestDirective()
                   .UseTypoCorrections()
                   .UseParseErrorReporting()
                   .UseExceptionHandler()
                   .CancelOnProcessTermination();
        }

        /// <summary>
        /// Enables an exception handler to catch any unhandled exceptions thrown by a command handler during invocation.
        /// </summary>
        /// <param name="onException">A delegate that will be called when an exception is thrown by a command handler.
        /// It needs to return an exit code to be used when an exception is thrown.</param>
        /// <param name="errorExitCode">The exit code to be used when an exception is thrown.</param>
        /// <returns>The reference to this <see cref="CommandLineBuilder"/> instance.</returns>
        public CommandLineBuilder UseExceptionHandler(Func<Exception, InvocationContext, int>? onException = null, int errorExitCode = 1)
        {
            ExceptionHandler = onException ?? Default;

            return this;

            int Default(Exception exception, InvocationContext context)
            {
                if (exception is not OperationCanceledException)
                {
                    ConsoleHelpers.ResetTerminalForegroundColor();
                    ConsoleHelpers.SetTerminalForegroundRed();

                    context.Console.Error.Write(LocalizationResources.ExceptionHandlerHeader());
                    context.Console.Error.WriteLine(exception.ToString());

                    ConsoleHelpers.ResetTerminalForegroundColor();
                }
                return errorExitCode;
            }
        }

        /// <summary>
        /// Configures the application to show help when one of the following options are specified on the command line:
        /// <code>
        ///    -h
        ///    /h
        ///    --help
        ///    -?
        ///    /?
        /// </code>
        /// </summary>
        /// <param name="maxWidth">Maximum output width for default help builder.</param>
        /// <returns>The reference to this <see cref="CommandLineBuilder"/> instance.</returns>
        public CommandLineBuilder UseHelp(int? maxWidth = null)
        {
            return UseHelp(new HelpOption()
            {
                Action = new HelpAction()
                {
                    Builder = new HelpBuilder(maxWidth ?? int.MaxValue)
                }
            });
        }

        /// <summary>
        /// Configures the application to show help when one of the specified option aliases are used on the command line.
        /// </summary>
        /// <remarks>The specified aliases will override the default values.</remarks>
        /// <param name="name">The name of the help option.</param>
        /// <param name="helpAliases">The set of aliases that can be specified on the command line to request help.</param>
        /// <returns>The reference to this <see cref="CommandLineBuilder"/> instance.</returns>
        public CommandLineBuilder UseHelp(string name, params string[] helpAliases)
        {
            return UseHelp(new HelpOption(name, helpAliases));
        }

        internal CommandLineBuilder UseHelp(HelpOption helpOption)
        {
            if (HelpOption is null)
            {
                HelpOption = helpOption;

                OverwriteOrAdd(Command, helpOption);
            }
            return this;
        }

        /// <inheritdoc cref="ParseDirective"/>
        /// <param name="errorExitCode">If the parse result contains errors, this exit code will be used when the process exits.</param>
        /// <returns>The reference to this <see cref="CommandLineBuilder"/> instance.</returns>
        public CommandLineBuilder UseParseDirective(
            int errorExitCode = 1)
        {
            Directives.Add(new ParseDirective(errorExitCode));

            return this;
        }

        /// <summary>
        /// Configures the command line to write error information to standard error when there are errors parsing command line input.
        /// </summary>
        /// <param name="errorExitCode">The exit code to use when parser errors occur.</param>
        /// <returns>The reference to this <see cref="CommandLineBuilder"/> instance.</returns>
        public CommandLineBuilder UseParseErrorReporting(
            int errorExitCode = 1)
        {
            ParseErrorReportingExitCode = errorExitCode;

            return this;
        }

        /// <inheritdoc cref="SuggestDirective"/>
        /// <returns>The reference to this <see cref="CommandLineBuilder"/> instance.</returns>
        public CommandLineBuilder UseSuggestDirective()
        {
            Directives.Add(new SuggestDirective());

            return this;
        }

        /// <summary>
        /// Configures the application to provide alternative suggestions when a parse error is detected.
        /// </summary>
        /// <param name="maxLevenshteinDistance">The maximum Levenshtein distance for suggestions based on detected typos in command line input.</param>
        /// <returns>The reference to this <see cref="CommandLineBuilder"/> instance.</returns>
        public CommandLineBuilder UseTypoCorrections(
            int maxLevenshteinDistance = 3)
        {
            if (maxLevenshteinDistance <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxLevenshteinDistance));
            }

            MaxLevenshteinDistance = maxLevenshteinDistance;

            return this;
        }

        /// <summary>
        /// Specifies a delegate used to replace any token prefixed with <code>@</code> with zero or more other tokens, prior to parsing. 
        /// </summary>
        /// <param name="replaceToken">Replaces the specified token with any number of other tokens.</param>
        /// <returns>The reference to this <see cref="CommandLineBuilder"/> instance.</returns>
        public CommandLineBuilder UseTokenReplacer(TryReplaceToken? replaceToken)
        {
            EnableTokenReplacement = replaceToken is not null;
            TokenReplacer = replaceToken;

            return this;
        }

        /// <summary>
        /// Enables the use of a option (defaulting to the alias <c>--version</c>) which when specified in command line input will short circuit normal command handling and instead write out version information before exiting.
        /// </summary>
        /// <returns>The reference to this <see cref="CommandLineBuilder"/> instance.</returns>
        public CommandLineBuilder UseVersionOption()
        {
            if (VersionOption is null)
            {
                OverwriteOrAdd(Command, VersionOption = new());
            }

            return this;
        }

        /// <inheritdoc cref="UseVersionOption()"/>
        /// <param name="name">The name of the version option.</param>
        /// <param name="aliases">One or more aliases to use instead of the default to signal that version information should be displayed.</param>
        /// <returns>The reference to this <see cref="CommandLineBuilder"/> instance.</returns>
        public CommandLineBuilder UseVersionOption(string name, params string[] aliases)
        {
            if (VersionOption is null)
            {
                OverwriteOrAdd(Command, VersionOption = new(name, aliases));
            }

            return this;
        }

        /// <summary>
        /// Creating a config from Command might cause side effects for Command.
        /// The config type may add Options to the Command.
        /// Since single command can be parsed multiple times with different configs,
        /// we need to handle it properly.
        /// Ideally config should not mutate Command at all.
        /// </summary>
        private static void OverwriteOrAdd<T>(Command command, T option) where T : Option
        {
            if (command.HasOptions)
            {
                for (int i = 0; i < command.Options.Count; i++)
                {
                    if (command.Options[i] is T)
                    {
                        command.Options[i] = option;
                        return;
                    }
                }
            }

            command.Options.Add(option);
        }
    }
}
