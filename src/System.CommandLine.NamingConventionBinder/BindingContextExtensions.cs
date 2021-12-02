// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Binding;

namespace System.CommandLine.NamingConventionBinder;

/// <summary>
/// Methods for working with binding contexts.
/// </summary>
public static class BindingContextExtensions
{
    /// <summary>
    /// Adds a model binder which can be used to bind a specific type.
    /// </summary>
    /// <param name="bindingContext">The binding context for the current binding operation.</param>
    /// <param name="binder">The model binder to add.</param>
    public static void AddModelBinder(
        this BindingContext bindingContext,
        ModelBinder binder)
    {
        var modelBinders = GetModelBinderCollection(bindingContext);

        modelBinders.Add(binder);
    }

    /// <summary>
    /// Gets a model binder for the specified value descriptor.
    /// </summary>
    /// <returns>A model binder for the specified value descriptor.</returns>
    public static ModelBinder GetOrCreateModelBinder(
        this BindingContext bindingContext,
        IValueDescriptor valueDescriptor) =>
        GetModelBinderCollection(bindingContext).GetModelBinder(valueDescriptor);

    private static ModelBinderCollection GetModelBinderCollection(BindingContext bindingContext)
    {
        if (bindingContext.GetService(typeof(ModelBinderCollection)) is not ModelBinderCollection modelBinders)
        {
            modelBinders = new ModelBinderCollection();
            bindingContext.AddService(typeof(ModelBinderCollection), _ => modelBinders);
        }

        return modelBinders;
    }
}