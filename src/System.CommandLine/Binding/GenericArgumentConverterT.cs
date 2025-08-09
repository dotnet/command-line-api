// Copyright (c) .NET Foundation and contributors.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Parsing;
using System.Globalization;
using System.IO;

namespace System.CommandLine.Binding
{
    // Reflection-free, trimming-safe generic converter used by Argument<T> for single-value parsing.
    internal static class GenericArgumentConverter<T>
    {
        internal static bool TryConvert(ArgumentResult argumentResult, out object? value)
        {
            var arity = argumentResult.Argument.Arity;

            // Delegate multi-value cases to existing pipeline to preserve semantics for now.
            if (!(arity.MinimumNumberOfValues == 1 && arity.MaximumNumberOfValues == 1))
            {
                // Use existing converter end-to-end.
                var ok = ArgumentConverter.TryConvertArgument(argumentResult, out var v);
                value = v;
                return ok;
            }

            // Single token expected (arity validated before conversion)
            var tokens = argumentResult.Tokens;
            var text = tokens[tokens.Count - 1].Value;

            // string
            if (typeof(T) == typeof(string))
            {
                value = text;
                return true;
            }

            // Enums
            if (typeof(T).IsEnum)
            {
#if NET7_0_OR_GREATER
                if (Enum.TryParse(typeof(T), text, ignoreCase: true, out var converted))
                {
                    value = converted;
                    return true;
                }
#else
                try
                {
                    value = Enum.Parse(typeof(T), text, true);
                    return true;
                }
                catch (ArgumentException)
                {
                    value = null;
                    return false;
                }
#endif
            }

            // Nullable primitives and enums
            if (TryParseNullable(text, out value))
            {
                return true;
            }

            // Common BCLs and primitives
            if (TryParseCommon(text, out value))
            {
                return true;
            }

            // File system types
            if (typeof(T) == typeof(FileInfo))
            {
                if (string.IsNullOrEmpty(text))
                {
                    value = null;
                    return false;
                }
                value = new FileInfo(text);
                return true;
            }
            if (typeof(T) == typeof(DirectoryInfo))
            {
                if (string.IsNullOrEmpty(text))
                {
                    value = null;
                    return false;
                }
                value = new DirectoryInfo(text);
                return true;
            }
            if (typeof(T) == typeof(FileSystemInfo))
            {
                if (string.IsNullOrEmpty(text))
                {
                    value = null;
                    return false;
                }
                if (Directory.Exists(text))
                {
                    value = new DirectoryInfo(text);
                    return true;
                }
                if (text.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal) ||
                    text.EndsWith(Path.AltDirectorySeparatorChar.ToString(), StringComparison.Ordinal))
                {
                    value = new DirectoryInfo(text);
                    return true;
                }
                value = new FileInfo(text);
                return true;
            }

            // Fallback to existing converters for exact matches
            if (TryWithStringConverters(typeof(T), text, out value))
            {
                return true;
            }

            value = null;
            return false;
        }

        private static bool TryParseCommon(string text, out object? value)
        {
            var provider = CultureInfo.CurrentCulture;

            if (typeof(T) == typeof(int) && int.TryParse(text, NumberStyles.Integer, provider, out var i)) { value = i; return true; }
            if (typeof(T) == typeof(long) && long.TryParse(text, NumberStyles.Integer, provider, out var l)) { value = l; return true; }
            if (typeof(T) == typeof(short) && short.TryParse(text, NumberStyles.Integer, provider, out var s)) { value = s; return true; }
            if (typeof(T) == typeof(byte) && byte.TryParse(text, NumberStyles.Integer, provider, out var b)) { value = b; return true; }
            if (typeof(T) == typeof(sbyte) && sbyte.TryParse(text, NumberStyles.Integer, provider, out var sb)) { value = sb; return true; }
            if (typeof(T) == typeof(uint) && uint.TryParse(text, NumberStyles.Integer, provider, out var ui)) { value = ui; return true; }
            if (typeof(T) == typeof(ulong) && ulong.TryParse(text, NumberStyles.Integer, provider, out var ul)) { value = ul; return true; }
            if (typeof(T) == typeof(ushort) && ushort.TryParse(text, NumberStyles.Integer, provider, out var usv)) { value = usv; return true; }
            if (typeof(T) == typeof(decimal) && decimal.TryParse(text, NumberStyles.Number, provider, out var dec)) { value = dec; return true; }
            if (typeof(T) == typeof(double) && double.TryParse(text, NumberStyles.Float | NumberStyles.AllowThousands, provider, out var d)) { value = d; return true; }
            if (typeof(T) == typeof(float) && float.TryParse(text, NumberStyles.Float | NumberStyles.AllowThousands, provider, out var f)) { value = f; return true; }
            if (typeof(T) == typeof(bool) && bool.TryParse(text, out var bo)) { value = bo; return true; }
            if (typeof(T) == typeof(Guid) && Guid.TryParse(text, out var g)) { value = g; return true; }
            if (typeof(T) == typeof(TimeSpan) && TimeSpan.TryParse(text, provider, out var ts)) { value = ts; return true; }
            if (typeof(T) == typeof(DateTime) && DateTime.TryParse(text, provider, DateTimeStyles.None, out var dt)) { value = dt; return true; }
            if (typeof(T) == typeof(DateTimeOffset) && DateTimeOffset.TryParse(text, provider, DateTimeStyles.None, out var dto)) { value = dto; return true; }
#if NET6_0_OR_GREATER
            if (typeof(T) == typeof(DateOnly) && DateOnly.TryParse(text, provider, out var donly)) { value = donly; return true; }
            if (typeof(T) == typeof(TimeOnly) && TimeOnly.TryParse(text, provider, out var tonly)) { value = tonly; return true; }
#endif
            if (typeof(T) == typeof(Uri) && Uri.TryCreate(text, UriKind.RelativeOrAbsolute, out var uri)) { value = uri; return true; }

            value = null;
            return false;
        }

