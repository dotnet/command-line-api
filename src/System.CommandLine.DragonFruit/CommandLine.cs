// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Binding;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.CommandLine.Rendering;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace System.CommandLine.DragonFruit
{
    public static class CommandLine
    {
        /// <summary>
        /// Finds and executes 'Program.Main', but with strong types.
        /// </summary>
        /// <param name="entryAssembly">The entry assembly</param>
        /// <param name="args">The string arguments.</param>
        /// <param name="entryPointFullTypeName">Explicitly defined entry point</param>
        /// <param name="xmlDocsFilePath">Explicitly defined path to xml file containing XML Docs</param>
        /// <param name="console">Output console</param>
        /// <returns>The exit code.</returns>
        public static async Task<int> ExecuteAssemblyAsync(
            Assembly entryAssembly,
            string[] args,
            string entryPointFullTypeName,
            string xmlDocsFilePath = null,
            IConsole console = null)
        {
            if (entryAssembly == null)
            {
                throw new ArgumentNullException(nameof(entryAssembly));
            }

            args = args ?? Array.Empty<string>();
            entryPointFullTypeName = entryPointFullTypeName?.Trim();

            MethodInfo entryMethod = EntryPointDiscoverer.FindStaticEntryMethod(entryAssembly, entryPointFullTypeName);

            //TODO The xml docs file name and location can be customized using <DocumentationFile> project property.
            return await InvokeMethodAsync(args, entryMethod, xmlDocsFilePath, null, console);
        }

        /// <summary>
        /// Finds and executes 'Program.Main', but with strong types.
        /// </summary>
        /// <param name="entryAssembly">The entry assembly</param>
        /// <param name="args">The string arguments.</param>
        /// <param name="entryPointFullTypeName">Explicitly defined entry point</param>
        /// <param name="xmlDocsFilePath">Explicitly defined path to xml file containing XML Docs</param>
        /// <param name="console">Output console</param>
        /// <returns>The exit code.</returns>
        public static int ExecuteAssembly(
            Assembly entryAssembly,
            string[] args,
            string entryPointFullTypeName,
            string xmlDocsFilePath = null,
            IConsole console = null)
        {
            if (entryAssembly == null)
            {
                throw new ArgumentNullException(nameof(entryAssembly));
            }

            args = args ?? Array.Empty<string>();
            entryPointFullTypeName = entryPointFullTypeName?.Trim();

            MethodInfo entryMethod = EntryPointDiscoverer.FindStaticEntryMethod(entryAssembly, entryPointFullTypeName);

            //TODO The xml docs file name and location can be customized using <DocumentationFile> project property.
            return InvokeMethod(args, entryMethod, xmlDocsFilePath, null, console);
        }

        public static async Task<int> InvokeMethodAsync(
            string[] args,
            MethodInfo method,
            string xmlDocsFilePath = null,
            object target = null,
            IConsole console = null)
        {
            Parser parser = BuildParser(method, xmlDocsFilePath, target);

            return await parser.InvokeAsync(args, console);
        }

        public static int InvokeMethod(
            string[] args,
            MethodInfo method,
            string xmlDocsFilePath = null,
            object target = null,
            IConsole console = null)
        {
            Parser parser = BuildParser(method, xmlDocsFilePath, target);

            return parser.Invoke(args, console);
        }

        private static Parser BuildParser(MethodInfo method,
            string xmlDocsFilePath,
            object target)
        {
            var builder = new CommandLineBuilder()
                .ConfigureRootCommandFromMethod(method, target)
                .ConfigureHelpFromXmlComments(method, xmlDocsFilePath)
                .UseDefaults()
                .UseAnsiTerminalWhenAvailable();

            return  builder.Build();
        }

        public static CommandLineBuilder ConfigureRootCommandFromMethod(
            this CommandLineBuilder builder,
            MethodInfo method,
            object target = null)
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

        private static readonly string[] _argumentParameterNames =
        {
            "arguments",
            "argument",
            "args"
        };

        public static void ConfigureFromMethod(
            this Command command,
            MethodInfo method,
            object target = null)
        {
            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            if (method == null)
            {
                throw new ArgumentNullException(nameof(method));
            }

            foreach (var option in method.BuildOptions())
            {
                command.AddOption(option);
            }

            if (method.GetParameters()
                      .FirstOrDefault(p => _argumentParameterNames.Contains(p.Name)) is ParameterInfo argsParam)
            {
                var argument = new Argument
                {
                    ArgumentType = argsParam.ParameterType,
                    Name = argsParam.Name
                };

                if (argsParam.HasDefaultValue)
                {
                    if (argsParam.DefaultValue != null)
                    {
                        argument.SetDefaultValue(argsParam.DefaultValue);
                    }
                    else
                    {
                        argument.SetDefaultValueFactory(() => null);
                    }
                }

                command.AddArgument(argument);
            }

            command.Handler = CommandHandler.Create(method, target);
        }

        public static CommandLineBuilder ConfigureHelpFromXmlComments(
            this CommandLineBuilder builder,
            MethodInfo method,
            string xmlDocsFilePath)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (method == null)
            {
                throw new ArgumentNullException(nameof(method));
            }

            if (XmlDocReader.TryLoad(xmlDocsFilePath ?? GetDefaultXmlDocsFileLocation(method.DeclaringType.Assembly), out var xmlDocs))
            {
                if (xmlDocs.TryGetMethodDescription(method, out CommandHelpMetadata metadata) &&
                    metadata.Description != null)
                {
                    builder.Command.Description = metadata.Description;
                    var options = builder.Options.ToArray();

                    foreach (var parameterDescription in metadata.ParameterDescriptions)
                    {
                        var kebabCasedParameterName = parameterDescription.Key.ToKebabCase();

                        var option = options.FirstOrDefault(o => o.HasAlias(kebabCasedParameterName));

                        if (option != null)
                        {
                            option.Description = parameterDescription.Value;
                        }
                        else
                        {
                            foreach (var argument in builder.Command.Arguments)
                            {
                                if (string.Equals(
                                        argument.Name,
                                        kebabCasedParameterName, 
                                        StringComparison.OrdinalIgnoreCase))
                                {
                                    argument.Description = parameterDescription.Value;
                                }
                            }
                        }
                    }

                    metadata.Name = method.DeclaringType.Assembly.GetName().Name;
                }
            }

            return builder;
        }

        public static string BuildAlias(this IValueDescriptor descriptor)
        {
            if (descriptor == null)
            {
                throw new ArgumentNullException(nameof(descriptor));
            }

            return BuildAlias(descriptor.ValueName);
        }

        internal static string BuildAlias(string parameterName)
        {
            if (String.IsNullOrWhiteSpace(parameterName))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(parameterName));
            }

            return parameterName.Length > 1
                       ? $"--{parameterName.ToKebabCase()}"
                       : $"-{parameterName.ToLowerInvariant()}";
        }

        public static IEnumerable<Option> BuildOptions(this MethodInfo method)
        {
            var descriptor = HandlerDescriptor.FromMethodInfo(method);

            var omittedTypes = new[]
                               {
                                   typeof(IConsole),
                                   typeof(InvocationContext),
                                   typeof(BindingContext),
                                   typeof(ParseResult),
                                   typeof(CancellationToken),
                               };

            foreach (var option in descriptor.ParameterDescriptors
                                             .Where(d => !omittedTypes.Contains (d.ValueType))
                                             .Where(d => !_argumentParameterNames.Contains(d.ValueName))
                                             .Select(p => p.BuildOption()))
            {
                yield return option;
            }
        }

        public static Option BuildOption(this ParameterDescriptor parameter)
        {
            var argument = new Argument
                           {
                               ArgumentType = parameter.ValueType
                           };

            if (parameter.HasDefaultValue)
            {
                argument.SetDefaultValueFactory(parameter.GetDefaultValue);
            }

            var option = new Option(
                parameter.BuildAlias(),
                parameter.ValueName)
            {
                Argument = argument
            };

            return option;
        }

        private static string GetDefaultXmlDocsFileLocation(Assembly assembly)
        {
            return Path.Combine(
                Path.GetDirectoryName(assembly.Location),
                Path.GetFileNameWithoutExtension(assembly.Location) + ".xml");
        }
    }
}
