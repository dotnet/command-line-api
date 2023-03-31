// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Binding;
using System.CommandLine.NamingConventionBinder;
using System.CommandLine.Utility;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using CommandHandler = System.CommandLine.NamingConventionBinder.CommandHandler;

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
        /// <returns>The exit code.</returns>
        public static Task<int> ExecuteAssemblyAsync(
            Assembly entryAssembly,
            string[] args,
            string entryPointFullTypeName,
            string xmlDocsFilePath = null)
        {
            if (entryAssembly == null)
            {
                throw new ArgumentNullException(nameof(entryAssembly));
            }

            args = args ?? Array.Empty<string>();
            entryPointFullTypeName = entryPointFullTypeName?.Trim();

            MethodInfo entryMethod = EntryPointDiscoverer.FindStaticEntryMethod(entryAssembly, entryPointFullTypeName);

            //TODO The xml docs file name and location can be customized using <DocumentationFile> project property.
            return InvokeMethodAsync(args, entryMethod, xmlDocsFilePath, null);
        }

        /// <summary>
        /// Finds and executes 'Program.Main', but with strong types.
        /// </summary>
        /// <param name="entryAssembly">The entry assembly</param>
        /// <param name="args">The string arguments.</param>
        /// <param name="entryPointFullTypeName">Explicitly defined entry point</param>
        /// <param name="xmlDocsFilePath">Explicitly defined path to xml file containing XML Docs</param>
        /// <returns>The exit code.</returns>
        public static int ExecuteAssembly(
            Assembly entryAssembly,
            string[] args,
            string entryPointFullTypeName,
            string xmlDocsFilePath = null)
        {
            if (entryAssembly == null)
            {
                throw new ArgumentNullException(nameof(entryAssembly));
            }

            args = args ?? Array.Empty<string>();
            entryPointFullTypeName = entryPointFullTypeName?.Trim();

            MethodInfo entryMethod = EntryPointDiscoverer.FindStaticEntryMethod(entryAssembly, entryPointFullTypeName);

            //TODO The xml docs file name and location can be customized using <DocumentationFile> project property.
            return InvokeMethod(args, entryMethod, xmlDocsFilePath, null);
        }

        public static Task<int> InvokeMethodAsync(
            string[] args,
            MethodInfo method,
            string xmlDocsFilePath = null,
            object target = null,
            TextWriter standardOutput = null,
            TextWriter standardError = null)
        {
            CliConfiguration configuration = BuildConfiguration(method, xmlDocsFilePath, target);
            configuration.Output = standardOutput ?? Console.Out;
            configuration.Error = standardError ?? Console.Error;

            return configuration.Parse(args).InvokeAsync();
        }

        public static int InvokeMethod(
            string[] args,
            MethodInfo method,
            string xmlDocsFilePath = null,
            object target = null,
            TextWriter standardOutput = null,
            TextWriter standardError = null)
        {
            CliConfiguration configuration = BuildConfiguration(method, xmlDocsFilePath, target);
            configuration.Output = standardOutput ?? Console.Out;
            configuration.Error = standardError ?? Console.Error;

            return configuration.Parse(args).Invoke();
        }

        private static CliConfiguration BuildConfiguration(MethodInfo method,
            string xmlDocsFilePath,
            object target)
        {
            return new CliConfiguration(new CliRootCommand())
                .ConfigureRootCommandFromMethod(method, target)
                .ConfigureHelpFromXmlComments(method, xmlDocsFilePath);
        }

        public static CliConfiguration ConfigureRootCommandFromMethod(
            this CliConfiguration builder,
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

            builder.RootCommand.ConfigureFromMethod(method, target);

            return builder;
        }

        private static readonly string[] _argumentParameterNames =
        {
            "arguments",
            "argument",
            "args"
        };

        public static void ConfigureFromMethod(
            this CliCommand command,
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
                command.Options.Add(option);
            }

            if (method.GetParameters().FirstOrDefault(p => _argumentParameterNames.Contains(p.Name)) is { } argsParam)
            {
                command.Arguments.Add(ArgumentBuilder.CreateArgument(argsParam));
            }

            command.Action = CommandHandler.Create(method, target);
        }

        public static CliConfiguration ConfigureHelpFromXmlComments(
            this CliConfiguration builder,
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
                    builder.RootCommand.Description = metadata.Description;

                    foreach (var parameterDescription in metadata.ParameterDescriptions)
                    {
                        var kebabCasedParameterName = parameterDescription.Key.ToKebabCase();

                        var option = builder.RootCommand.Options.FirstOrDefault(o => HasAliasIgnoringPrefix(o, kebabCasedParameterName));

                        if (option != null)
                        {
                            option.Description = parameterDescription.Value;
                        }
                        else
                        {
                            for (var i = 0; i < builder.RootCommand.Arguments.Count; i++)
                            {
                                var argument = builder.RootCommand.Arguments[i];
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

        public static IEnumerable<CliOption> BuildOptions(this MethodInfo method)
        {
            var descriptor = HandlerDescriptor.FromMethodInfo(method);

            var omittedTypes = new[]
                               {
                                   typeof(BindingContext),
                                   typeof(ParseResult),
                                   typeof(CliConfiguration),
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

        public static CliOption BuildOption(this ParameterDescriptor parameter)
            => OptionBuilder.CreateOption(
                parameter.BuildAlias(),
                parameter.ValueType,
                parameter.ValueName,
                parameter.HasDefaultValue ? parameter.GetDefaultValue : null);

        private static string GetDefaultXmlDocsFileLocation(Assembly assembly)
        {
            if (!string.IsNullOrEmpty(assembly.Location))
            {
                return Path.Combine(
                    Path.GetDirectoryName(assembly.Location),
                    Path.GetFileNameWithoutExtension(assembly.Location) + ".xml");
            }

            // Assembly.Location is empty for bundled (i.e, single-file) assemblies, but we can't be confident
            // that whenever Assembly.Location is empty the corresponding assembly is bundled.
            //
            // Provisionally assume that the entry-assembly is bundled. If this query is for something other
            // than the entry-assembly, then return nothing. 
            if (assembly == Assembly.GetEntryAssembly())
            {
                return Path.Combine(
                    AppContext.BaseDirectory,
                    assembly.GetName().Name + ".xml");
            }

            return string.Empty;
        }

        /// <summary>
        /// Indicates whether a given alias exists on the option, regardless of its prefix.
        /// </summary>
        /// <param name="alias">The alias, which can include a prefix.</param>
        /// <returns><see langword="true"/> if the alias exists; otherwise, <see langword="false"/>.</returns>
        private static bool HasAliasIgnoringPrefix(CliOption option, string alias)
        {
            ReadOnlySpan<char> rawAlias = alias.AsSpan(GetPrefixLength(alias));

            if (MemoryExtensions.Equals(option.Name.AsSpan(GetPrefixLength(option.Name)), rawAlias, StringComparison.CurrentCulture))
            {
                return true;
            }

            foreach (string existingAlias in option.Aliases)
            {
                if (MemoryExtensions.Equals(existingAlias.AsSpan(GetPrefixLength(existingAlias)), rawAlias, StringComparison.CurrentCulture))
                {
                    return true;
                }
            }

            return false;

            static int GetPrefixLength(string alias)
            {
                if (alias[0] == '-')
                {
                    return alias.Length > 1 && alias[1] == '-' ? 2 : 1;
                }
                else if (alias[0] == '/')
                {
                    return 1;
                }

                return 0;
            }
        }
    }
}
