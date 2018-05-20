// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Linq;

namespace System.CommandLine.Builder
{
    public class ParserBuilder : CommandDefinitionBuilder
    {
        public bool EnablePosixBundling { get; set; } = true;

        public ResponseFileHandling ResponseFileHandling { get; set; }

        public Parser Build()
        {
            return new Parser(
                new ParserConfiguration(
                    BuildChildSymbolDefinitions(),
                    allowUnbundling: EnablePosixBundling,
                    validationMessages: ValidationMessages.Instance,
                    responseFileHandling: ResponseFileHandling));
        }

        public override CommandDefinition BuildCommandDefinition()
        {
            if (CommandDefinitionBuilders?.Count == 1)
            {
                return CommandDefinitionBuilders.Single().BuildCommandDefinition();
            }

            return CommandDefinition.CreateImplicitRootCommand(BuildChildSymbolDefinitions().ToArray());
        }
    }
}
