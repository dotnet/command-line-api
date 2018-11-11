// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.Builder
{
    public static class ArgumentBuilderExtensions
    {
        #region arity

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

        #endregion

        #region set inclusion

        public static ArgumentBuilder FromAmong(
            this ArgumentBuilder builder,
            params string[] values)
        {
            builder.Configure(argument =>
            {
                argument.AddValidValues(values);
                argument.AddSuggestions(values);
            });

            return builder;
        }

        #endregion

        #region type / return value

        public static Argument ParseArgumentsAs<T>(
            this ArgumentBuilder builder,
            ConvertArgument convert = null,
            IArgumentArity arity = null) =>
            ParseArgumentsAs(
                builder,
                typeof(T),
                convert,
                arity);

        public static Argument ParseArgumentsAs(
            this ArgumentBuilder builder,
            Type type,
            ConvertArgument convert = null,
            IArgumentArity arity = null)
        {
            builder.Configure(a =>
            {
                a.ArgumentType = type;

                if (convert != null)
                {
                    a.ConvertArguments = convert;
                }

                if (arity != null)
                {
                    a.Arity = arity;
                }
            });

            return builder.Build();
        }

        #endregion

        public static ArgumentBuilder WithHelp(
            this ArgumentBuilder builder,
            string name = null,
            string description = null)
        {
            builder.Configure(a =>
            {
                if (name != null)
                {
                    a.Help.Name = name;
                }

                if (description != null)
                {
                    a.Help.Description = description;
                }
            });

            return builder;
        }

        public static ArgumentBuilder WithDefaultValue(
            this ArgumentBuilder builder,
            Func<object> defaultValue)
        {
            builder.Configure(argument => argument.SetDefaultValue(defaultValue));

            return builder;
        }
    }
}
