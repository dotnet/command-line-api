// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.Help
{
    public sealed class HelpOption : Option<bool>
    {
        private CliAction? _action;

        /// <summary>
        /// When added to a <see cref="Command"/>, it configures the application to show help when one of the following options are specified on the command line:
        /// <code>
        ///    -h
        ///    /h
        ///    --help
        ///    -?
        ///    /?
        /// </code>
        /// </summary>
        public HelpOption() : this("--help", new[] { "-h", "/h", "-?", "/?" })
        {
        }

        /// <summary>
        /// When added to a <see cref="Command"/>, it configures the application to show help when given name or one of the aliases are specified on the command line.
        /// </summary>
        public HelpOption(string name, params string[] aliases)
            : base(name, aliases, new Argument<bool>(name) { Arity = ArgumentArity.Zero })
        {
            AppliesToSelfAndChildren = true;
            Description = LocalizationResources.HelpOptionDescription();
        }

        /// <inheritdoc />
        public override CliAction? Action 
        { 
            get => _action ??= new HelpAction(); 
            set => _action = value ?? throw new ArgumentNullException(nameof(value));
        }

        internal override bool IsGreedy => false;

        public override bool Equals(object? obj) => obj is HelpOption;

        public override int GetHashCode() => typeof(HelpOption).GetHashCode();
    }
}