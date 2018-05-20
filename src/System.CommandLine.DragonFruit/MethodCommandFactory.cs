// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Builder;
using System.Reflection;

namespace System.CommandLine.DragonFruit
{
    internal class MethodCommandFactory
    {
        public MethodCommand Create(
            MethodInfo method,
            CommandHelpMetadata helpMetadata,
            IEnumerable<OptionDefinition> additionalOptions)
        {
            var optionDefinitions = new List<OptionDefinition>(additionalOptions);
            ParameterInfo[] parameters = method.GetParameters();
            var paramOptions = new OptionDefinition[parameters.Length];

            for (int i = 0; i < parameters.Length; i++)
            {
                OptionDefinition optionDefinition = paramOptions[i] = CreateOption(parameters[i], helpMetadata);
                optionDefinitions.Add(optionDefinition);
            }

            var definition = new CommandDefinition(
                helpMetadata.Name ?? method.Name,
                helpMetadata.Description,
                optionDefinitions,
                ArgumentDefinition.None);

            return new MethodCommand(method, definition, paramOptions);
        }

        private static OptionDefinition CreateOption(ParameterInfo parameter, CommandHelpMetadata helpMetadata)
        {
            string paramName = parameter.Name.ToKebabCase();

            var argumentDefinitionBuilder = new ArgumentDefinitionBuilder();
            if (parameter.HasDefaultValue)
            {
                argumentDefinitionBuilder.WithDefaultValue(() => parameter.DefaultValue);
            }

            string description = null;
            helpMetadata?.TryGetParameterDescription(parameter.Name, out description);

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
    }
}
