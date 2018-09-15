// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.Builder
{
    public abstract class SymbolBuilder
    {
        private ArgumentBuilder argumentBuilder;

        protected SymbolBuilder(CommandBuilder parent = null)
        {
            Parent = parent;
        }

        public string Description { get; set; }

        public CommandBuilder Parent { get; }

        public ArgumentBuilder Arguments
        {
            get
            {
                if (argumentBuilder == null)
                {
                    argumentBuilder = new ArgumentBuilder();
                }

                return argumentBuilder;
            }
        }

        internal Argument BuildArguments()
        {
            if (argumentBuilder != null)
            {
                return argumentBuilder.Build();
            }

            return Argument.None;
        }
    }
}
