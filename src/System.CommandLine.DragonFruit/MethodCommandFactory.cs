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
            var parameters = method.GetParameters();
            var paramOptions = new OptionDefinition[parameters.Length];

            for (var i = 0; i < parameters.Length; i++)
            {
                var optionDefinition = paramOptions[i] = CreateOption(parameters[i], helpMetadata);
                optionDefinitions.Add(optionDefinition);
            }

            var definition = new CommandDefinition(
                name: helpMetadata.Name ?? method.Name,
                description: helpMetadata.Description,
                symbolDefinitions: optionDefinitions,
                argumentDefinition: ArgumentDefinition.None);

            return new MethodCommand(method, definition, paramOptions);
        }

        private static OptionDefinition CreateOption(ParameterInfo parameter, CommandHelpMetadata helpMetadata)
        {
            var paramName = parameter.Name.ToKebabCase();

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
