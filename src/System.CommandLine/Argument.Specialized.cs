// Copyright (c) .NET Foundation and contributors.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Binding;
using System.CommandLine.Parsing;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;

namespace System.CommandLine
{

#if NET7_0_OR_GREATER
    public sealed class SpanParsableArgument<T> : Argument<T> where T : ISpanParsable<T>
    {
        public SpanParsableArgument(string name) : base(name)
        {
            ConvertArguments = (ArgumentResult result, out object? value) =>
            {
                var arg = result.Argument;
                if (arg.Arity.MaximumNumberOfValues == 1 && result.Tokens.Count > 0)
                {
                    var text = result.Tokens[result.Tokens.Count - 1].Value;
                    if (T.TryParse(text.AsSpan(), CultureInfo.CurrentCulture, out var parsed))
                    {
                        value = parsed;
                        return true;
                    }
                    value = null;
                    return false;
                }

                // Delegate to existing pipeline for multi-token scenarios
                var success = ArgumentConverter.TryConvertArgument(result, out value);
                return success;
            };
        }
    }

    public sealed class ParsableArgument<T> : Argument<T> where T : IParsable<T>
    {
        public ParsableArgument(string name) : base(name)
        {
            ConvertArguments = (ArgumentResult result, out object? value) =>
            {
                var arg = result.Argument;
                if (arg.Arity.MaximumNumberOfValues == 1 && result.Tokens.Count > 0)
                {
                    var text = result.Tokens[result.Tokens.Count - 1].Value;
                    if (T.TryParse(text, CultureInfo.CurrentCulture, out var parsed))
                    {
                        value = parsed;
                        return true;
                    }
                    value = null;
                    return false;
                }

                var success = ArgumentConverter.TryConvertArgument(result, out value);
                return success;
            };
        }
    }

    public sealed class ParsableListArgument<T> : Argument<List<T>> where T : IParsable<T>
    {
        public ParsableListArgument(string name) : base(name)
        {
            ConvertArguments = (ArgumentResult result, out object? value) =>
            {
                var arg = result.Argument;
                var list = new List<T>();
                var addedError = false;
                foreach (var token in result.Tokens)
                {
                    if (T.TryParse(token.Value, CultureInfo.CurrentCulture, out var parsed))
                    {
                        list.Add(parsed);
                    }
                    else
                    {
                        addedError = true;
                        result.AddError($"Failed to parse '{token.Value}' as {typeof(T).Name}.");
                    }
                }
                if (!addedError)
                {
                    value = list;
                    return true;
                }
                else
                {
                    value = null;
                    return false;
                }
            };
        }
    }
#endif

}
