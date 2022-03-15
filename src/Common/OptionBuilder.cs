// Copyright (c) .NET Foundation and contributors. All rights reserved.
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

    public static Option CreateOption(string name, Type valueType)
    {
        var optionType = typeof(Option<>).MakeGenericType(valueType);

#if NET6_0_OR_GREATER
            var ctor = (ConstructorInfo)optionType.GetMemberWithSameMetadataDefinitionAs(_ctor);
#else
        var ctor = optionType.GetConstructor(new[] { typeof(string), typeof(string) });
#endif

        var option = (Option)ctor.Invoke(new object[] { name, null });

        return option;
    }
}