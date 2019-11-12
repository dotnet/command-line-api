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
            NamedValueSources[property.MemberTypeAndName()] = 
                new SpecificSymbolValueSource(valueDescriptor);
        }

        public void BindMemberFromValue<TValue>(
            Expression<Func<TModel, TValue>> member,
            Func<BindingContext, TValue> getValue)
        {
            NamedValueSources[member.MemberTypeAndName()] = 
                new DelegateValueSource(c => getValue(c));
        }
    }
}
