﻿// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace System.CommandLine.Binding
{
    public class ConstructorDescriptor : IMethodDescriptor
    {
        private List<ParameterDescriptor> _parameterDescriptors;

        private readonly ConstructorInfo _constructorInfo;

        internal ConstructorDescriptor(
            ConstructorInfo constructorInfo,
            ModelDescriptor parent)
        {
            Parent = parent;
            _constructorInfo = constructorInfo;
        }

        public ModelDescriptor Parent { get; }

        public IReadOnlyList<ParameterDescriptor> ParameterDescriptors =>
            _parameterDescriptors
            ??
            (_parameterDescriptors = _constructorInfo.GetParameters().Select(p => new ParameterDescriptor(p, this)).ToList());

        internal object Invoke(IReadOnlyCollection<object> parameters)
        {
            return _constructorInfo.Invoke(parameters.ToArray());
        }

        public override string ToString() =>
            $"{Parent} ({string.Join(", ", ParameterDescriptors)})";
    }
}