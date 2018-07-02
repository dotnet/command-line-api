// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections;
using System.IO;
using System.Linq;

namespace System.CommandLine.Builder
{
    public static class ArgumentBuilderExtensions
    {
        #region arity

        public static Argument ExactlyOne(
            this ArgumentBuilder builder)
        {
            builder.ArgumentArity = ArgumentArity.ExactlyOne;
            return builder.Build();
        }

        public static Argument None(
            this ArgumentBuilder builder)
        {
            builder.ArgumentArity = ArgumentArity.Zero;
            return builder.Build();
        }

        public static Argument ZeroOrMore(
            this ArgumentBuilder builder)
        {
            builder.ArgumentArity = ArgumentArity.ZeroOrMore;
            return builder.Build();
        }

        public static Argument ZeroOrOne(
            this ArgumentBuilder builder)
        {
            builder.ArgumentArity = ArgumentArity.ZeroOrOne;
            return builder.Build();
        }

        public static Argument OneOrMore(
            this ArgumentBuilder builder)
        {
            builder.ArgumentArity = ArgumentArity.OneOrMore;
            return builder.Build();
        }

        #endregion

        #region set inclusion

        public static ArgumentBuilder FromAmong(
            this ArgumentBuilder builder,
            params string[] values)
        {
            builder.ValidTokens.UnionWith(values);

            builder.SuggestionSource.AddSuggestions(values);

            return builder;
        }

        #endregion

        #region files

        public static ArgumentBuilder ExistingFilesOnly(
            this ArgumentBuilder builder)
        {
            builder.AddValidator(symbol =>
            {
                return symbol.Arguments
                                   .Where(filePath => !File.Exists(filePath) &&
                                                      !Directory.Exists(filePath))
                                   .Select(symbol.ValidationMessages.FileDoesNotExist)
                                   .FirstOrDefault();
            });
            return builder;
        }

        public static ArgumentBuilder LegalFilePathsOnly(
            this ArgumentBuilder builder)
        {
            builder.AddValidator(symbol =>
            {
                foreach (var arg in symbol.Arguments)
                {
                    try
                    {
                        var fileInfo = new FileInfo(arg);
                    }
                    catch (NotSupportedException ex)
                    {
                        return ex.Message;
                    }
                    catch (ArgumentException ex)
                    {
                        return ex.Message;
                    }
                }

                return null;
            });

            return builder;
        }

        #endregion

        #region type / return value

        public static Argument ParseArgumentsAs<T>(
            this ArgumentBuilder builder) =>
            ParseArgumentsAs(
                builder,
                typeof(T));

        public static Argument ParseArgumentsAs(
            this ArgumentBuilder builder,
            Type type) =>
            ParseArgumentsAs(
                builder,
                type,
                symbol => {
                    switch (type.DefaultArity().MaximumNumberOfArguments)
                    {
                        case 1:
                            return ArgumentConverter.Parse(type, symbol.Arguments.SingleOrDefault());
                        default:
                            return ArgumentConverter.ParseMany(type, symbol.Arguments);
                    }
                });

        public static Argument ParseArgumentsAs<T>(
            this ArgumentBuilder builder,
            ConvertArgument convert,
            ArgumentArityValidator arity = null) =>
            ParseArgumentsAs(
                builder,
                typeof(T),
                convert,
                arity);

        public static Argument ParseArgumentsAs(
            this ArgumentBuilder builder,
            Type type,
            ConvertArgument convert,
            ArgumentArityValidator arity = null)
        {
            if (convert == null)
            {
                throw new ArgumentNullException(nameof(convert));
            }

            arity = arity ?? type.DefaultArity();

            if (arity.MaximumNumberOfArguments == 1)
            {
                var originalConvert = convert;

                if (type == typeof(bool))
                {
                    convert = symbol =>
                        ArgumentConverter.Parse<bool>(symbol.Arguments.SingleOrDefault() ?? "true");
                }
                else
                {
                    convert = symbol => {
                        if (symbol.Arguments.Count != 1)
                        {
                            return ArgumentParseResult.Failure(symbol.ValidationMessages.ExpectsOneArgument(symbol));
                        }

                        return originalConvert(symbol);
                    };
                }
            }

            builder.ArgumentArity = arity;

            builder.ConvertArguments = convert;

            return builder.Build();
        }

        internal static ArgumentArityValidator DefaultArity(this Type type)
        {
            if (typeof(IEnumerable).IsAssignableFrom(type) &&
                type != typeof(string))
            {
                return ArgumentArity.OneOrMore;
            }

            if (type == typeof(bool))
            {
                return ArgumentArity.ZeroOrOne;
            }

            return ArgumentArity.ExactlyOne;
        }

        #endregion

        public static ArgumentBuilder WithHelp(
            this ArgumentBuilder builder,
            string name = null,
            string description = null,
            bool isHidden = HelpDefinition.DefaultIsHidden)
        {
            builder.Help = new HelpDefinition(name, description, isHidden);

            return builder;
        }

        public static ArgumentBuilder WithDefaultValue(
            this ArgumentBuilder builder,
            Func<object> defaultValue)
        {
            builder.DefaultValue = defaultValue;

            return builder;
        }

        public static ArgumentBuilder AddSuggestions(
            this ArgumentBuilder builder,
            params string[] suggestions)
        {
            builder.SuggestionSource.AddSuggestions(suggestions);

            return builder;
        }

        public static ArgumentBuilder AddSuggestionSource(
            this ArgumentBuilder builder,
            Suggest suggest)
        {
            if (suggest == null)
            {
                throw new ArgumentNullException(nameof(suggest));
            }

            builder.SuggestionSource.AddSuggestionSource(suggest);

            return builder;
        }
    }
}
