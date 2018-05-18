// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.Builder
{
    public abstract class SymbolDefinitionBuilder
    {
        private ArgumentDefinitionBuilder argumentDefinitionBuilder;

        protected SymbolDefinitionBuilder(CommandDefinitionBuilder parent = null)
        {
            Parent = parent;
        }

        public string Description { get; set; }

        public CommandDefinitionBuilder Parent { get; }

        public ArgumentDefinitionBuilder Arguments
        {
            get
            {
                if (argumentDefinitionBuilder == null)
                {
                    argumentDefinitionBuilder = new ArgumentDefinitionBuilder(this);
                }

                return argumentDefinitionBuilder;
            }
        }

        internal ArgumentDefinition BuildArguments()
        {
            if (argumentDefinitionBuilder != null)
            {
                return argumentDefinitionBuilder.Build();
            }
            else
            {
                return ArgumentDefinition.None;
            }
        }
    }
}
