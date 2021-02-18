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

        public string ValueName => _parameterInfo.Name;

        public IMethodDescriptor Parent { get; }

        public Type ValueType => _parameterInfo.ParameterType;

        public bool HasDefaultValue => _parameterInfo.HasDefaultValue;

        public bool AllowsNull
        {
            get
            {
                if (_allowsNull is null)
                {
                    _allowsNull = CalculateAllowsNull(_parameterInfo);
                }
                return _allowsNull ?? false;
            }
        }

        public static bool CalculateAllowsNull(ParameterInfo parameterInfo) 
            => parameterInfo.ParameterType.IsNullable() ||
                    (parameterInfo.HasDefaultValue && parameterInfo.DefaultValue is null);

        public object? GetDefaultValue() =>
            _parameterInfo.DefaultValue is DBNull
                ? Binder.GetDefaultValue(ValueType)
                : _parameterInfo.DefaultValue;

        public override string ToString() => $"{ValueType.Name} {ValueName}";
    }
}