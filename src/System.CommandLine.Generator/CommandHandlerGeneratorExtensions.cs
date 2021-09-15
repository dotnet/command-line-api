// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.Invocation
{
    public static class CommandHandlerGeneratorExtensions
    {
        public static ICommandHandler Create<TDelegate>(this CommandHandlerGenerator handler, 
            TDelegate @delegate, params ISymbol[] symbols)
            where TDelegate : Delegate
        {
            //TODO: Better exception/message
            throw new InvalidOperationException("Should never get here....");
        }

        public static ICommandHandler Create<TDelegate, TModel>(this CommandHandlerGenerator handler,
            TDelegate @delegate, Func<InvocationContext, TModel> modelBuilder)
            where TDelegate : Delegate
        {
            //TODO: Better exception/message
            throw new InvalidOperationException("Should never get here....");
        }
    }
}