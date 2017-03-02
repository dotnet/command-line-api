using System;
using System.Collections.Generic;
using System.Linq;

namespace CommandLine
{
    public class AppliedOption : IAliased
    {
        private readonly List<string> arguments = new List<string>();

        private readonly OptionSet<AppliedOption> appliedOptions = new OptionSet<AppliedOption>();

        public AppliedOption(Option option, string token = null)
        {
            if (option == null)
            {
                throw new ArgumentNullException(nameof(option));
            }

            Option = option;

            Token = token ?? option.ToString();
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

        public IReadOnlyCollection<string> TryTakeTokens(params string[] tokens)
        {
            if (!tokens.Any())
            {
                return Array.Empty<string>();
            }

            var remainder = AddTokensToChildOption(tokens);

            while (remainder.Any())
            {
                arguments.Add(remainder.First());

                if (Validate() == null)
                {
                    remainder = remainder.Skip(1).ToArray();
                }
                else
                {
                    arguments.RemoveAt(arguments.Count - 1);
                    break;
                }
            }

            return remainder;
        }

        private IReadOnlyCollection<string> AddTokensToChildOption(IReadOnlyCollection<string> tokens)
        {
            var firstToken = tokens.First();

            var childOption =
                Option.DefinedOptions
                      .Where(o => o.HasAlias(firstToken))
                      .Select(o => new AppliedOption(o, firstToken))
                      .Do(appliedOptions.TryAdd)
                      .FirstOrDefault();

            if (childOption != null)
            {
                IReadOnlyCollection<string> remainder = tokens.Skip(1).ToArray();

                if (remainder.Any())
                {
                    remainder = childOption.TryTakeTokens(remainder.ToArray());
                }

                return remainder;
            }

            foreach (var appliedOption in appliedOptions)
            {
                tokens = appliedOption.TryTakeTokens(tokens.ToArray());
            }

            return tokens;
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

        public object Value() => Option.Materialize(this);

        public override string ToString() => this.Diagram();
    }
}