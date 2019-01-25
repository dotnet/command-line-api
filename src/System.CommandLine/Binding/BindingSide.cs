// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.Binding
{
    public abstract class BindingSide
    {
        public BindingSide(BindingGetter get, BindingSetter set)
        {
            Set = set;
            Get = get;
        }
        public BindingSetter Set { get; }
        public BindingGetter Get { get; }
    }
}
