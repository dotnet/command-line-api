// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace System.CommandLine.NamingConventionBinder.Tests;

public class BindingTestSet : Dictionary<(Type type, string variationName), BindingTestCase>
{
    public void Add(BindingTestCase testCase)
    {
        Add((testCase.ParameterType, testCase.VariationName), testCase);
    }

    public BindingTestCase this[Type type] => base[(type, null)];
}