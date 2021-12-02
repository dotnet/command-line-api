// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Binding;
using System.Linq.Expressions;

namespace System.CommandLine.NamingConventionBinder;

/// <inheritdoc />
public class ModelBinder<TModel> : ModelBinder
{
    /// <inheritdoc />
    public ModelBinder() : base(typeof(TModel))
    {
    }

    /// <summary>
    /// Configures a custom binding behavior for the specified property.
    /// </summary>
    /// <param name="property">An expression specifying the property to bind.</param>
    /// <param name="valueDescriptor">A value descriptor for the value from which the property will be bound.</param>
    /// <typeparam name="TValue">The type of the value to be bound.</typeparam>
    public void BindMemberFromValue<TValue>(
        Expression<Func<TModel, TValue>> property,
        IValueDescriptor valueDescriptor)
    {
        var (propertyType, propertyName) = property.MemberTypeAndName();
        var propertyDescriptor = FindModelPropertyDescriptor(
            propertyType, propertyName);
        MemberBindingSources[propertyDescriptor] = 
            new SpecificSymbolValueSource(valueDescriptor);
    }

    /// <summary>
    /// Configures a custom binding behavior for the specified property.
    /// </summary>
    /// <param name="property">An expression specifying the property to bind.</param>
    /// <param name="getValue">A delegate that gets the value to bind to the target property.</param>
    /// <typeparam name="TValue">The type of the target property.</typeparam>
    public void BindMemberFromValue<TValue>(
        Expression<Func<TModel, TValue>> property,
        Func<BindingContext?, TValue> getValue)
    {
        var (propertyType, propertyName) = property.MemberTypeAndName();
        var propertyDescriptor = FindModelPropertyDescriptor(
            propertyType, propertyName);
        MemberBindingSources[propertyDescriptor] =
            new DelegateValueSource(c => getValue(c));
    }
}