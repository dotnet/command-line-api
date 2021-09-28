﻿// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Binding;
using System.CommandLine.Help;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.CommandLine.Parsing;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using static System.Environment;
using Process = System.CommandLine.Invocation.Process;

namespace System.CommandLine.Builder
{
    /// <summary>
    /// Provides extension methods for <see cref="CommandBuilder"/>.
    /// </summary>
    public static class CommandLineBuilderExtensions
    {
        private static readonly Lazy<string> _assemblyVersion =
            new(() =>
            {
                var assembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
                var assemblyVersionAttribute = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
                if (assemblyVersionAttribute is null)
                {
                    return assembly.GetName().Version.ToString();
                }
                else
                {
                    return assemblyVersionAttribute.InformationalVersion;
                }
            });

        /// <summary>
        /// Enables signaling and handling of process termination via a <see cref="CancellationToken"/> that can be passed to a <see cref="ICommandHandler"/> during invocation.
        /// </summary>
        /// <param name="builder">A command line builder.</param>
        /// <returns>The same instance of <see cref="CommandLineBuilder"/>.</returns>
        public static CommandLineBuilder CancelOnProcessTermination(this CommandLineBuilder builder)
        {
            builder.AddMiddleware(async (context, next) =>
            {
                bool cancellationHandlingAdded = false;
                ManualResetEventSlim? blockProcessExit = null;
                ConsoleCancelEventHandler? consoleHandler = null;
                EventHandler? processExitHandler = null;

                context.CancellationHandlingAdded += (CancellationTokenSource cts) =>
                {
                    blockProcessExit = new ManualResetEventSlim(initialState: false);
                    cancellationHandlingAdded = true;
                    consoleHandler = (_, args) =>
                    {
                        cts.Cancel();
                        // Stop the process from terminating.
                        // Since the context was cancelled, the invocation should
                        // finish and Main will return.
                        args.Cancel = true;
                    };
                    processExitHandler = (_, _) =>
                    {
                        cts.Cancel();
                        // The process exits as soon as the event handler returns.
                        // We provide a return value using Environment.ExitCode
                        // because Main will not finish executing.
                        // Wait for the invocation to finish.
                        blockProcessExit.Wait();
                        Environment.ExitCode = context.ExitCode;
                    };
                    Console.CancelKeyPress += consoleHandler;
                    AppDomain.CurrentDomain.ProcessExit += processExitHandler;
                };

                try
                {
                    await next(context);
                }
                finally
                {
                    if (cancellationHandlingAdded)
                    {
                        Console.CancelKeyPress -= consoleHandler;
                        AppDomain.CurrentDomain.ProcessExit -= processExitHandler;
                        blockProcessExit!.Set();
                    }
                }
            }, MiddlewareOrderInternal.Startup);

            return builder;
        }

        public static CommandLineBuilder ConfigureConsole(
            this CommandLineBuilder builder,
            Func<BindingContext, IConsole> createConsole)
        {
            builder.AddMiddleware(async (context, next) =>
            {
                context.BindingContext.ConsoleFactory = new AnonymousConsoleFactory(createConsole);
                await next(context);
            }, MiddlewareOrderInternal.ConfigureConsole);

            return builder;
        }

        /// <summary>
        /// Enables the parser to recognize command line directives.
        /// </summary>
        /// <param name="builder">A command line builder.</param>
        /// <param name="value">If set to <see langword="true"/>, then directives are enabled. Otherwise, they are parsed like any other token.</param>
        /// <returns>The same instance of <see cref="CommandLineBuilder"/>.</returns>
        /// <seealso cref="IDirectiveCollection"/>
        public static CommandLineBuilder EnableDirectives(
            this CommandLineBuilder builder,
            bool value = true)
        {
            builder.EnableDirectives = value;
            return builder;
        }

        /// <summary>
        /// Enables the parser to recognize and expand POSIX-style bundled options.
        /// </summary>
        /// <param name="builder">A command line builder.</param>
        /// <param name="value">If set to <see langword="true"/>, then POSIX bundles are parsed. ; otherwise, <see langword="false"/>.</param>
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

