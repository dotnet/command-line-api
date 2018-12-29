// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace System.CommandLine.Invocation
{
    public class TypeBinder : IOptionBuilder
    {
        private readonly Type _type;
        private IReadOnlyCollection<PropertyInfo> _settableProperties;
        private readonly ConstructorBinder _constructorBinder;

        public TypeBinder(
            Type type,
            ConstructorBinder constructorBinder = null)
        {
            _type = type ?? throw new ArgumentNullException(nameof(type));

            _constructorBinder =
                constructorBinder ??
                new ConstructorBinder(_type.GetConstructors().SingleOrDefault() ??
                                      throw new ArgumentException($"No eligible constructor found to bind type {_type}"));
        }

        public object CreateInstance(InvocationContext context)
        {
            var instance = _constructorBinder.InvokeMethod(context);

            SetProperties(context, instance);

            return instance;
        }

        public void SetProperties(
            InvocationContext context,
            object instance)
        {
            var commandResult = context.ParseResult.CommandResult;

            foreach (var propertyInfo in GetSettableProperties())
            {
                var typeToResolve = propertyInfo.PropertyType;

                var value = context.ServiceProvider.GetService(typeToResolve);

                if (value == null)
                {
                    var optionResult = Binder.FindMatchingOption(
                        commandResult,
                        propertyInfo.Name);

                    if (optionResult != null)
                    {
                        value = optionResult.GetValueOrDefault();
                    }
                    else if (propertyInfo.Name.IsMatch(commandResult.Command?.Argument?.Name))
                    {
                        value = commandResult.GetValueOrDefault();
                    }
                    else
                    {
                        continue;
                    }
                }

                propertyInfo.SetValue(instance, value);
            }
        }

        public IEnumerable<Option> BuildOptions()
        {
            var optionSet = new SymbolSet();

            foreach (var parameter in _constructorBinder.BuildOptions())
            {
                optionSet.Add(parameter);
            }

            foreach (var property in GetSettableProperties()
                .OmitInfrastructureTypes())
            {
                var option = property.BuildOption();

                if (!optionSet.Contains(option.Name))
                {
                    optionSet.Add(option);
                }
            }

            return optionSet.Cast<Option>();
        }

        private IEnumerable<PropertyInfo> GetSettableProperties()
        {
            return _settableProperties ??
                   (_settableProperties = _type.GetProperties().Where(p => p.CanWrite).ToArray());
        }
    }
}
