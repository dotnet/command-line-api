// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace System.CommandLine.DragonFruit
{
    public class CommandLine
    {
        internal const int ErrorExitCode = 1;
        internal const int OkExitCode = 0;

        /// <summary>
        /// Finds and executes 'Program.Main', but with strong types.
        /// </summary>
        /// <param name="entryAssembly">The entry assembly</param>
        /// <param name="args"></param>
        /// <returns></returns>
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

            if (console == null)
            {
                throw new ArgumentNullException(nameof(console));
            }

            MethodInfo entryMethod = EntryPointCreator.FindStaticEntryMethod(entryAssembly);

            string docFilePath = Path.Combine(
                Path.GetDirectoryName(entryAssembly.Location),
                Path.GetFileNameWithoutExtension(entryAssembly.Location) + ".xml");

            var commandHelpMetadata = new CommandHelpMetadata();
            if (XmlDocReader.TryLoad(docFilePath, out XmlDocReader xmlDocs))
            {
                xmlDocs.TryGetMethodDescription(entryMethod, out commandHelpMetadata);
            }

            commandHelpMetadata.Name = entryAssembly.GetName().Name;

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

            MethodCommand command = new MethodCommandFactory()
                .Create(
                    method,
                    helpMetadata,
                    new[] { helpOption });

            var parser = new Parser(new ParserConfiguration(new[] { command.Definition }));

            ParseResult result = parser.Parse(args);

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
                return await command.InvokeAsync(@object, result);
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
            foreach (ParseError parseError in result.Errors)
            {
                console.Error.WriteLine(parseError.Message);
            }

            console.ResetColor();


            console.Error.WriteLine("Use --help to see available commands and options.");

            return ErrorExitCode;
        }
    }
}
