// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Binding;

namespace System.CommandLine.NamingConventionBinder;

internal class ModelBinderCollection
{
    private readonly Dictionary<Type, ModelBinder> _modelBindersByValueDescriptor = new();

    public void Add(ModelBinder binder)
    {
        _modelBindersByValueDescriptor.Add(binder.ValueDescriptor.ValueType, binder);
    }

    public ModelBinder GetModelBinder(IValueDescriptor valueDescriptor)
    {
        if (!_modelBindersByValueDescriptor.TryGetValue(valueDescriptor.ValueType, out var binder))
        {
            binder = new ModelBinder(valueDescriptor);
            _modelBindersByValueDescriptor.Add(valueDescriptor.ValueType, binder);
        }

        return binder;
    }
}