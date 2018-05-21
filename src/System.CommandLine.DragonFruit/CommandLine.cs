// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.IO;
using System.Linq.Expressions;
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
        /// <param name="args">The string arguments.</param>
        /// <returns>The exit code.</returns>
        public static Task<int> ExecuteAssemblyAsync(Assembly entryAssembly, string[] args)
            => ExecuteAssemblyAsync(entryAssembly, args, PhysicalConsole.Instance);

        internal static async Task<int> ExecuteAssemblyAsync(Assembly entryAssembly,
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
                /* @object:*/ null, // this is a static method
                entryMethod);
        }

        public static async Task<int> InvokeMethodAsync(string[] args,
            IConsole console,
            object @object,
            MethodInfo method)
        {
            var helpOption = new OptionDefinition("--help", "Show help output");
            var helpMetadata = GetHelpMetadata(method);

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
                LogUnhandledException(console, e);
                return ErrorExitCode;
            }
        }

        private static CommandHelpMetadata GetHelpMetadata(MethodInfo method)
        {
            Assembly assembly = method.DeclaringType.Assembly;
            string docFilePath = Path.Combine(
                Path.GetDirectoryName(assembly.Location),
                Path.GetFileNameWithoutExtension(assembly.Location) + ".xml");

            var metadata = new CommandHelpMetadata();
            if (XmlDocReader.TryLoad(docFilePath, out var xmlDocs))
            {
                xmlDocs.TryGetMethodDescription(method, out metadata);
            }

            metadata.Name = assembly.GetName().Name;
            return metadata;
        }

        private static void LogUnhandledException(IConsole console, Exception e)
        {
            console.ResetColor();
            console.ForegroundColor = ConsoleColor.Red;
            console.Error.Write("Unhandled exception: ");
            console.Error.WriteLine(e.ToString());
            console.ResetColor();
        }

        private static int HandleParserErrors(ParseResult result, IConsole console)
        {
            console.ResetColor();
            console.ForegroundColor = ConsoleColor.Red;

            foreach (ParseError parseError in result.Errors)
            {
                console.Error.WriteLine(parseError.Message);
            }

            console.ResetColor();
            console.Error.WriteLine("Specify --help to see usage.");
            return ErrorExitCode;
        }
    }
}
