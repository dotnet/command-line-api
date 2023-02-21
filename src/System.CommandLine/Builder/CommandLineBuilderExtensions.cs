// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine;
using System.CommandLine.Binding;
using System.CommandLine.Help;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.CommandLine.Parsing;
using System.IO;
using System.Threading;
using Process = System.CommandLine.Invocation.Process;

namespace System.CommandLine
{
    /// <summary>
    /// Provides extension methods for <see cref="CommandLineBuilder"/>.
    /// </summary>
    public static class CommandLineBuilderExtensions
    {
        /// <summary>
        /// Enables signaling and handling of process termination via a <see cref="CancellationToken"/> that can be passed to a <see cref="ICommandHandler"/> during invocation.
        /// </summary>
        /// <param name="builder">A command line builder.</param>
        /// <param name="timeout">
        /// Optional timeout for the command to process the exit cancellation.
        /// If not passed, a default timeout of 2 seconds is enforced.
        /// If positive value is passed - command is forcefully terminated after the timeout with exit code 130 (as if <see cref="CancelOnProcessTermination"/> was not called).
        /// Host enforced timeout for ProcessExit event cannot be extended - default is 2 seconds: https://docs.microsoft.com/en-us/dotnet/api/system.appdomain.processexit?view=net-6.0.
        /// </param>
        /// <returns>The same instance of <see cref="CommandLineBuilder"/>.</returns>
        public static CommandLineBuilder CancelOnProcessTermination(
            this CommandLineBuilder builder,
            TimeSpan? timeout = null)
        {
            builder.ProcessTerminationTimeout = timeout ?? TimeSpan.FromSeconds(2);

            return builder;
        }

        /// <summary>
        /// Enables the parser to recognize and expand POSIX-style bundled options.
        /// </summary>
        /// <param name="builder">A command line builder.</param>
        /// <param name="value"><see langword="true"/> to parse POSIX bundles; otherwise, <see langword="false"/>.</param>
        /// <returns>The same instance of <see cref="CommandLineBuilder"/>.</returns>
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
        public static CommandLineBuilder EnablePosixBundling(
            this CommandLineBuilder builder,
            bool value = true)
        {
            builder.EnablePosixBundling = value;
            return builder;
        }

        /// <summary>
        /// Ensures that the application is registered with the <c>dotnet-suggest</c> tool to enable command line completions.
        /// </summary>
        /// <remarks>For command line completions to work, users must install the <c>dotnet-suggest</c> tool as well as the appropriate shim script for their shell.</remarks>
        /// <param name="builder">A command line builder.</param>
        /// <returns>The same instance of <see cref="CommandLineBuilder"/>.</returns>
        public static CommandLineBuilder RegisterWithDotnetSuggest(
            this CommandLineBuilder builder)
        {
            builder.AddMiddleware(async (context, cancellationToken, next) =>
            {
                var feature = new FeatureRegistration("dotnet-suggest-registration");

                await feature.EnsureRegistered(async () =>
                {
                    var stdOut = StringBuilderPool.Default.Rent();
                    var stdErr = StringBuilderPool.Default.Rent();

                    try
                    {
                        var currentProcessFullPath = Diagnostics.Process.GetCurrentProcess().MainModule?.FileName;
                        var currentProcessFileNameWithoutExtension = Path.GetFileNameWithoutExtension(currentProcessFullPath);

                        var dotnetSuggestProcess = Process.StartProcess(
                            command: "dotnet-suggest",
                            args: $"register --command-path \"{currentProcessFullPath}\" --suggestion-command \"{currentProcessFileNameWithoutExtension}\"",
                            stdOut: value => stdOut.Append(value),
                            stdErr: value => stdOut.Append(value));

                        await dotnetSuggestProcess.CompleteAsync(cancellationToken);

                        return $@"{dotnetSuggestProcess.StartInfo.FileName} exited with code {dotnetSuggestProcess.ExitCode}
OUT:
{stdOut}
ERR:
{stdErr}";
                    }
                    catch (Exception exception)
                    {
                        return $@"Exception during registration:
{exception}";
                    }
                    finally
                    {
                        StringBuilderPool.Default.ReturnToPool(stdOut);
                        StringBuilderPool.Default.ReturnToPool(stdErr);
                    }
                });

                await next(context, cancellationToken);
            }, MiddlewareOrderInternal.RegisterWithDotnetSuggest);

            return builder;
        }

