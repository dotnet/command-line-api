// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace System.CommandLine.DragonFruit
{
    public class CommandLine
    {
        public const int ErrorExitCode = 1;
        public const int OkExitCode = 0;

        /// <summary>
        /// Finds and executes 'Program.Main', but with strong types.
        /// </summary>
        /// <param name="entryAssembly">The entry assembly</param>
        /// <param name="args">The string arguments.</param>
        /// <returns>The exit code.</returns>
        public static Task<int> ExecuteAssemblyAsync(Assembly entryAssembly, string[] args)
            => ExecuteAssemblyAsync(entryAssembly, args, PhysicalConsole.Instance);

        internal static async Task<int> ExecuteAssemblyAsync(
            Assembly entryAssembly,
            string[] args,
            IConsole console)
        {
            if (entryAssembly == null)
            {
                throw new ArgumentNullException(nameof(entryAssembly));
            }

            if (console == null)
            {
                throw new ArgumentNullException(nameof(console));
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
                          .AddCommandFromMethod(method, @object)
                          .UseParseDirective()
                          .UseHelp()
                          .UseSuggestDirective()
                          .UseParseErrorReporting()
                          .UseExceptionHandler();

            SetHelpMetadata(method, builder);

            Parser parser = builder.Build();

            return await parser.InvokeAsync(args, console);
        }

        private static void SetHelpMetadata(MethodInfo method, CommandDefinitionBuilder builder)
        {
            Assembly assembly = method.DeclaringType.Assembly;
            string docFilePath = Path.Combine(
                Path.GetDirectoryName(assembly.Location),
                Path.GetFileNameWithoutExtension(assembly.Location) + ".xml");

            var metadata = new CommandHelpMetadata();
            if (XmlDocReader.TryLoad(docFilePath, out var xmlDocs))
            {
                xmlDocs.TryGetMethodDescription(method, out metadata);

                if (metadata.Description != null)
                {
                    var options = builder.Options;

                    foreach (var parameterDescription in metadata.ParameterDescriptions)
                    {
                        if (options[parameterDescription.Key] is OptionDefinitionBuilder option)
                        {
                            option.Description = parameterDescription.Value;
                        }
                    }
                }
            }

            metadata.Name = assembly.GetName().Name;
        }
    }
}
