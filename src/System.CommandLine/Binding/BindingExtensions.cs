// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.Binding
{
    public static class BindingExtensions
    {
        public static ModelBinder<T> CreateBinder<T>(
            this ModelDescriptor<T> modelDescriptor)
        {
            return new ModelBinder<T>();
        }
    }
}
