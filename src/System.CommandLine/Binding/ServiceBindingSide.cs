// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.Binding
{
    public class ServiceBindingSide : BindingSide
    {
        private ServiceBindingSide(BindingGetter getter, BindingSetter setter)
           : base(getter, setter)
        { }

        public static ServiceBindingSide Create(Type serviceType)
             => new ServiceBindingSide((context, target) => context.ServiceProvider.GetService(serviceType),  null);

    }
}
