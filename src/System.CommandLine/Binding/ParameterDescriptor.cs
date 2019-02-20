// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reflection;

namespace System.CommandLine.Binding
{
    public class ParameterDescriptor : IValueDescriptor
    {
        private readonly ParameterInfo _parameterInfo;

        public ParameterDescriptor(ParameterInfo parameterInfo)
        {
            _parameterInfo = parameterInfo;
        }

        public string Name => _parameterInfo.Name;

        public Type Type => _parameterInfo.ParameterType;

        public bool HasDefaultValue => _parameterInfo.HasDefaultValue;

        public object GetDefaultValue()
        {
            return _parameterInfo.DefaultValue;
        }
    }
}
