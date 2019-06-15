// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Binding;
using System.Linq;

namespace System.CommandLine
{
    public class Option : Symbol, IOption
    {
        public Option(string alias, string description = null)
            : base(new[]
            {
                alias
            }, description)
        {
        }

        public Option(string[] aliases, string description = null) : base(aliases, description)
        {
        }

        public virtual Argument Argument
        {
            get => _arguments.FirstOrDefault() ?? Argument.None;
            set
            {
                if (_arguments.Any())
                {
                    _arguments.Clear();
                }

                AddArgumentInner(value);
            }
        }

        public void AddAlias(string alias) => AddAliasInner(alias);

        IArgument IOption.Argument => Argument;

        string IValueDescriptor.ValueName => Name;

        Type IValueDescriptor.Type => Argument.ArgumentType;

        bool IValueDescriptor.HasDefaultValue => _arguments.Single().HasDefaultValue;

        object IValueDescriptor.GetDefaultValue() => _arguments.Single().GetDefaultValue();
    }
}
