// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;

namespace System.CommandLine.Binding
{
    public class ModelBinder : BinderBase
    {
        public ModelDescriptor ModelDescriptor { get; }

        public ModelBinder(ModelDescriptor modelDescriptor)
        {
            ModelDescriptor = modelDescriptor ?? throw new ArgumentNullException(nameof(modelDescriptor));
        }

        public ModelBinder(Type modelType) : this(ModelDescriptor.FromType(modelType))
        {
        }

        public IReadOnlyCollection<BoundValue> GetConstructorArguments(BindingContext context)
        {
            return GetValues(context, ConstructorDescriptor.ParameterDescriptors);
        }

        public object CreateInstance(BindingContext context)
        {
            IReadOnlyCollection<BoundValue> boundConstructorArguments = GetConstructorArguments(context);

            var values = boundConstructorArguments.Select(v => v.Value).ToArray();

            var instance = ConstructorDescriptor.Invoke(values);

            UpdateInstance(instance, context);

            return instance;
        }

        public void UpdateInstance<T>(T instance, BindingContext bindingContext)
        {
            SetProperties(bindingContext, instance);
        }

        private ConstructorDescriptor ConstructorDescriptor => ModelDescriptor.ConstructorDescriptors.Single();

        private void SetProperties(
            BindingContext context,
            object instance)
        {
            var boundValues = GetValues(
                context,
                ModelDescriptor.PropertyDescriptors,
                false);

            foreach (var boundValue in boundValues)
            {
                ((PropertyDescriptor)boundValue.ValueDescriptor).SetValue(instance, boundValue.Value);
            }
        }
    }
}
