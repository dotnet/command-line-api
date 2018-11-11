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

        public static TBuilder AddCommand<TBuilder>(
            this TBuilder builder,
            string name,
            string description,
            Action<CommandBuilder> symbols = null,
            Action<ArgumentBuilder> arguments = null,
            IHelpBuilder helpBuilder = null)
            where TBuilder : CommandBuilder
        {
            var commandBuilder = new CommandBuilder(name, builder)
                                 {
                                     Description = description,
                                     HelpBuilder = helpBuilder ?? builder.HelpBuilder
                                 };

            if (symbols != null)
            {
                symbols.Invoke(commandBuilder);
            }

            if (arguments != null)
            {
                arguments.Invoke(commandBuilder.Arguments);
            } 

            var command = commandBuilder.BuildCommand();
            
            builder.AddCommand(command);

            return builder;
        }

        public static TBuilder AddCommand<TBuilder>(
            this TBuilder builder,
            string name,
            string description = null,
            Argument argument = null,
            IHelpBuilder helpBuilder = null)
            where TBuilder : CommandBuilder
        {
            var command = new Command(name, helpBuilder: helpBuilder ?? builder.HelpBuilder)
                          {
                              Description = description
                          };

            if (argument != null)
            {
                command.Argument = argument;
            }

            builder.AddCommand(command);

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

        public static TBuilder AddOptionFromParameter<TBuilder>(
            this TBuilder builder,
            ParameterInfo parameter)
            where TBuilder : CommandBuilder
        {
            var argument = new Argument
                           {
                               ArgumentType = parameter.ParameterType
                           };

            if (parameter.HasDefaultValue)
            {
                argument.SetDefaultValue(() => parameter.DefaultValue);
            }

            var option = new Option(
                parameter.BuildAlias(),
                parameter.Name,
                argument);

            builder.AddOption(option);

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

        public static TBuilder AddOption<TBuilder>(
            this TBuilder builder,
            string alias)
            where TBuilder : CommandBuilder
        {
            builder.AddOption(new Option(alias));

            return builder;
        }

        public static TBuilder AddOption<TBuilder>(
            this TBuilder builder,
            string[] aliases,
            string description = null,
            Argument argument = null)
            where TBuilder : CommandBuilder
        {
            var option = new Option(aliases)
                         {
                             Description = description
                         };

            if (argument != null)
            {
                option.Argument = argument;
            }

            builder.AddOption(option);

            return builder;
        }

        public static TBuilder AddOption<TBuilder>(
            this TBuilder builder,
            string[] aliases,
            string description = null,
            IArgumentArity arity = null)
            where TBuilder : CommandBuilder
        {
            var option = new Option(aliases)
                         {
                             Description = description
                         };

            if (arity != null)
            {
                option.Argument = new Argument
                                  {
                                      Arity = arity
                                  };
            }

            builder.AddOption(option);

            return builder;
        }

        public static TBuilder AddOption<TBuilder>(
            this TBuilder builder,
            string name,
            string description = null,
            Argument argument = null)
            where TBuilder : CommandBuilder
        {
            return builder.AddOption(new[] { name }, description, argument);
        }

        public static TBuilder AddOption<TBuilder>(
            this TBuilder builder,
            string name,
            string description = null,
            IArgumentArity arity = null)
            where TBuilder : CommandBuilder
        {
            return builder.AddOption(new[] { name }, description, arity);
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
    }
}