        private static bool TryParseNullable(string text, out object? value)
        {
            var provider = CultureInfo.CurrentCulture;

            if (typeof(T) == typeof(int?) && int.TryParse(text, NumberStyles.Integer, provider, out var i)) { value = (int?)i; return true; }
            if (typeof(T) == typeof(long?) && long.TryParse(text, NumberStyles.Integer, provider, out var l)) { value = (long?)l; return true; }
            if (typeof(T) == typeof(short?) && short.TryParse(text, NumberStyles.Integer, provider, out var s)) { value = (short?)s; return true; }
            if (typeof(T) == typeof(byte?) && byte.TryParse(text, NumberStyles.Integer, provider, out var b)) { value = (byte?)b; return true; }
            if (typeof(T) == typeof(sbyte?) && sbyte.TryParse(text, NumberStyles.Integer, provider, out var sb)) { value = (sbyte?)sb; return true; }
            if (typeof(T) == typeof(uint?) && uint.TryParse(text, NumberStyles.Integer, provider, out var ui)) { value = (uint?)ui; return true; }
            if (typeof(T) == typeof(ulong?) && ulong.TryParse(text, NumberStyles.Integer, provider, out var ul)) { value = (ulong?)ul; return true; }
            if (typeof(T) == typeof(ushort?) && ushort.TryParse(text, NumberStyles.Integer, provider, out var usv)) { value = (ushort?)usv; return true; }
            if (typeof(T) == typeof(decimal?) && decimal.TryParse(text, NumberStyles.Number, provider, out var dec)) { value = (decimal?)dec; return true; }
            if (typeof(T) == typeof(double?) && double.TryParse(text, NumberStyles.Float | NumberStyles.AllowThousands, provider, out var d)) { value = (double?)d; return true; }
            if (typeof(T) == typeof(float?) && float.TryParse(text, NumberStyles.Float | NumberStyles.AllowThousands, provider, out var f)) { value = (float?)f; return true; }
            if (typeof(T) == typeof(bool?) && bool.TryParse(text, out var bo)) { value = (bool?)bo; return true; }
            if (typeof(T) == typeof(Guid?) && Guid.TryParse(text, out var g)) { value = (Guid?)g; return true; }
            if (typeof(T) == typeof(TimeSpan?) && TimeSpan.TryParse(text, provider, out var ts)) { value = (TimeSpan?)ts; return true; }
            if (typeof(T) == typeof(DateTime?) && DateTime.TryParse(text, provider, DateTimeStyles.None, out var dt)) { value = (DateTime?)dt; return true; }
            if (typeof(T) == typeof(DateTimeOffset?) && DateTimeOffset.TryParse(text, provider, DateTimeStyles.None, out var dto)) { value = (DateTimeOffset?)dto; return true; }
#if NET6_0_OR_GREATER
            if (typeof(T) == typeof(DateOnly?) && DateOnly.TryParse(text, provider, out var donly)) { value = (DateOnly?)donly; return true; }
            if (typeof(T) == typeof(TimeOnly?) && TimeOnly.TryParse(text, provider, out var tonly)) { value = (TimeOnly?)tonly; return true; }
#endif
            if (Nullable.GetUnderlyingType(typeof(T)) is Type underlying && underlying.IsEnum)
            {
#if NET7_0_OR_GREATER
                if (Enum.TryParse(typeof(T), text, ignoreCase: true, out var converted))
                {
                    value = converted;
                    return true;
                }
#else
                try
                {
                    value = Enum.Parse(typeof(T), text, true);
                    return true;
                }
                catch (ArgumentException)
                {
                    value = null;
                    return false;
                }
#endif
            }

            value = null;
            return false;
        }

        private static bool TryWithStringConverters(Type targetType, string text, out object? value)
        {
            // Delegate to existing ConvertObject path for exact target types
            var dummyArg = new Argument<string>("dummy");
            var tree = new SymbolResultTree(new RootCommand());
            var argResult = new ArgumentResult(dummyArg, tree, null);
            argResult.AddToken(new Token(text, TokenType.Argument, dummyArg));
            var converted = ArgumentConverter.ConvertObject(argResult, targetType, argResult.Tokens[argResult.Tokens.Count - 1]);
            if (converted.Result == ArgumentConversionResultType.Successful)
            {
                value = converted.Value;
                return true;
            }
            value = null;
            return false;
        }
    }
}
