// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Builder;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace System.CommandLine.DragonFruit
{
    public class CommandLine
    {
        internal const int ErrorExitCode = 1;
        internal const int OkExitCode = 0;

        public static Task<int> ExecuteAssemblyAsync(Assembly assembly, string[] args)
            => ExecuteAssemblyAsync(assembly, args, PhysicalConsole.Instance);

        internal static async Task<int> ExecuteAssemblyAsync(
            Assembly assembly,
            string[] args,
            IConsole console)
        {
            if (assembly == null)
            {
                throw new ArgumentNullException(nameof(assembly));
            }

            if (console == null)
            {
                throw new ArgumentNullException(nameof(console));
            }

            if (console == null)
            {
                throw new ArgumentNullException(nameof(console));
            }

            MethodInfo entryMethod = EntryPointCreator.FindStaticEntryMethod(assembly);

            var docFilePath = Path.Combine(
                Path.GetDirectoryName(assembly.Location),
                Path.GetFileNameWithoutExtension(assembly.Location) + ".xml");

            CommandHelpMetadata commandHelpMetadata = new CommandHelpMetadata();
            if (XmlDocReader.TryLoad(docFilePath, out var xmlDocs))
            {
                xmlDocs.TryGetMethodDescription(entryMethod, out commandHelpMetadata);
            }

            commandHelpMetadata.Name = assembly.GetName().Name;

            return await InvokeMethodAsync(
                args,
                console,
                /* @object:*/ null, // this is a static method
                entryMethod,
                commandHelpMetadata);
        }

        internal static async Task<int> InvokeMethodAsync(
            string[] args,
            IConsole console,
            object @object,
            MethodInfo method,
            CommandHelpMetadata helpMetadata)
        {
            if (helpMetadata == null)
            {
                throw new ArgumentNullException(nameof(helpMetadata));
            }

            var helpOption = new OptionDefinition("--help", "Show help output");

            var command = new MethodCommandFactory()
                .Create(
                    method,
                    helpMetadata,
                    additionalOptions: new[] {helpOption});

            var parser = new Parser(new ParserConfiguration(new[] {command.Definition}));

            var result = parser.Parse(args);

            if (result.Errors.Count > 0)
            {
                return HandleParserErrors(result, console);
            }

            if (result.HasOption(helpOption))
            {
                console.Out.WriteLine(command.Definition.HelpView());
                return OkExitCode;
            }

            try
            {
                return await command.InvokeMethodAsync(@object, result);
            }
            catch (Exception e)
            {
                console.ForegroundColor = ConsoleColor.Red;
                console.Error.Write("Unhandled exception: ");
                console.Error.WriteLine(e.ToString());
                console.ResetColor();
                return ErrorExitCode;
            }
        }

        private static int HandleParserErrors(ParseResult result, IConsole console)
        {
            console.ForegroundColor = ConsoleColor.Red;
            foreach (var parseError in result.Errors)
            {
                console.Error.WriteLine(parseError.Message);
            }

            console.ResetColor();


            console.Error.WriteLine("Use --help to see available commands and options.");

            return ErrorExitCode;
        }
    }
}
