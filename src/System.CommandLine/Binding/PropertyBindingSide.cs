// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reflection;

namespace System.CommandLine.Binding
{
    public class PropertyBindingSide : BindingSide
    {
        private PropertyBindingSide(PropertyInfo propertyInfo)
            : base(GetGetter(propertyInfo), GetSetter(propertyInfo))
            => PropertyInfo = propertyInfo;

        public static PropertyBindingSide Create(PropertyInfo propertyInfo)
            => new PropertyBindingSide(propertyInfo);

        public PropertyInfo PropertyInfo { get; }

        private static BindingGetter GetGetter(PropertyInfo propertyInfo)
            => (context, target) => propertyInfo.GetValue(target);

        private static BindingSetter GetSetter(PropertyInfo propertyInfo)
            => (context, target, value) => propertyInfo.SetValue(target, value);
    }
}
