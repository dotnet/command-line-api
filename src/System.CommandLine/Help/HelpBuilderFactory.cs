// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Binding;

namespace System.CommandLine
{
    internal class HelpBuilderFactory : IHelpBuilderFactory
    {
        public IHelpBuilder CreateHelpBuilder(BindingContext context)
        {
            return new HelpBuilder(context.Console);
        }
    }
}
