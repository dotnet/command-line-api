// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Linq;

namespace System.CommandLine
{
    public static class CommandResultExtensions
    {
        [Obsolete("Use GetArgumentValueOrDefault instead. This method will be removed in a future version.")]
        public static object GetValueOrDefault(this CommandResult commandResult)
        {
            return commandResult.GetValueOrDefault<object>();
        }

        [Obsolete("Use GetArgumentValueOrDefault instead. This method will be removed in a future version.")]
        public static T GetValueOrDefault<T>(this CommandResult commandResult)
        {
            var argumentResult = commandResult.ArgumentResults.SingleOrDefault();
           
            return argumentResult
                   .GetValueAs(typeof(T))
                   .GetValueOrDefault<T>();
        }

        public static object GetArgumentValueOrDefault(
            this CommandResult commandResult,
            string argumentName)
        {
            return commandResult.GetArgumentValueOrDefault<object>(argumentName);
        }

        public static T GetArgumentValueOrDefault<T>(
            this CommandResult commandResult,
            string argumentName)
        {
            var argumentResult =
                commandResult.ArgumentResults
                             .SingleOrDefault(r => r.Argument.Name == argumentName);

            if (argumentResult == null)
            {
                // FIX: (GetValueOrDefault)    
            }

            return argumentResult.GetValueOrDefault<T>();
        }
    }
}
