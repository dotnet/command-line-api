// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Invocation;
using System.Reflection;

namespace System.CommandLine.Builder
{
    public static class CommandLineBuilderExtensions
    {
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

        private static readonly Lazy<string> _assemblyVersion =
            new Lazy<string>(() => {
                var assembly = Assembly.GetEntryAssembly();
                var assemblyVersionAttribute = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
                if (assemblyVersionAttribute == null)
                {
                    return assembly.GetName().Version.ToString();
                }
                else
                {
                    return assemblyVersionAttribute.InformationalVersion;
                }
            });

        public static CommandLineBuilder AddVersionOption(
            this CommandLineBuilder builder)
        {
            var versionOption = new Option("--version", "Display version information");

            builder.AddOption(versionOption);

            builder.AddMiddleware(async (context, next) =>
            {
                if (context.ParseResult.HasOption(versionOption))
                {
                    context.Console.Out.WriteLine(_assemblyVersion.Value);
                }
                else
                {
                    await next(context);
                }
            }, CommandLineBuilder.MiddlewareOrder.Preprocessing);

            return builder;
        }

        public static TBuilder ConfigureFromMethod<TBuilder>(
            this TBuilder builder,
            MethodInfo method,
            object target = null)
            where TBuilder : CommandBuilder
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (method == null)
            {
                throw new ArgumentNullException(nameof(method));
            }

            builder.Command.ConfigureFromMethod(method, target);

            return builder;
        }

        public static CommandLineBuilder ConfigureFromType<T>(
            this CommandLineBuilder builder,
            MethodInfo onExecuteMethod = null)
            where T : class
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            var typeBinder = new TypeBinder(typeof(T));

            foreach (var option in typeBinder.BuildOptions())
            {
                builder.AddOption(option);
            }

            builder.Handler = new TypeBindingCommandHandler(
                onExecuteMethod,
                typeBinder);

            return builder;
        }

        public static CommandLineBuilder EnablePositionalOptions(
            this CommandLineBuilder builder,
            bool value = true)
        {
            builder.EnablePositionalOptions = value;
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
        
        public static CommandLineBuilder UseDefaults(this CommandLineBuilder builder)
        {
            return builder
                   .AddVersionOption()
                   .UseHelp()
                   .UseParseDirective()
                   .UseDebugDirective()
                   .UseSuggestDirective()
                   .RegisterWithDotnetSuggest()
                   .UseTypoCorrections()
                   .UseParseErrorReporting()
                   .UseExceptionHandler()
                   .CancelOnProcessTermination();
        }

        public static TBuilder UsePrefixes<TBuilder>(this TBuilder builder, IReadOnlyCollection<string> prefixes)
            where TBuilder : CommandLineBuilder
        {
            builder.Prefixes = prefixes;
            return builder;
        }

        public static TBuilder UseHelpBuilderFactory<TBuilder>(this TBuilder builder, IHelpBuilderFactory helpBuilderFactory)
            where TBuilder : CommandLineBuilder
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }
            builder.HelpBuilderFactory = helpBuilderFactory;
            return builder;
        }
    }
}
