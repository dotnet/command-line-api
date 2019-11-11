// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Linq.Expressions;

namespace System.CommandLine.Binding
{
    public class ModelBinder<TModel> : ModelBinder
    {
        public ModelBinder() : base(typeof(TModel))
        {
        }

        public void BindMemberFromValue<TValue>(
            Expression<Func<TModel, TValue>> property,
            IValueDescriptor valueDescriptor)
        {
            var key = property.MemberTypeAndName();
            if (NamedValueSources.TryGetValue(key, out var existingValueSource) &&
                existingValueSource is null)
            {
                // Override existing null value source
                NamedValueSources[key] = new SpecificSymbolValueSource(valueDescriptor);
            }
            else
            {
                NamedValueSources.Add(
                    key,
                    new SpecificSymbolValueSource(valueDescriptor)); 
            }
        }

        public void BindMemberFromValue<TValue>(
            Expression<Func<TModel, TValue>> member,
            Func<BindingContext, TValue> getValue)
        {
            var key = member.MemberTypeAndName();
            if (NamedValueSources.TryGetValue(key, out var existingValueSource) &&
                existingValueSource is null)
            {
                // Override existing null value source
                NamedValueSources[key] = new DelegateValueSource(c => getValue(c));
            }
            else
            {
                NamedValueSources.Add(
                    key,
                    new DelegateValueSource(c => getValue(c)));
            }
        }
    }
}
