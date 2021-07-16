// Copyright (c) .NET Foundation and contributors. All rights reserved.
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
            new Lazy<string>(() =>
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

        public static TBuilder AddArgument<TBuilder>(
            this TBuilder builder,
            Argument argument)
            where TBuilder : CommandBuilder
        {
            builder.AddArgument(argument);

            return builder;
        }

        public static TBuilder AddCommand<TBuilder>(
            this TBuilder builder,
            Command command)
            where TBuilder : CommandBuilder
        {
            builder.AddCommand(command);

            return builder;
        }

        public static TBuilder AddOption<TBuilder>(
            this TBuilder builder,
            Option option)
            where TBuilder : CommandBuilder
        {
            builder.AddOption(option);

            return builder;
        }

        public static TBuilder AddGlobalOption<TBuilder>(
            this TBuilder builder,
            Option option)
            where TBuilder : CommandBuilder
        {
            builder.AddGlobalOption(option);

            return builder;
        }

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
                    processExitHandler = (_1, _2) =>
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

        public static CommandLineBuilder EnableDirectives(
            this CommandLineBuilder builder,
            bool value = true)
        {
            builder.EnableDirectives = value;
            return builder;
        }

        public static CommandLineBuilder EnablePosixBundling(
            this CommandLineBuilder builder,
            bool value = true)
        {
            builder.EnablePosixBundling = value;
            return builder;
        }

        public static CommandLineBuilder ParseResponseFileAs(
            this CommandLineBuilder builder,
            ResponseFileHandling responseFileHandling)
        {
            builder.ResponseFileHandling = responseFileHandling;
            return builder;
        }

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
{3}", dotnetSuggestProcess.StartInfo.FileName, dotnetSuggestProcess.ExitCode, stdOut.ToString(), stdErr.ToString());
                    }
                    catch (Exception exception)
                    {
                        return string.Format(@"Exception during registration:
{0}", exception);
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

        public static CommandLineBuilder UseDebugDirective(
            this CommandLineBuilder builder,
            int? errorExitCode = null)
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
                        context.ExitCode = errorExitCode ?? 1;
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
                            context.ExitCode = errorExitCode ?? 1;
                            return;
                        }
                    }
                }

                await next(context);
            }, MiddlewareOrderInternal.DebugDirective);

            return builder;
        }

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

        public static CommandLineBuilder UseHelp(this CommandLineBuilder builder)
        {
            return builder.UseHelp(new HelpOption());
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

        public static CommandLineBuilder UseHelp<THelpBuilder>(
            this CommandLineBuilder builder,
            Action<THelpBuilder>? configureHelp)
            where THelpBuilder : IHelpBuilder
        {
            return builder.UseHelp(new HelpOption(), configureHelp);
        }

        internal static CommandLineBuilder UseHelp<THelpBuilder>(
            this CommandLineBuilder builder,
            HelpOption helpOption,
            Action<THelpBuilder>? configureHelp)
            where THelpBuilder : IHelpBuilder
        {
            if (configureHelp is { })
            {
                builder.ConfigureHelp = helpBuilder => configureHelp((THelpBuilder)helpBuilder);
            }
            else
            {
                builder.ConfigureHelp = null;
            }
            return builder.UseHelp(helpOption);
        }

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

        public static CommandLineBuilder UseTypoCorrections(
            this CommandLineBuilder builder, int maxLevenshteinDistance = 3)
        {
            builder.AddMiddleware(async (context, next) =>
            {
                if (context.ParseResult.UnmatchedTokens.Count > 0 &&
                    context.ParseResult.CommandResult.Command.TreatUnmatchedTokensAsErrors)
                {
                    var typoCorrection = new TypoCorrection(maxLevenshteinDistance);

                    typoCorrection.ProvideSuggestions(context.ParseResult, context.Console);
                }
                await next(context);
            }, MiddlewareOrderInternal.TypoCorrection);

            return builder;
        }

        public static CommandLineBuilder UseResources(
            this CommandLineBuilder builder,
            Resources validationMessages)
        {
            builder.Resources = validationMessages;
            return builder;
        }

        public static CommandLineBuilder UseVersionOption(
            this CommandLineBuilder builder,
            int? errorExitCode = null)
        {
            var command = builder.Command;

            if (builder.VersionOption is not null)
            {
                return builder;
            }
            
            var versionOption = new Option<bool>(
                "--version",
                parseArgument: result =>
                {
                    var commandChildren = result.FindResultFor(command)?.Children;
                    if (commandChildren is null)
                    {
                        return true;
                    }

                    var versionOptionResult = result.Parent;
                    for (int i = 0; i < commandChildren.Count; i++)
                    {
                        var symbolResult = commandChildren[i];
                        if (symbolResult == versionOptionResult)
                        {
                            continue;
                        }
                        
                        if (IsNotImplicit(symbolResult))
                        {
                            result.ErrorMessage = result.Resources.VersionOptionCannotBeCombinedWithOtherArguments("--version");
                            return false;
                        }
                    }

                    return true;
                });

            versionOption.DisallowBinding = true;

            builder.VersionOption = versionOption;
            command.AddOption(versionOption);

            builder.AddMiddleware(async (context, next) =>
            {
                if (context.ParseResult.FindResultFor(versionOption) is { } result)
                {
                    if (result.ArgumentConversionResult.ErrorMessage is { })
                    {
                        context.InvocationResult = new ParseErrorResult(errorExitCode);
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

            static bool IsNotImplicit(SymbolResult symbolResult)
            {
                return symbolResult switch
                {
                    ArgumentResult argumentResult => !argumentResult.IsImplicit,
                    OptionResult optionResult => !optionResult.IsImplicit,
                    _ => true
                };
            }
        }

        private static bool ShowHelp(
            InvocationContext context,
            IOption helpOption)
        {
            if (context.ParseResult.FindResultFor(helpOption) != null)
            {
                context.InvocationResult = new HelpResult();
                return true;
            }

            return false;
        }
    }
}
