// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine
{
    public class Option<T> : Option
    {
        public Option(string alias, string description = null) : base(alias, description)
        {
            Argument = new Argument<T>();
        }

        public Option(string[] aliases, string description = null) : base(aliases, description)
        {
            Argument = new Argument<T>();
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