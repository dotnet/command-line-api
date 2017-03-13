// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.DotNet.Cli.CommandLine
{
    public class AppliedOption : IAliased
    {
        private readonly List<string> arguments = new List<string>();

        private readonly Func<object> materialize;

        private readonly OptionSet<AppliedOption> appliedOptions = new OptionSet<AppliedOption>();

        public AppliedOption(Option option, string token = null)
        {
            if (option == null)
            {
                throw new ArgumentNullException(nameof(option));
            }

            Option = option;

            Token = token ?? option.ToString();

            materialize = () => option.ArgumentsRule.Materialize(this);
        }

        public OptionSet<AppliedOption> AppliedOptions =>
            appliedOptions;

        public IReadOnlyCollection<string> Arguments
        {
            get
            {
                if (arguments.Any()
                    || Option.ArgumentsRule.DefaultValue == null)
                {
                    return arguments.ToArray();
                }
                else
                {
                    return new[] { Option.ArgumentsRule.DefaultValue };
                }
            }
        }

        public string Name => Option.Name;

        public Option Option { get; }

        public string Token { get; }

        public AppliedOption TryTakeToken(Token token) =>
            TryTakeArgument(token) ??
            TryTakeOptionOrCommand(token);

        private AppliedOption TryTakeArgument(Token token)
        {
            if (token.Type != TokenType.Argument)
            {
                return null;
            }

            foreach (var appliedOption in appliedOptions)
            {
                var a = appliedOption.TryTakeToken(token);
                if (a != null)
                {
                    return a;
                }
            }

            arguments.Add(token.Value);

            if (Validate() == null)
            {
                return this;
            }

            arguments.RemoveAt(arguments.Count - 1);
            return null;
        }

        private AppliedOption TryTakeOptionOrCommand(Token token)
        {
            var childOption = appliedOptions
                .SingleOrDefault(o =>
                                     o.Option.DefinedOptions
                                      .Any(oo => oo.RawAliases.Contains(token.Value)));

            if (childOption != null)
            {
                return childOption.TryTakeToken(token);
            }

            if (token.Type == TokenType.Command &&
                appliedOptions.Any(o => o.Option.IsCommand && !o.HasAlias(token.Value)))
            {
                return null;
            }

            var applied =
                appliedOptions.SingleOrDefault(o => o.HasAlias(token.Value));

            if (applied != null)
            {
                return applied;
            }

            applied =
                Option.DefinedOptions
                      .Where(o => o.RawAliases.Contains(token.Value))
                      .Select(o => new AppliedOption(o, token.Value))
                      .SingleOrDefault();

            if (applied != null)
            {
                appliedOptions.Add(applied);
            }

            return applied;
        }

        internal OptionError Validate()
        {
            var error = Option.ArgumentsRule.Validate(this);
            return string.IsNullOrWhiteSpace(error)
                       ? null
                       : new OptionError(error, Token, this);
        }

        public AppliedOption this[string alias] => AppliedOptions[alias];

        public IReadOnlyCollection<string> Aliases => Option.Aliases;

        public bool HasAlias(string alias) => Option.HasAlias(alias);

        public T Value<T>() => (T) Value();

        public object Value()
        {
            try
            {
                return materialize();
            }
            catch (Exception exception)
            {
                var argumentsDescription = Arguments.Any()
                                               ? string.Join(", ", Arguments)
                                               : " (none)";
                throw new ParseException(
                    $"An exception occurred while getting the value for option '{Option.Name}' based on argument(s): {argumentsDescription}.",
                    exception);
            }
        }

        public override string ToString() => this.Diagram();
    }
}