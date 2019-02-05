// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Builder;
using System.CommandLine.Invocation;

namespace System.CommandLine.Binding
{
    public static class BindingExtensions
    {
        public static InvocationContext MakeDefaultInvocationContext(this Command command, string commandLine)
        {
            var parser = new CommandLineBuilder(command)
                         .UseDefaults()
                         .Build();
            var parseResult = parser.Parse(commandLine);
            var invocationContext = new InvocationContext(parseResult, parser);
            return invocationContext;
        }

        public static InvocationContext MakeSimpleInvocationContext(this Command command, string[] commandLine) 
            => MakeDefaultInvocationContext(command, string.Join("", commandLine));

        public static void AddBinding(IBinder binder, BindingSide targetSide, BindingSide parserSide) 
            => binder.AddBinding(new Binding(targetSide, parserSide));

    }
}
