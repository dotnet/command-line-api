// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reflection;
using System.Threading.Tasks;

namespace System.CommandLine.Invocation
{
    internal class ConstructorBindingCommandHandler : CommandHandler
    {
        private readonly ConstructorInfo constructorInfo;
        private readonly MethodInfo _method;

        public ConstructorBindingCommandHandler(ConstructorInfo constructorInfo, MethodInfo method) 
        {
            this.constructorInfo = constructorInfo ?? throw new ArgumentNullException(nameof(constructorInfo));
            _method = method ?? throw new ArgumentNullException(nameof(method));
        }

        public override Task<int> InvokeAsync(InvocationContext context)
        {

            object[] constructorArgs = Binder.BindArguments(context, constructorInfo.GetParameters());

            object instance = constructorInfo.Invoke(constructorArgs);

            Binder.SetProperties(context, instance);

            object[] methodArgs = Binder.BindArguments(context, _method.GetParameters());

            object value = _method.Invoke(instance, methodArgs);

            return CoerceResultAsync(value);
        }
    }
}
