// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine
{
    public static class ArgumentResultExtensions
    {
        internal static ArgumentResult GetValueAs(
            this ArgumentResult result, 
            Type type)
        {
            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            if (!(result is SuccessfulArgumentResult successful))
            {
                return result;
            }

            if (type.IsInstanceOfType(successful.Value))
            {
                return result;
            }

            return ArgumentConverter.Parse(result.Argument, type, successful.Value);
        }

        public static object GetValueOrDefault(this ArgumentResult result) =>
            result.GetValueOrDefault<object>();

        public static T GetValueOrDefault<T>(this ArgumentResult result)
        {
            switch (result)
            {
                case SuccessfulArgumentResult successful:
                    return (T)successful.Value;
                case FailedArgumentResult failed:
                    throw new InvalidOperationException(failed.ErrorMessage);
                case NoArgumentResult _:
                default:
                    return default;
            }
        }
    }
}
