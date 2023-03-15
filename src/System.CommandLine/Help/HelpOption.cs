// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.Threading;
using System.Threading.Tasks;

namespace System.CommandLine.Help
{
    internal class HelpOption : Option<bool>
    {
        internal HelpOption(string name, string[] aliases)
            : base(name, aliases, new Argument<bool>(name) { Arity = ArgumentArity.Zero })
        {
            AppliesToSelfAndChildren = true;
            Description = LocalizationResources.HelpOptionDescription();
            Action = new HelpOptionAction();
        }

        internal HelpOption() : this("--help", new[] { "-h", "/h", "-?", "/?" })
        {
        }

        internal override bool IsGreedy => false;

        public override bool Equals(object? obj) => obj is HelpOption;

        public override int GetHashCode() => typeof(HelpOption).GetHashCode();

        private sealed class HelpOptionAction : CliAction
        {
            private static void Display(InvocationContext context)
            {
                var output = context.Console.Out.CreateTextWriter();

                var helpContext = new HelpContext(context.HelpBuilder,
                                                  context.ParseResult.CommandResult.Command,
                                                  output,
                                                  context.ParseResult);

                context.HelpBuilder.Write(helpContext);
            }

            public override int Invoke(InvocationContext context)
            {
                Display(context);

                return 0;
            }

            public override Task<int> InvokeAsync(InvocationContext context, CancellationToken cancellationToken = default)
                => cancellationToken.IsCancellationRequested 
                    ? Task.FromCanceled<int>(cancellationToken)
                    : Task.FromResult(Invoke(context));
        }

        
    }
}