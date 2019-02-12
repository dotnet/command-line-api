// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.Binding
{
    public class ModelDescriptor<TModel> : ModelDescriptor
    {
        public ModelDescriptor() : base(typeof(TModel))
        {
        }
    }
}
