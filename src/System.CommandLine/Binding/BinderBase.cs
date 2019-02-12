// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace System.CommandLine.Binding
{
    public abstract class BinderBase
    {
        protected IReadOnlyCollection<BoundValue> GetValues(
            BindingContext context,
            IReadOnlyList<IValueDescriptor> valueDescriptors,
            bool includeMissingValues = true)
        {
            var values = new List<BoundValue>();

            for (var index = 0; index < valueDescriptors.Count; index++)
            {
                var valueDescriptor = valueDescriptors[index];

                if (context.TryBind(valueDescriptor, out var value))
                {
                    values.Add(value);
                }
                else if (includeMissingValues)
                {
                    values.Add(BoundValue.DefaultForType(valueDescriptor));
                }
            }

            return values;
        }
    }
}