        /// <inheritdoc cref="EnvironmentVariablesDirective"/>
        /// <param name="builder">A command line builder.</param>
        /// <returns>The same instance of <see cref="CommandLineBuilder"/>.</returns>
        public static CommandLineBuilder UseEnvironmentVariableDirective(
            this CommandLineBuilder builder)
        {
            (builder.Directives ??= new()).Add(new EnvironmentVariablesDirective());

            return builder;
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
        /// <param name="builder">A command line builder.</param>
        /// <returns>The same instance of <see cref="CommandLineBuilder"/>.</returns>
        public static CommandLineBuilder UseDefaults(this CommandLineBuilder builder)
        {
            return builder
                   .UseVersionOption()
                   .UseHelp()
                   .UseEnvironmentVariableDirective()
                   .UseParseDirective()
                   .UseSuggestDirective()
                   .RegisterWithDotnetSuggest()
                   .UseTypoCorrections()
                   .UseParseErrorReporting()
                   .UseExceptionHandler()
                   .CancelOnProcessTermination();
        }

        /// <summary>
        /// Enables an exception handler to catch any unhandled exceptions thrown by a command handler during invocation.
        /// </summary>
        /// <param name="builder">A command line builder.</param>
        /// <param name="onException">A delegate that will be called when an exception is thrown by a command handler.</param>
        /// <param name="errorExitCode">The exit code to be used when an exception is thrown.</param>
        /// <returns>The same instance of <see cref="CommandLineBuilder"/>.</returns>
        public static CommandLineBuilder UseExceptionHandler(
            this CommandLineBuilder builder,
            Action<Exception, InvocationContext>? onException = null,
            int? errorExitCode = null)
        {
            builder.ExceptionHandler = onException ?? Default;

            return builder;

            void Default(Exception exception, InvocationContext context)
            {
                if (exception is not OperationCanceledException)
                {
                    context.Console.ResetTerminalForegroundColor();
                    context.Console.SetTerminalForegroundRed();

                    context.Console.Error.Write(context.LocalizationResources.ExceptionHandlerHeader());
                    context.Console.Error.WriteLine(exception.ToString());

                    context.Console.ResetTerminalForegroundColor();
                }
                context.ExitCode = errorExitCode ?? 1;
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
        /// <param name="builder">A command line builder.</param>
        /// <param name="maxWidth">Maximum output width for default help builder.</param>
        /// <returns>The same instance of <see cref="CommandLineBuilder"/>.</returns>
        public static CommandLineBuilder UseHelp(this CommandLineBuilder builder, int? maxWidth = null)
        {
            return builder.UseHelp(new HelpOption(() => builder.LocalizationResources), maxWidth);
        }

        /// <summary>
        /// Configures the application to show help when one of the specified option aliases are used on the command line.
        /// </summary>
        /// <remarks>The specified aliases will override the default values.</remarks>
        /// <param name="builder">A command line builder.</param>
        /// <param name="helpAliases">The set of aliases that can be specified on the command line to request help.</param>
        /// <returns>The same instance of <see cref="CommandLineBuilder"/>.</returns>
        public static CommandLineBuilder UseHelp(
            this CommandLineBuilder builder,
            params string[] helpAliases)
        {
            return builder.UseHelp(new HelpOption(helpAliases, () => builder.LocalizationResources));
        }

        /// <summary>
        /// Configures the application to show help when one of the specified option aliases are used on the command line.
        /// </summary>
        /// <remarks>The specified aliases will override the default values.</remarks>
        /// <param name="builder">A command line builder.</param>
        /// <param name="customize">A delegate that will be called to customize help if help is requested.</param>
        /// <param name="maxWidth">Maximum output width for default help builder.</param>
        /// <returns>The same instance of <see cref="CommandLineBuilder"/>.</returns>
        public static CommandLineBuilder UseHelp(
            this CommandLineBuilder builder,
            Action<HelpContext> customize,
            int? maxWidth = null)
        {
            builder.CustomizeHelpLayout(customize);

            if (builder.HelpOption is null)
            {
                builder.UseHelp(new HelpOption(() => builder.LocalizationResources), maxWidth);
            }

            return builder;
        }

        internal static CommandLineBuilder UseHelp(
            this CommandLineBuilder builder,
            HelpOption helpOption,
            int? maxWidth = null)
        {
            if (builder.HelpOption is null)
            {
                builder.HelpOption = helpOption;
                builder.Command.Options.Add(helpOption);
                builder.MaxHelpWidth = maxWidth;
            }
            return builder;
        }

        /// <summary>
        /// Specifies an <see cref="HelpBuilder"/> to be used to format help output when help is requested.
        /// </summary>
        /// <param name="builder">A command line builder.</param>
        /// <param name="getHelpBuilder">A delegate that returns an instance of <see cref="HelpBuilder"/></param>
        /// <returns>The same instance of <see cref="CommandLineBuilder"/>.</returns>
        public static TBuilder UseHelpBuilder<TBuilder>(this TBuilder builder,
            Func<BindingContext, HelpBuilder> getHelpBuilder)
            where TBuilder : CommandLineBuilder
        {
            if (builder is null)
            {
                throw new ArgumentNullException(nameof(builder));
            }
            builder.UseHelpBuilderFactory(getHelpBuilder);
            return builder;
        }

        /// <summary>
        /// Adds a middleware delegate to the invocation pipeline called before a command handler is invoked.
        /// </summary>
        /// <param name="builder">A command line builder.</param>
        /// <param name="middleware">A delegate that will be invoked before a call to a command handler.</param>
        /// <param name="order">A value indicating the order in which the added delegate will be invoked relative to others in the pipeline.</param>
        /// <returns>The same instance of <see cref="CommandLineBuilder"/>.</returns>
        public static CommandLineBuilder AddMiddleware(
            this CommandLineBuilder builder,
            InvocationMiddleware middleware,
            MiddlewareOrder order = MiddlewareOrder.Default)
        {
            builder.AddMiddleware(
                middleware,
                order);

            return builder;
        }

        /// <summary>
        /// Adds a middleware delegate to the invocation pipeline called before a command handler is invoked.
        /// </summary>
        /// <param name="onInvoke">A delegate that will be invoked before a call to a command handler.</param>
        /// <param name="order">A value indicating the order in which the added delegate will be invoked relative to others in the pipeline.</param>
        /// <param name="builder">A command line builder.</param>
        /// <returns>The same instance of <see cref="CommandLineBuilder"/>.</returns>
        public static CommandLineBuilder AddMiddleware(
            this CommandLineBuilder builder,
            Action<InvocationContext> onInvoke,
            MiddlewareOrder order = MiddlewareOrder.Default)
        {
            builder.AddMiddleware(async (context, cancellationToken, next) =>
            {
                onInvoke(context);
                await next(context, cancellationToken);
            }, order);

            return builder;
        }


        /// <inheritdoc cref="ParseDirective"/>
        /// <param name="builder">A command line builder.</param>
        /// <param name="errorExitCode">If the parse result contains errors, this exit code will be used when the process exits.</param>
        /// <returns>The same instance of <see cref="CommandLineBuilder"/>.</returns>
        public static CommandLineBuilder UseParseDirective(
            this CommandLineBuilder builder,
            int errorExitCode = 1)
        {
            (builder.Directives ??= new()).Add(new ParseDirective(errorExitCode));

            return builder;
        }

        /// <summary>
        /// Configures the command line to write error information to standard error when there are errors parsing command line input.
        /// </summary>
        /// <param name="errorExitCode">The exit code to use when parser errors occur.</param>
        /// <param name="builder">A command line builder.</param>
        /// <returns>The same instance of <see cref="CommandLineBuilder"/>.</returns>
        public static CommandLineBuilder UseParseErrorReporting(
            this CommandLineBuilder builder,
            int errorExitCode = 1)
        {
            builder.ParseErrorReportingExitCode = errorExitCode;

            return builder;
        }

        /// <inheritdoc cref="SuggestDirective"/>
        /// <param name="builder">A command line builder.</param>
        /// <returns>The same instance of <see cref="CommandLineBuilder"/>.</returns>
        public static CommandLineBuilder UseSuggestDirective(
            this CommandLineBuilder builder)
        {
            (builder.Directives ??= new()).Add(new SuggestDirective());

            return builder;
        }

        /// <summary>
        /// Configures the application to provide alternative suggestions when a parse error is detected.
        /// </summary>
        /// <param name="builder">A command line builder.</param>
        /// <param name="maxLevenshteinDistance">The maximum Levenshtein distance for suggestions based on detected typos in command line input.</param>
        /// <returns>The same instance of <see cref="CommandLineBuilder"/>.</returns>
        public static CommandLineBuilder UseTypoCorrections(
            this CommandLineBuilder builder,
            int maxLevenshteinDistance = 3)
        {
            if (maxLevenshteinDistance <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxLevenshteinDistance));
            }

            builder.MaxLevenshteinDistance = maxLevenshteinDistance;

            return builder;
        }

        /// <summary>
        /// Specifies localization resources to be used when displaying help, error messages, and other user-facing strings.
        /// </summary>
        /// <param name="builder">A command line builder.</param>
        /// <param name="validationMessages">The localizations resources to use.</param>
        /// <returns>The same instance of <see cref="CommandLineBuilder"/>.</returns>
        public static CommandLineBuilder UseLocalizationResources(
            this CommandLineBuilder builder,
            LocalizationResources validationMessages)
        {
            builder.LocalizationResources = validationMessages;
            return builder;
        }

        /// <summary>
        /// Specifies a delegate used to replace any token prefixed with <code>@</code> with zero or more other tokens, prior to parsing. 
        /// </summary>
        /// <param name="builder">A command line builder.</param>
        /// <param name="replaceToken">Replaces the specified token with any number of other tokens.</param>
        /// <returns>The same instance of <see cref="CommandLineBuilder"/>.</returns>
        public static CommandLineBuilder UseTokenReplacer(
            this CommandLineBuilder builder,
            TryReplaceToken? replaceToken)
        {
            builder.TokenReplacer = replaceToken;

            return builder;
        }

        /// <summary>
        /// Enables the use of a option (defaulting to the alias <c>--version</c>) which when specified in command line input will short circuit normal command handling and instead write out version information before exiting.
        /// </summary>
        /// <param name="builder">A command line builder.</param>
        /// <returns>The same instance of <see cref="CommandLineBuilder"/>.</returns>
        public static CommandLineBuilder UseVersionOption(
            this CommandLineBuilder builder)
        {
            if (builder.VersionOption is not null)
            {
                return builder;
            }

            builder.VersionOption = new (builder);
            builder.Command.Options.Add(builder.VersionOption);

            return builder;
        }

        /// <inheritdoc cref="UseVersionOption(System.CommandLine.CommandLineBuilder)"/>
        /// <param name="aliases">One or more aliases to use instead of the default to signal that version information should be displayed.</param>
        /// <param name="builder">A command line builder.</param>
        public static CommandLineBuilder UseVersionOption(
            this CommandLineBuilder builder,
            params string[] aliases)
        {
            if (builder.VersionOption is not null)
            {
                return builder;
            }

            builder.VersionOption = new (aliases, builder);
            builder.Command.Options.Add(builder.VersionOption);

            return builder;
        }
    }
}
