// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.Invocation
{
    public static class CommandHandlerGeneratorExtensions
    {
        public static ICommandHandler Generate<TDelegate>(this CommandHandlerGenerator handler, 
            TDelegate @delegate, params IdentifierSymbol[] symbols)
            where TDelegate : Delegate
        {
            throw new InvalidOperationException("Should never get here....");
        }

        public static ICommandHandler Generate<TDelegate, TModel>(this CommandHandlerGenerator handler,
            TDelegate @delegate, Func<InvocationContext, TModel> modelBuilder)
            where TDelegate : Delegate
        {
            throw new InvalidOperationException("Should never get here....");
        }
    }
}