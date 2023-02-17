// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Invocation;
using System.CommandLine.IO;

namespace System.CommandLine.Help
{
    internal class HelpOption : Option<bool>
    {
        private readonly Func<LocalizationResources> _localizationResources;
        private string? _description;

        public HelpOption(string[] aliases, Func<LocalizationResources> getLocalizationResources)
            : base(aliases, null, new Argument<bool> { Arity = ArgumentArity.Zero })
        {
            _localizationResources = getLocalizationResources;
            AppliesToSelfAndChildren = true;
        }

        public HelpOption(Func<LocalizationResources> getLocalizationResources) : this(new[]
        {
            "-h",
            "/h",
            "--help",
            "-?",
            "/?"
        }, getLocalizationResources)
        {
        }

        public override string? Description
        {
            get => _description ??= _localizationResources().HelpOptionDescription();
            set => _description = value;
        }

        internal override bool IsGreedy => false;

        public override bool Equals(object? obj) => obj is HelpOption;

        public override int GetHashCode() => typeof(HelpOption).GetHashCode();

        internal static void Handler(InvocationContext context)
        {
            var output = context.Console.Out.CreateTextWriter();

            var helpContext = new HelpContext(context.BindingContext.HelpBuilder,
                                              context.ParseResult.CommandResult.Command,
                                              output,
                                              context.ParseResult);

            context.BindingContext
                   .HelpBuilder
                   .Write(helpContext);
        }
    }
}