﻿// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reflection;

namespace System.CommandLine.Utility;

internal static class OptionBuilder
{
    private static readonly ConstructorInfo _ctor;

    static OptionBuilder()
    {
        _ctor = typeof(Option<string>).GetConstructor(new[] { typeof(string), typeof(string) });
    }

    public static Option CreateOption(string name, Type valueType, string description = null)
    {
        var optionType = typeof(Option<>).MakeGenericType(valueType);

#if NET6_0_OR_GREATER
        var ctor = (ConstructorInfo)optionType.GetMemberWithSameMetadataDefinitionAs(_ctor);
#else
        var ctor = optionType.GetConstructor(new[] { typeof(string), typeof(string) });
#endif

        var option = (Option)ctor.Invoke(new object[] { name, description });

        return option;
    }

    public static Option CreateOption(string name, Type valueType, string description, Func<object> defaultValueFactory)
    {
        if (defaultValueFactory == null)
        {
            return CreateOption(name, valueType, description);
        }

        var optionType = typeof(Bridge<>).MakeGenericType(valueType);

        var ctor = optionType.GetConstructor(new[] { typeof(string), typeof(Func<object>), typeof(string) });

        var option = (Option)ctor.Invoke(new object[] { name, defaultValueFactory, description });

        return option;
    }

    private class Bridge<T> : Option<T>
    {
        public Bridge(string name, Func<object> defaultValueFactory, string description)
            : base(name, 
                  () => (T)defaultValueFactory(), // this type exists only for an easy Func<object> => Func<T> transformation
                  description)
        {
        }
    }
}