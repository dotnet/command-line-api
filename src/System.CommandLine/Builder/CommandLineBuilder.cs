// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Binding;
using System.CommandLine.Help;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;

namespace System.CommandLine.Builder
{
    /// <summary>
    /// Enables composition of command line configurations.
    /// </summary>
    public class CommandLineBuilder 
    {
        // for every generic type with type argument being struct JIT needs to compile a dedicated version
        // (because each struct is of a different size)
        // that is why we don't use List<ValueTuple> for middleware
        private List<Tuple<InvocationMiddleware, int>>? _middlewareList;
        private LocalizationResources? _localizationResources;
        private Action<HelpContext>? _customizeHelpBuilder;
        private Func<BindingContext, HelpBuilder>? _helpBuilderFactory;

        /// <param name="rootCommand">The root command of the application.</param>
        public CommandLineBuilder(Command? rootCommand = null)
        {
            Command = rootCommand ?? new RootCommand();
        }

        /// <summary>
        /// The command that the builder uses the root of the parser.
        /// </summary>
        public Command Command { get; }

        /// <summary>
        /// Determines whether the parser recognizes command line directives.
        /// </summary>
        /// <seealso cref="DirectiveCollection"/>
        internal bool EnableDirectives { get; set; } = true;

        /// <summary>
        /// Determines whether the parser recognize and expands POSIX-style bundled options.
        /// </summary>
        internal bool EnablePosixBundling { get; set; } = true;
        
        internal bool EnableTokenReplacement { get; set; } = true;

        /// <summary>
        /// Determines the behavior when parsing a double dash (<c>--</c>) in a command line.
        /// </summary>
        /// <remarks>When set to <see langword="true"/>, all tokens following <c>--</c> will be placed into the <see cref="ParseResult.UnparsedTokens"/> collection. When set to <see langword="false"/>, all tokens following <c>--</c> will be treated as command arguments, even if they match an existing option.</remarks>
        internal bool EnableLegacyDoubleDashBehavior { get; set; }
        
        internal void CustomizeHelpLayout(Action<HelpContext> customize) => 
            _customizeHelpBuilder = customize;

        internal void UseHelpBuilderFactory(Func<BindingContext, HelpBuilder> factory) =>
            _helpBuilderFactory = factory;

        private Func<BindingContext, HelpBuilder> GetHelpBuilderFactory()
        {
            return CreateHelpBuilder;

            HelpBuilder CreateHelpBuilder(BindingContext bindingContext)
            {
                var helpBuilder = _helpBuilderFactory is { }
                                             ? _helpBuilderFactory(bindingContext)
                                             : CommandLineConfiguration.DefaultHelpBuilderFactory(bindingContext, MaxHelpWidth);

                helpBuilder.OnCustomize = _customizeHelpBuilder;

                return helpBuilder;
            }
        }

        internal HelpOption? HelpOption { get; set; }

        internal VersionOption? VersionOption { get; set; }

        internal int? MaxHelpWidth { get; set; }

        internal LocalizationResources LocalizationResources
        {
            get => _localizationResources ??= LocalizationResources.Instance;
            set => _localizationResources = value;
        }

        internal TryReplaceToken? TokenReplacer { get; set; }

        /// <summary>
        /// Creates a parser based on the configuration of the command line builder.
        /// </summary>
        public Parser Build() =>
            new(
                new CommandLineConfiguration(
                    Command,
                    enablePosixBundling: EnablePosixBundling,
                    enableDirectives: EnableDirectives,
                    enableLegacyDoubleDashBehavior: EnableLegacyDoubleDashBehavior,
                    enableTokenReplacement: EnableTokenReplacement,
                    resources: LocalizationResources,
                    middlewarePipeline: _middlewareList is null
                                            ? Array.Empty<InvocationMiddleware>()
                                            : GetMiddleware(),
                    helpBuilderFactory: GetHelpBuilderFactory(),
                    tokenReplacer: TokenReplacer));

        private IReadOnlyList<InvocationMiddleware> GetMiddleware()
        {
            _middlewareList!.Sort(static (m1, m2) => m1.Item2.CompareTo(m2.Item2));
            InvocationMiddleware[] result = new InvocationMiddleware[_middlewareList.Count];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = _middlewareList[i].Item1;
            }
            return result;
        }

        internal void AddMiddleware(InvocationMiddleware middleware, MiddlewareOrder order)
            => AddMiddleware(middleware, (int)order);

        internal void AddMiddleware(InvocationMiddleware middleware, MiddlewareOrderInternal order)
            => AddMiddleware(middleware, (int)order);

        private void AddMiddleware(InvocationMiddleware middleware, int order)
            => (_middlewareList ??= new()).Add(new Tuple<InvocationMiddleware, int>(middleware, order));
    }
}
