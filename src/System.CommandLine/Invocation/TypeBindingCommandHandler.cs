// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace System.CommandLine.Invocation
{
    internal class TypeBindingCommandHandler : CommandHandler
    {
        private readonly Type _type;
        private readonly MethodInfo _invocationTargetMethod;

        public TypeBindingCommandHandler(
            Type type,
            MethodInfo method)
        {
            _type = type ?? throw new ArgumentNullException(nameof(type));
            _invocationTargetMethod = method ?? throw new ArgumentNullException(nameof(method));
        }

        public override Task<int> InvokeAsync(InvocationContext context)
        {
            object instance = CreateInstanceOf(
                _type,
                context);

            object[] methodArgs = Binder.BindArguments(context, _invocationTargetMethod.GetParameters());

            object value = _invocationTargetMethod.Invoke(instance, methodArgs);

            return GetResultCodeAsync(value);
        }

        public static object CreateInstanceOf(
            Type type,
            InvocationContext context)
        {
            var ctor = type.GetConstructor();

            object[] constructorArgs = Binder.BindArguments(
                context,
                ctor.GetParameters());

            object instance = ctor.Invoke(constructorArgs);

            Binder.SetProperties(context, instance);

            return instance;
        }

        public IEnumerable<Option> BuildOptions()
        {
            var optionSet = new SymbolSet();

            foreach (var parameter in _type.GetConstructor().GetParameters())
            {
                optionSet.Add(parameter.BuildOption());
            }

            foreach (var property in _type.GetProperties()
                                          .Where(p => p.CanWrite))
            {
                var option = property.BuildOption();

                if (!optionSet.Contains(option.Name))
                {
                    optionSet.Add(option);
                }
            }

            return optionSet.Cast<Option>();
        }
    }
}
