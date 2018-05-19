﻿// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Builder;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace System.CommandLine.DragonFruit
{
    public class CommandLine
    {
        public static int ExecuteAssembly(Assembly assembly, string[] args)
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
                                                       BindingFlags.NonPublic).Where(m => string.Equals("Main", m.Name, StringComparison.OrdinalIgnoreCase)))
                {
                    candidates.Add(method);
                }
            }

            if (candidates.Count > 1)
            {
                throw new InvalidProgramException("Ambiguous entry point. Could not identify which method is the main entry point for this function.");
            }

            var mainmethod = candidates[0];

            var parserBuilder = new ParserBuilder();

            parserBuilder.AddCommand(
                mainmethod.Name, string.Empty,
                cmd => {
                    foreach (var parameter in mainmethod.GetParameters())
                    {
                        cmd.AddOption("--" + parameter.Name, parameter.Name,
                            a => { a.ParseArgumentsAs(parameter.ParameterType); });
                    }
                });

            var command = parserBuilder.BuildCommandDefinition();

            Console.WriteLine(command.HelpView());

            return 0;
        }
    }
}
