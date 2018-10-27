// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
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
                var errorMessage = new List<(string, string)>();
                foreach (var arg in symbol.Arguments)
                {
                    try
                    {
                        var fileInfo = new FileInfo(arg);
                    }
                    catch (NotSupportedException ex)
                    {
                        errorMessage.Add((arg, ex.Message));
                    }
                    catch (ArgumentException ex)
                    {
                        errorMessage.Add((arg, ex.Message));
                    }

                    // File class no longer check invalid charactor
                    // https://blogs.msdn.microsoft.com/jeremykuhne/2018/03/09/custom-directory-enumeration-in-net-core-2-1/
                    var invalidCharactorsIndex = arg.IndexOfAny(Path.GetInvalidPathChars());
                    if (invalidCharactorsIndex >= 0)
                    {
                        errorMessage.Add((arg, arg[invalidCharactorsIndex] + " is invalid charactor in path {arg}"));
                    }
                }

                if (errorMessage.Any())
                {
                    return errorMessage
                           .Select(e => $"Arguement {e.Item1} failed validation due to {e.Item2}")
                           .Aggregate((current, next) => current + Environment.NewLine + next);
                }

                return null;
            });

            return builder;
        }

        #endregion

        #region type / return value

        public static Argument ParseArgumentsAs<T>(
            this ArgumentBuilder builder,
            ConvertArgument convert = null,
            ArgumentArity arity = null) =>
            ParseArgumentsAs(
                builder,
                typeof(T),
                convert,
                arity);

        public static Argument ParseArgumentsAs(
            this ArgumentBuilder builder,
            Type type,
            ConvertArgument convert = null,
            ArgumentArity arity = null)
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
            string description = null,
            bool? isHidden = null)
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

                if (isHidden != null)
                {
                    a.Help.IsHidden = isHidden.Value;
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

        public static ArgumentBuilder AddSuggestions(
            this ArgumentBuilder builder,
            params string[] suggestions)
        {
            builder.Configure(argument => argument.AddSuggestions(suggestions));

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

            builder.Configure(argument => argument.AddSuggestionSource(suggest));

            return builder;
        }
    }
}
