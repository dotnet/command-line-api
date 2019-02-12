// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace System.CommandLine.Binding
{
    public class ConstructorDescriptor
    {
        private readonly List<ParameterDescriptor> _parameterDescriptors = new List<ParameterDescriptor>();

        private readonly ConstructorInfo _constructorInfo;

        internal ConstructorDescriptor(ConstructorInfo constructorInfo)
        {
            _constructorInfo = constructorInfo;

            foreach (var parameterInfo in _constructorInfo.GetParameters())
            {
                _parameterDescriptors.Add(new ParameterDescriptor(parameterInfo));
            }
        }

        public IReadOnlyList<ParameterDescriptor> ParameterDescriptors => _parameterDescriptors;

        internal object Invoke(IReadOnlyCollection<object> parameters)
        {
            return _constructorInfo.Invoke(parameters.ToArray());
        }
    }
}
