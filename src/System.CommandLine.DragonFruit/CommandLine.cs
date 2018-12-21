// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.IO;
using System.Linq;
using System.Reflection;
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
        /// <returns>The exit code.</returns>
        public static Task<int> ExecuteAssemblyAsync(Assembly entryAssembly, string[] args)
            => ExecuteAssemblyAsync(entryAssembly, args, null);

        internal static async Task<int> ExecuteAssemblyAsync(
            Assembly entryAssembly,
            string[] args,
            IConsole console)
        {
            if (entryAssembly == null)
            {
                throw new ArgumentNullException(nameof(entryAssembly));
            }

            args = args ?? Array.Empty<string>();

            MethodInfo entryMethod = EntryPointDiscoverer.FindStaticEntryMethod(entryAssembly);

            return await InvokeMethodAsync(
                       args,
                       console,
                       entryMethod,
                       null /* this is a static method*/);
        }

        public static async Task<int> InvokeMethodAsync(
            string[] args,
            IConsole console,
            MethodInfo method,
            object @object)
        {
            var builder = new CommandLineBuilder()
                          .ConfigureFromMethod(method, @object)
                          .ConfigureHelpFromXmlComments(method)
                          .UseDefaults();

            Parser parser = builder.Build();

            return await parser.InvokeAsync(args, console);
        }

        public static CommandLineBuilder ConfigureHelpFromXmlComments(
            this CommandLineBuilder builder,
            MethodInfo method)
        {
            Assembly assembly = method.DeclaringType.Assembly;
            string docFilePath = Path.Combine(
                Path.GetDirectoryName(assembly.Location),
                Path.GetFileNameWithoutExtension(assembly.Location) + ".xml");

            var metadata = new CommandHelpMetadata();
            if (XmlDocReader.TryLoad(docFilePath, out var xmlDocs))
            {
                if (xmlDocs.TryGetMethodDescription(method, out metadata) &&
                    metadata.Description != null)
                {
                    builder.Command.Description = metadata.Description;
                    var options = builder.Options;

                    foreach (var parameterDescription in metadata.ParameterDescriptions)
                    {
                        ConfigureOption(parameterDescription, options);
                    }

                    metadata.Name = assembly.GetName().Name;
                }
            }

            return builder;
        }

        private static void ConfigureOption(ParameterMetadata parameterDescription, IEnumerable<Option> options)
        {
            var parameterName = BuildAlias(parameterDescription.Name);

            var option = options.FirstOrDefault(o => o.HasAlias(parameterName));

            if (option != null)
            {
                option.Description = parameterDescription.Description;

                if (!string.IsNullOrWhiteSpace(parameterDescription.Alias))
                {
                    var aliasName = BuildAlias(parameterDescription.Alias);
                    if (!option.HasAlias(aliasName))
                    {
                        option.AddAlias(aliasName);
                    }
                }
            }
        }

        private static string BuildAlias(string parameterName)
        {
            if (string.IsNullOrWhiteSpace(parameterName))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(parameterName));
            }

            return parameterName.Length > 1
                ? $"--{parameterName.ToKebabCase()}"
                : $"-{parameterName.ToLowerInvariant()}";
        }
    }
}
