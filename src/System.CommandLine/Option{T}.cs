// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Parsing;

namespace System.CommandLine
{
    public class Option<T> : Option
    {
        public Option(
            string alias, 
            string? description = null) : base(alias, description)
        {
            Argument = new Argument<T>();
        }

        public Option(
            string[] aliases, 
            string? description = null) : base(aliases, description)
        {
            Argument = new Argument<T>();
        }

        public Option(
            string alias,
            ParseArgument<T> parseArgument,
            bool isDefault = false,
            string? description = null) : base(alias, description)
        {
            if (parseArgument is null)
            {
                throw new ArgumentNullException(nameof(parseArgument));
            }

            Argument = new Argument<T>(parseArgument, isDefault);
        }

        public Option(
            string[] aliases,
            ParseArgument<T> parseArgument,
            bool isDefault = false,
            string? description = null) : base(aliases, description)
        {
            if (parseArgument is null)
            {
                throw new ArgumentNullException(nameof(parseArgument));
            }

            Argument = new Argument<T>(parseArgument, isDefault);
        }

        public Option(
            string alias,
            Func<T> getDefaultValue,
            string? description = null) : base(alias, description)
        {
            if (getDefaultValue is null)
            {
                throw new ArgumentNullException(nameof(getDefaultValue));
            }

            Argument = new Argument<T>(getDefaultValue);
        }

        public Option(
            string[] aliases,
            Func<T> getDefaultValue,
            string? description = null) : base(aliases, description)
        {
            if (getDefaultValue is null)
            {
                throw new ArgumentNullException(nameof(getDefaultValue));
            }

            Argument = new Argument<T>(getDefaultValue);
        }

        public override Argument Argument
        {
            set
            {
                if (!(value is Argument<T>))
                {
                    throw new ArgumentException($"{nameof(Argument)} must be of type {typeof(Argument<T>)} but was {value?.GetType().ToString() ?? "null"}");
                }

                base.Argument = value;
            }
        }
    }
}