// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Reflection;

namespace System.CommandLine.Binding
{
    public interface IMethodDescriptor
    {
        MethodBase MethodInfo { get; } 

        ModelDescriptor? Parent { get; }

        IReadOnlyList<ParameterDescriptor> ParameterDescriptors { get; }
    }
}
