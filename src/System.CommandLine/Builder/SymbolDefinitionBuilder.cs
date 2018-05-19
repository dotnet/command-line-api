// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.Builder
{
    public abstract class SymbolDefinitionBuilder
    {
        private ArgumentDefinitionBuilder _argumentDefinitionBuilder;

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
                if (_argumentDefinitionBuilder == null)
                {
                    _argumentDefinitionBuilder = new ArgumentDefinitionBuilder(this);
                }

                return _argumentDefinitionBuilder;
            }
        }

        internal ArgumentDefinition BuildArguments()
        {
            if (_argumentDefinitionBuilder != null)
            {
                return _argumentDefinitionBuilder.Build();
            }

            return ArgumentDefinition.None;
        }
    }
}
