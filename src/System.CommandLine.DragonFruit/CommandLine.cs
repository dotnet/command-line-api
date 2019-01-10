// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.CommandLine.Rendering;
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
        public static async Task<int> ExecuteAssemblyAsync(
            Assembly entryAssembly, 
            string[] args)
        {
            if (entryAssembly == null)
            {
                throw new ArgumentNullException(nameof(entryAssembly));
            }

            args = args ?? Array.Empty<string>();

            MethodInfo entryMethod = EntryPointDiscoverer.FindStaticEntryMethod(entryAssembly);

            return await InvokeMethodAsync(
                       args,
                       entryMethod);
        }

        public static async Task<int> InvokeMethodAsync(
            string[] args,
            MethodInfo method,
            object target = null,
            IConsole console = null)
        {
            var builder = new CommandLineBuilder()
                          .ConfigureFromMethod(method, target)
                          .ConfigureHelpFromXmlComments(method)
                          .UseDefaults()
                          .UseAnsiTerminalWhenAvailable();

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
                        var kebabCasedParameterName = parameterDescription.Key.ToKebabCase();

                        var option = options.FirstOrDefault(o => o.HasAlias(kebabCasedParameterName));

                        if (option != null)
                        {
                            option.Description = parameterDescription.Value;
                        }
                    }

                    metadata.Name = assembly.GetName().Name;
                }
            }

            return builder;
        }
    }
}
