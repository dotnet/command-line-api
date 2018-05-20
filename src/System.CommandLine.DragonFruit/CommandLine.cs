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
        internal const int HelpExitCode = 2;
        internal const int ErrorExitCode = 1;
        internal const int OkExitCode = 0;

        public static Task<int> ExecuteAssemblyAsync(Assembly assembly, string[] args)
            => ExecuteAssemblyAsync(assembly, args, PhysicalConsole.Instance);

        internal static async Task<int> ExecuteAssemblyAsync(Assembly assembly, string[] args, IConsole console)
        {
            MethodInfo entryMethod = EntryPointCreator.FindEntryMethod(assembly);

            var docFilePath = Path.Combine(
                Path.GetDirectoryName(assembly.Location),
                Path.GetFileNameWithoutExtension(assembly.Location) + ".xml");

            MethodDescription methodDescription = null;
            if (XmlDocReader.TryLoad(docFilePath, out var xmlDocs))
            {
                xmlDocs.TryGetMethodDescription(entryMethod, out methodDescription);
            }

            var context = new InvocationContext<int>(entryMethod, console);

            return await ExecuteMethodAsync(args,
                context,
                assembly.GetName().Name,
                methodDescription);
        }

        internal static async Task<int> ExecuteMethodAsync(string[] args,
            InvocationContext<int> context,
            string commandName,
            MethodDescription methodDescription)
        {
            var method = context.MethodInfo;
            var parameters = method.GetParameters();

            var helpOption = new OptionDefinition("--help", "Show help output");
            var optionDefinitions = new List<OptionDefinition> {
                helpOption,
            };

            var paramOptions = new OptionDefinition[parameters.Length];

            for (var i = 0; i < parameters.Length; i++)
            {
                var optionDefinition = paramOptions[i] = CreateOption(parameters[i], methodDescription);
                optionDefinitions.Add(optionDefinition);
            }

            var commandDefinition = new CommandDefinition(
                name: commandName,
                description: methodDescription?.Description,
                symbolDefinitions: optionDefinitions,
                argumentDefinition: ArgumentDefinition.None);

            var parser = new Parser(new ParserConfiguration(new[] {commandDefinition}));

            var result = parser.Parse(args);

            if (result.Errors.Count > 0)
            {
                return HandleParserErrors(result, context.Console);
            }

            if (result.HasOption(helpOption))
            {
                context.Console.Out.WriteLine(commandDefinition.HelpView());
                return HelpExitCode;
            }

            var rootCommand = result.Command();

            var values = new object[parameters.Length];
            for (var i = 0; i < paramOptions.Length; i++)
            {
                values[i] = rootCommand.ValueForOption(paramOptions[i]);
            }

            return await context.RunAsync(values);
        }

        private static OptionDefinition CreateOption(ParameterInfo parameter, MethodDescription methodDescription)
        {
            var paramName = parameter.Name.ToKebabCase();

            var argumentDefinitionBuilder = new ArgumentDefinitionBuilder();
            if (parameter.HasDefaultValue)
            {
                argumentDefinitionBuilder.WithDefaultValue(() => parameter.DefaultValue);
            }

            string description = null;
            methodDescription?.TryGetParameterDescription(parameter.Name, out description);

            var optionDefinition = new OptionDefinition(
                new[] {
                    "-" + paramName[0],
                    "--" + paramName,
                },
                description ?? parameter.Name,
                parameter.ParameterType != typeof(bool)
                    ? argumentDefinitionBuilder.ParseArgumentsAs(parameter.ParameterType)
                    : ArgumentDefinition.None);
            return optionDefinition;
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