        /// <inheritdoc cref="CommandLineBuilder.ResponseFileHandling"/>
        /// <param name="responseFileHandling">Specifies whether or how response files are parsed.</param>
        /// <param name="builder">A command line builder.</param>
        /// <returns>The same instance of <see cref="CommandLineBuilder"/>.</returns>
        public static CommandLineBuilder ParseResponseFileAs(
            this CommandLineBuilder builder,
            ResponseFileHandling responseFileHandling)
        {
            builder.ResponseFileHandling = responseFileHandling;
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
            builder.AddMiddleware(async (context, next) =>
            {
                var feature = new FeatureRegistration("dotnet-suggest-registration");

                await feature.EnsureRegistered(async () =>
                {
                    var stdOut = StringBuilderPool.Default.Rent();
                    var stdErr = StringBuilderPool.Default.Rent();

                    try
                    {
                        var currentProcessFullPath = Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
                        var currentProcessFileNameWithoutExtension = Path.GetFileNameWithoutExtension(currentProcessFullPath);

                        var dotnetSuggestProcess = Process.StartProcess(
                            command: "dotnet-suggest",
                            args: $"register --command-path \"{currentProcessFullPath}\" --suggestion-command \"{currentProcessFileNameWithoutExtension}\"",
                            stdOut: value => stdOut.Append(value),
                            stdErr: value => stdOut.Append(value));

                        await dotnetSuggestProcess.CompleteAsync();

                        return string.Format(@"{0} exited with code {1}
OUT:
{2}
ERR:
{3}", dotnetSuggestProcess.StartInfo.FileName, dotnetSuggestProcess.ExitCode, stdOut, stdErr);
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

                await next(context);
            }, MiddlewareOrderInternal.RegisterWithDotnetSuggest);

            return builder;
        }

        /// <summary>
        /// Enables the use of the <c>[debug]</c> directive, which will pause invocation prior to invoking any command handler so that developers can attach a debugger.
        /// </summary>
        /// <param name="builder">A command line builder.</param>
        /// <returns>The same instance of <see cref="CommandLineBuilder"/>.</returns>
        public static CommandLineBuilder UseDebugDirective(
            this CommandLineBuilder builder)
        {
            builder.AddMiddleware(async (context, next) =>
            {
                if (context.ParseResult.Directives.Contains("debug"))
                {
                    const string environmentVariableName = "DOTNET_COMMANDLINE_DEBUG_PROCESSES";

                    var process = Diagnostics.Process.GetCurrentProcess();
                    string debuggableProcessNames = GetEnvironmentVariable(environmentVariableName);
                    if (string.IsNullOrWhiteSpace(debuggableProcessNames))
                    {
                        context.Console.Error.WriteLine(context.Resources.DebugDirectiveExecutableNotSpecified(environmentVariableName, process.ProcessName));
                        context.ExitCode = 1;
                        return;
                    }
                    else
                    {
                        string[] processNames = debuggableProcessNames.Split(';');
                        if (processNames.Contains(process.ProcessName, StringComparer.Ordinal))
                        {
                            var processId = process.Id;
                            context.Console.Out.WriteLine(context.Resources.DebugDirectiveAttachToProcess(processId, process.ProcessName));
                            while (!Debugger.IsAttached)
                            {
                                await Task.Delay(500);
                            }
                        }
                        else
                        {
                            context.Console.Error.WriteLine(context.Resources.DebugDirectiveProcessNotIncludedInEnvironmentVariable(process.ProcessName, environmentVariableName, debuggableProcessNames));
                            context.ExitCode = 1;
                            return;
                        }
                    }
                }

                await next(context);
            }, MiddlewareOrderInternal.DebugDirective);

            return builder;
        }

        /// <summary>
        /// Enables the use of the <c>[env:key=value]</c> directive, allowing environment variables to be set from the command line during invocation.
        /// </summary>
        /// <param name="builder">A command line builder.</param>
        /// <returns>The same instance of <see cref="CommandLineBuilder"/>.</returns>
        public static CommandLineBuilder UseEnvironmentVariableDirective(
            this CommandLineBuilder builder)
        {
            builder.AddMiddleware((context, next) =>
            {
                if (context.ParseResult.Directives.TryGetValues("env", out var directives))
                {
                    foreach (var envDirective in directives)
                    {
                        var components = envDirective.Split(new[] { '=' }, count: 2);
                        var variable = components.Length > 0 ? components[0].Trim() : string.Empty;
                        if (string.IsNullOrEmpty(variable) || components.Length < 2)
                        {
                            continue;
                        }
                        var value = components[1].Trim();
                        SetEnvironmentVariable(variable, value);
                    }
                }

                return next(context);
            }, MiddlewareOrderInternal.EnvironmentVariableDirective);

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
        ///     .UseDebugDirective()
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
                   .UseDebugDirective()
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
            builder.AddMiddleware(async (context, next) =>
            {
                try
                {
                    await next(context);
                }
                catch (Exception exception)
                {
                    (onException ?? Default)(exception, context);
                }
            }, MiddlewareOrderInternal.ExceptionHandler);

            return builder;

            void Default(Exception exception, InvocationContext context)
            {
                if (exception is not OperationCanceledException)
                {
                    context.Console.ResetTerminalForegroundColor();
                    context.Console.SetTerminalForegroundRed();

                    context.Console.Error.Write(context.Resources.ExceptionHandlerHeader());
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
        /// <returns>The same instance of <see cref="CommandLineBuilder"/>.</returns>
        public static CommandLineBuilder UseHelp(this CommandLineBuilder builder)
        {
            return builder.UseHelp(new HelpOption());
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
            return builder.UseHelp(new HelpOption(helpAliases));
        }

        internal static CommandLineBuilder UseHelp(
            this CommandLineBuilder builder,
            HelpOption helpOption)
        {
            if (builder.HelpOption is null)
            {
                builder.HelpOption = helpOption;
                builder.Command.TryAddGlobalOption(helpOption);

                builder.AddMiddleware(async (context, next) =>
                {
                    if (!ShowHelp(context, builder.HelpOption))
                    {
                        await next(context);
                    }
                }, MiddlewareOrderInternal.HelpOption);
            }
            return builder;
        }

        /// <summary>
        /// Specifies an <see cref="IHelpBuilder"/> to be used to format help output when help is requested.
        /// </summary>
        /// <param name="builder">A command line builder.</param>
        /// <param name="getHelpBuilder">A delegate that returns an instance of <see cref="IHelpBuilder"/></param>
        /// <returns>The same instance of <see cref="CommandLineBuilder"/>.</returns>
        public static TBuilder UseHelpBuilder<TBuilder>(this TBuilder builder,
            Func<BindingContext, IHelpBuilder> getHelpBuilder)
            where TBuilder : CommandLineBuilder
        {
            if (builder is null)
            {
                throw new ArgumentNullException(nameof(builder));
            }
            builder.HelpBuilderFactory = getHelpBuilder;
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

        /// <inheritdoc cref="AddMiddleware(System.CommandLine.Builder.CommandLineBuilder,System.CommandLine.Invocation.InvocationMiddleware,System.CommandLine.Invocation.MiddlewareOrder)"/>
        [Obsolete("This method is obsolete and will be removed in a future version. Please use AddMiddleware instead.")]
        public static CommandLineBuilder UseMiddleware(
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
            builder.AddMiddleware(async (context, next) =>
            {
                onInvoke(context);
                await next(context);
            }, order);

            return builder;
        }

        /// <inheritdoc cref="AddMiddleware(System.CommandLine.Builder.CommandLineBuilder,System.CommandLine.Invocation.InvocationMiddleware,System.CommandLine.Invocation.MiddlewareOrder)"/>
        [Obsolete("This method is obsolete and will be removed in a future version. Please use AddMiddleware instead.")]
        public static CommandLineBuilder UseMiddleware(
            this CommandLineBuilder builder,
            Action<InvocationContext> onInvoke,
            MiddlewareOrder order = MiddlewareOrder.Default)
        {
            builder.AddMiddleware(async (context, next) =>
            {
                onInvoke(context);
                await next(context);
            }, order);

            return builder;
        }

        /// <summary>
        /// Enables the use of the <c>[parse]</c> directive, which when specified on the command line will short circuit normal command handling and display a diagram explaining the parse result for the command line input.
        /// </summary>
        /// <param name="builder">A command line builder.</param>
        /// <param name="errorExitCode">If the parse result contains errors, this exit code will be used when the process exits.</param>
        /// <returns>The same instance of <see cref="CommandLineBuilder"/>.</returns>
        public static CommandLineBuilder UseParseDirective(
            this CommandLineBuilder builder,
            int? errorExitCode = null)
        {
            builder.AddMiddleware(async (context, next) =>
            {
                if (context.ParseResult.Directives.Contains("parse"))
                {
                    context.InvocationResult = new ParseDirectiveResult(errorExitCode);
                }
                else
                {
                    await next(context);
                }
            }, MiddlewareOrderInternal.ParseDirective);

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
            int? errorExitCode = null)
        {
            builder.AddMiddleware(async (context, next) =>
            {
                if (context.ParseResult.Errors.Count > 0)
                {
                    context.InvocationResult = new ParseErrorResult(errorExitCode);
                }
                else
                {
                    await next(context);
                }
            }, MiddlewareOrderInternal.ParseErrorReporting);
            return builder;
        }

        /// <summary>
        /// Enables the use of the <c>[suggest]</c> directive which when specified in command line input short circuits normal command handling and writes a newline-delimited list of suggestions suitable for use by most shells to provide command line completions.
        /// </summary>
        /// <remarks>The <c>dotnet-suggest</c> tool requires the suggest directive to be enabled for an application to provide completions.</remarks>
        /// <param name="builder">A command line builder.</param>
        /// <returns>The same instance of <see cref="CommandLineBuilder"/>.</returns>
        public static CommandLineBuilder UseSuggestDirective(
            this CommandLineBuilder builder)
        {
            builder.AddMiddleware(async (context, next) =>
            {
                if (context.ParseResult.Directives.TryGetValues("suggest", out var values))
                {
                    int position;

                    if (values.FirstOrDefault() is { } positionString)
                    {
                        position = int.Parse(positionString);
                    }
                    else
                    {
                        position = context.ParseResult.RawInput?.Length ?? 0;
                    }

                    context.InvocationResult = new SuggestDirectiveResult(position);
                }
                else
                {
                    await next(context);
                }
            }, MiddlewareOrderInternal.SuggestDirective);

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
            builder.AddMiddleware(async (context, next) =>
            {
                if (context.ParseResult.CommandResult.Command.TreatUnmatchedTokensAsErrors &&
                    context.ParseResult.UnmatchedTokens.Count > 0)
                {
                    var typoCorrection = new TypoCorrection(maxLevenshteinDistance);

                    typoCorrection.ProvideSuggestions(context.ParseResult, context.Console);
                }
                await next(context);
            }, MiddlewareOrderInternal.TypoCorrection);

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
            Resources validationMessages)
        {
            builder.Resources = validationMessages;
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

            var versionOption = new VersionOption();

            builder.VersionOption = versionOption;
            builder.Command.AddOption(versionOption);

            builder.AddMiddleware(async (context, next) =>
            {
                if (context.ParseResult.FindResultFor(versionOption) is { })
                {
                    if (context.ParseResult.Errors.Any(e => e.SymbolResult?.Symbol is VersionOption))
                    {
                        context.InvocationResult = new ParseErrorResult(null);
                    }
                    else
                    {
                        context.Console.Out.WriteLine(_assemblyVersion.Value);
                    }
                }
                else
                {
                    await next(context);
                }
            }, MiddlewareOrderInternal.VersionOption);

            return builder;
        }

        /// <inheritdoc cref="UseVersionOption(System.CommandLine.Builder.CommandLineBuilder)"/>
        /// <param name="aliases">One or more aliases to use instead of the default to signal that version information should be displayed.</param>
        /// <param name="builder">A command line builder.</param>
        public static CommandLineBuilder UseVersionOption(
            this CommandLineBuilder builder,
            params string[] aliases)
        {
            var command = builder.Command;

            if (builder.VersionOption is not null)
            {
                return builder;
            }

            var versionOption = new VersionOption(aliases);

            builder.VersionOption = versionOption;
            command.AddOption(versionOption);

            builder.AddMiddleware(async (context, next) =>
            {
                if (context.ParseResult.FindResultFor(versionOption) is { })
                {
                    if (context.ParseResult.Errors.Any(e => e.SymbolResult?.Symbol is VersionOption))
                    {
                        context.InvocationResult = new ParseErrorResult(null);
                    }
                    else
                    {
                        context.Console.Out.WriteLine(_assemblyVersion.Value);
                    }
                }
                else
                {
                    await next(context);
                }
            }, MiddlewareOrderInternal.VersionOption);

            return builder;
        }

        private static bool ShowHelp(
            InvocationContext context,
            IOption helpOption)
        {
            if (context.ParseResult.FindResultFor(helpOption) is { })
            {
                context.InvocationResult = new HelpResult();
                return true;
            }

            return false;
        }
    }
}
