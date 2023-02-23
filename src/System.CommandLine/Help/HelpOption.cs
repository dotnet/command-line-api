// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Invocation;
using System.CommandLine.IO;

namespace System.CommandLine.Help
{
    internal class HelpOption : Option<bool>
    {
        internal HelpOption(string[] aliases)
            : base(aliases, LocalizationResources.HelpOptionDescription(), new Argument<bool> { Arity = ArgumentArity.Zero })
        {
            AppliesToSelfAndChildren = true;
        }

        internal HelpOption() : this(new[]
        {
            "-h",
            "/h",
            "--help",
            "-?",
            "/?"
        })
        {
        }

        internal override bool IsGreedy => false;

        public override bool Equals(object? obj) => obj is HelpOption;

        public override int GetHashCode() => typeof(HelpOption).GetHashCode();

        internal static void Handler(InvocationContext context)
        {
            var output = context.Console.Out.CreateTextWriter();

            var helpContext = new HelpContext(context.HelpBuilder,
                                              context.ParseResult.CommandResult.Command,
                                              output,
                                              context.ParseResult);

            context.HelpBuilder.Write(helpContext);
        }

        internal HelpAction Action => new HelpAction(this);
    }

    internal class HelpAction : CliAction
    {
        public HelpAction(HelpOption helpOption) : base(new AnonymousCommandHandler(HelpOption.Handler))
        {
            
        }
    }
}