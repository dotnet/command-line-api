// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.Binding
{
    internal static class ValueDescriptor
    {
        public static bool CanBind(
            IValueDescriptor from,
            IValueDescriptor to)
        {
            var namesMatch = from.Name == to.Name;

            if (!namesMatch)
            {
                return false;
            }

            var assignable = to.Type.IsAssignableFrom(from.Type);

            if (assignable)
            {
                return true;
            }

            return false;
        }
    }
}
