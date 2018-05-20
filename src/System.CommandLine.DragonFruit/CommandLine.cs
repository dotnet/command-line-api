// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Builder;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace System.CommandLine.DragonFruit
{
    public class CommandLine
    {
        public static async Task<int> ExecuteAssemblyAsync(Assembly assembly, string[] args)
        {
            var candidates = new List<MethodInfo>();
            foreach (var type in assembly.DefinedTypes.Where(t =>
                !t.IsAbstract && string.Equals("Program", t.Name, StringComparison.OrdinalIgnoreCase)))
            {
                if (type.GetCustomAttribute<CompilerGeneratedAttribute>() != null)
                {
                    continue;
                }

                foreach (var method in type.GetMethods(BindingFlags.Static | BindingFlags.Public |
                                                       BindingFlags.NonPublic).Where(m =>
                    string.Equals("Main", m.Name, StringComparison.OrdinalIgnoreCase)))
                {
                    if (method.ReturnType == typeof(void)
                        || method.ReturnType == typeof(int)
                        || method.ReturnType == typeof(Task)
                        || method.ReturnType == typeof(Task<int>))
                    {
                        candidates.Add(method);
                    }
                }
            }

            if (candidates.Count > 1)
            {
                throw new AmbiguousMatchException(
                    "Ambiguous entry point. Found muliple static functions named 'Program.Main'. Could not identify which method is the main entry point for this function.");
            }

            if (candidates.Count == 0)
            {
                throw new InvalidProgramException(
                    "Could not find a static entry point named 'Main' on a type named 'Program' that accepts option parameters.");
            }

            var mainmethod = candidates[0];
            var parameters = mainmethod.GetParameters();

            var helpOption = new OptionDefinition("--help", "Show help output");
            var optionDefinitions = new List<OptionDefinition> {
                helpOption,
            };

            var paramOptions = new OptionDefinition[parameters.Length];

            for (var i = 0; i < parameters.Length; i++)
            {
                var parameter = parameters[i];
                var paramName = parameter.Name.ToKebabCase();

                var argumentDefinitionBuilder = new ArgumentDefinitionBuilder();
                if (parameter.HasDefaultValue)
                {
                    argumentDefinitionBuilder.WithDefaultValue(() => parameter.DefaultValue);
                }

                paramOptions[i] = new OptionDefinition(
                    new[] {
                        "-" + paramName[0],
                        "--" + paramName,
                    },
                    parameter.Name,
                    parameter.ParameterType != typeof(bool)
                        ? argumentDefinitionBuilder.ParseArgumentsAs(parameter.ParameterType)
                        : ArgumentDefinition.None);
                optionDefinitions.Add(paramOptions[i]);
            }

            var commandDefinition = new CommandDefinition(
                name: assembly.GetName().Name,
                description: string.Empty,
                symbolDefinitions: optionDefinitions,
                argumentDefinition: ArgumentDefinition.None);

            var parser = new Parser(new ParserConfiguration(new[] {commandDefinition}));

            var result = parser.Parse(args);

            if (result.Errors.Count > 0)
            {
                return HandleParserErrors(result);
            }

            if (result.HasOption(helpOption))
            {
                Console.WriteLine(commandDefinition.HelpView());
                return 3;
            }

            var rootCommand = result.Command();

            var values = new object[parameters.Length];
            for (var i = 0; i < paramOptions.Length; i++)
            {
                values[i] = rootCommand.ValueForOption(paramOptions[i]);
            }

            var retVal = mainmethod.Invoke(null, values);

            switch (retVal)
            {
                case Task<int> taskOfInt:
                    return await taskOfInt;
                case Task task:
                    await task;
                    return 0;
                case int exitCode:
                    return exitCode;
            }

            return 0;
        }

        private static int HandleParserErrors(ParseResult result)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            foreach (var parseError in result.Errors)
            {
                Console.Error.WriteLine(parseError.Message);
            }

            Console.ResetColor();


            Console.Error.WriteLine("Use --help to see available commands and options.");

            return 1;
        }
    }
}
