// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine
{
    public static class ArgumentResultExtensions
    {
        internal static ArgumentResult ConvertIfNeeded(
            this ArgumentResult argumentResult,
            SymbolResult symbolResult,
            Type type)
        {
            if (argumentResult == null)
            {
                throw new ArgumentNullException(nameof(argumentResult));
            }

            switch (argumentResult)
            {
                case SuccessfulArgumentResult successful when !type.IsInstanceOfType(successful.Value):
                    return ArgumentConverter.ConvertObject(
                        argumentResult.Argument,
                        type,
                        successful.Value);

                case NoArgumentResult _ when type == typeof(bool):
                    return ArgumentResult.Success(argumentResult.Argument, true);

                case NoArgumentResult _ when argumentResult.Argument.Arity.MinimumNumberOfValues > 0:
                    return new MissingArgumentResult(
                        argumentResult.Argument,
                        ValidationMessages.Instance.RequiredArgumentMissing(symbolResult));

                case NoArgumentResult _ when type.IsEnumerable():
                    return ArgumentConverter.ConvertObject(
                        argumentResult.Argument,
                        type,
                        Array.Empty<string>());

                case TooManyArgumentsResult _:
                    return argumentResult;

                case MissingArgumentResult _:
                    return argumentResult;

                default:
                    return argumentResult;
            }
        }

        internal static object GetValueOrDefault(this ArgumentResult result) =>
            result.GetValueOrDefault<object>();

        internal static T GetValueOrDefault<T>(this ArgumentResult result)
        {
            switch (result)
            {
                case SuccessfulArgumentResult successful:
                    return (T)successful.Value;
                case FailedArgumentResult failed:
                    throw new InvalidOperationException(failed.ErrorMessage);
                case NoArgumentResult _:
                    return default;
                default:
                    return default;
            }
        }
    }
}
