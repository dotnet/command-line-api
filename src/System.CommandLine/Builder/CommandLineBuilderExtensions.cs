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
            string name,
            string description = null,
            Action<CommandBuilder> symbols = null,
            Action<ArgumentBuilder> arguments = null,
            IHelpBuilder helpBuilder = null)
            where TBuilder : CommandBuilder
        {
            var commandBuilder = new CommandBuilder(name, builder)
                                 {
                                     Description = description,
                                     HelpBuilder = helpBuilder ?? builder.HelpBuilder,
                                 };

            symbols?.Invoke(commandBuilder);

            arguments?.Invoke(commandBuilder.Arguments);

            builder.Commands.Add(commandBuilder);

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

            foreach (var parameter in method.GetParameters())
            {
                builder.AddOptionFromParameter(parameter);
            }

            builder.OnExecute(method, target);

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

        public static TBuilder AddOptionFromProperty<TBuilder>(
            this TBuilder builder,
            PropertyInfo property)
            where TBuilder : CommandBuilder
        {
            if (property.CanWrite)
            {
                builder.AddOption(
                    property.BuildAlias(),
                    property.Name,
                    args => args.ParseArgumentsAs(property.PropertyType));
            }

            return builder;
        }

        public static TBuilder AddOptionFromParameter<TBuilder>(
            this TBuilder builder,
            ParameterInfo parameter)
            where TBuilder : CommandBuilder
        {
            builder.AddOption(
                parameter.BuildAlias(),
                parameter.Name,
                args =>
                {
                    args.ParseArgumentsAs(parameter.ParameterType);

                    if (parameter.HasDefaultValue)
                    {
                        args.WithDefaultValue(() => parameter.DefaultValue);
                    }
                });

            return builder;
        }

        public static TBuilder AddOption<TBuilder>(
            this TBuilder builder,
            string[] aliases,
            string description = null,
            Action<ArgumentBuilder> arguments = null)
            where TBuilder : CommandBuilder
        {
            var optionBuilder = new OptionBuilder(aliases, builder)
                                {
                                    Description = description,
                                };

            arguments?.Invoke(optionBuilder.Arguments);

            builder.Options.Add(optionBuilder);

            return builder;
        }

        public static TBuilder AddOption<TBuilder>(
            this TBuilder builder,
            string name,
            string description = null,
            Action<ArgumentBuilder> arguments = null)
            where TBuilder : CommandBuilder
        {
            return builder.AddOption(new[] { name }, description, arguments);
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

        public static CommandLineBuilder TreatUnmatchedTokensAsErrors(
            this CommandLineBuilder builder,
            bool value = true)
        {
            builder.TreatUnmatchedTokensAsErrors = value;
            return builder;
        }

        public static TBuilder AddArguments<TBuilder>(
            this TBuilder builder,
            Action<ArgumentBuilder> action)
            where TBuilder : CommandBuilder
        {
            action.Invoke(builder.Arguments);
            return builder;
        }

        public static CommandLineBuilder ParseResponseFileAs(
            this CommandLineBuilder builder,
            ResponseFileHandling responseFileHandling)
        {
            builder.ResponseFileHandling = responseFileHandling;
            return builder;
        }

        public static TBuilder UsePrefixes<TBuilder>(this TBuilder builder, IReadOnlyCollection<string> prefixes)
            where TBuilder : CommandLineBuilder
        {
            builder.Prefixes = prefixes;
            return builder;
        }

        private static readonly Lazy<string> _assemblyVersion =
            new Lazy<string>(() =>
                                 Assembly.GetEntryAssembly()
                                         .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                                         .InformationalVersion);

        public static CommandLineBuilder AddVersionOption(
            this CommandLineBuilder builder)
        {
            builder.AddOption("--version", "Display version information");

            builder.AddMiddleware(async (context, next) =>
            {
                if (context.ParseResult.HasOption("version"))
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
    }
}
