// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reflection;

namespace System.CommandLine.Binding
{
    public class ParameterDescriptor : IValueDescriptor
    {
        private readonly ParameterInfo _parameterInfo;
        private bool? _allowsNull;

        internal ParameterDescriptor(
            ParameterInfo parameterInfo,
            IMethodDescriptor parent)
        {
            Parent = parent;
            _parameterInfo = parameterInfo;
        }

        public string Name => _parameterInfo.Name;

        public IMethodDescriptor Parent { get; }

        public Type Type => _parameterInfo.ParameterType;

        public bool HasDefaultValue => _parameterInfo.HasDefaultValue;

        public bool AllowsNull
        {
            get
            {
                if (_allowsNull == null)
                {
                    if (_parameterInfo.ParameterType.IsNullable())
                    {
                        _allowsNull = true;
                    }
                }

                return _allowsNull ?? false;
            }
        }

        public object GetDefaultValue() =>
            _parameterInfo.DefaultValue is DBNull
                ? Type.GetDefaultValueForType()
                : _parameterInfo.DefaultValue;

        public override string ToString() => $"{Type.Name} {Name}";
    }
}
