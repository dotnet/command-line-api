// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.DotNet.Cli.CommandLine
{
    public class AppliedOption
    {
        private readonly List<string> arguments = new List<string>();
        private readonly Lazy<string> defaultValue;
        private readonly Func<object> materialize;
        private readonly AppliedOptionSet appliedOptions = new AppliedOptionSet();
        private bool considerAcceptingAnotherArgument = true;

        public AppliedOption(Option option, string token = null)
        {
            if (option == null)
            {
                throw new ArgumentNullException(nameof(option));
            }

            Option = option;

            defaultValue = new Lazy<string>(option.ArgumentsRule.GetDefaultValue);

            Token = token ?? option.ToString();

            materialize = () => option.ArgumentsRule.Materialize(this);
        }

        public AppliedOptionSet AppliedOptions =>
            appliedOptions;

        public IReadOnlyCollection<string> Arguments
        {
            get
            {
                if (arguments.Any() ||
                    defaultValue.Value == null)
                {
                    return arguments.ToArray();
                }

                return new[] { defaultValue.Value };
            }
        }

        public string Name => Option.Name;

        public Option Option { get; }

        public string Token { get; }

        public AppliedOption TryTakeToken(Token token)
        {
            var option = TryTakeArgument(token) ??
                         TryTakeOptionOrCommand(token);
            considerAcceptingAnotherArgument = false;
            return option;
        }

        private AppliedOption TryTakeArgument(Token token)
        {
            if (token.Type != TokenType.Argument)
            {
                return null;
            }

            if (!considerAcceptingAnotherArgument &&
                !Option.IsCommand)
            {
                // Options must be respecified in order to accept additional arguments. This is 
                // not the case for commands.
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
                considerAcceptingAnotherArgument = false;
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
                // if a subcommand has already been applied, don't accept this one
                return null;
            }

            var applied =
                appliedOptions.SingleOrDefault(o => o.Option.HasRawAlias(token.Value));

            if (applied != null)
            {
                applied.OptionWasRespecified();
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

        internal void OptionWasRespecified() => considerAcceptingAnotherArgument = true;

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