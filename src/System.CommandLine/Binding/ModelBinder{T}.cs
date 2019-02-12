// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Linq.Expressions;

namespace System.CommandLine.Binding
{
    public class ModelBinder<TModel> : ModelBinder
    {
        public ModelBinder() : base(ModelDescriptor.FromType<TModel>())
        {
        }

        public void BindProperty<TValue>(
            Expression<Func<TModel, TValue>> property,
            IValueDescriptor valueDescriptor)
        {

        }

        public void BindProperty<TValue>(
            Expression<Func<TModel, TValue>> property,
            Func<TValue> valueSource)
        {

        }

        public void BindPropertyOrParameter(

            Func<BoundValue> valueSource)
        {

        }
    }
}
