// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.Builder
{
    public static class ArgumentBuilderExtensions
    {
        public static Argument ExactlyOne(
            this ArgumentBuilder builder)
        {
            builder.Configure(argument => argument.Arity = ArgumentArity.ExactlyOne);

            return builder.Build();
        }

        public static Argument None(
            this ArgumentBuilder builder)
        {
            builder.Configure(argument => argument.Arity = ArgumentArity.Zero);

            return builder.Build();
        }

        public static Argument ZeroOrMore(
            this ArgumentBuilder builder)
        {
            builder.Configure(argument => argument.Arity = ArgumentArity.ZeroOrMore);

            return builder.Build();
        }

        public static Argument ZeroOrOne(
            this ArgumentBuilder builder)
        {
            builder.Configure(argument => argument.Arity = ArgumentArity.ZeroOrOne);

            return builder.Build();
        }

        public static Argument OneOrMore(
            this ArgumentBuilder builder)
        {
            builder.Configure(argument => argument.Arity = ArgumentArity.OneOrMore);

            return builder.Build();
        }

        public static ArgumentBuilder WithHelp(
            this ArgumentBuilder builder,
            string name = null,
            string description = null)
        {
            builder.Configure(a => a.WithHelp(name, description));

            return builder;
        }
    }
}
