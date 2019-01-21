// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace System.CommandLine.Invocation
{
    public interface IBinder
    {
        object GetTarget(InvocationContext context);
        void SetTarget(object target);
        void AddBinding(Binding binding);
        void AddBinding(BindingSide targetSide, BindingSide parserSide);
    }
}
