// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Binding;
using System.Linq;
using System.Threading;

namespace System.CommandLine.Invocation
{
    public class TypeBinder : IOptionBuilder
    {
        private static readonly HashSet<Type> _typesThatDoNotGenerateOptions = new HashSet<Type>(
            new[]
            {
                typeof(IConsole),
                typeof(BindingContext),
                typeof(InvocationContext),
                typeof(ParseResult),
                typeof(CancellationToken)
            }
        );

        private readonly ModelBinder _modelBinder;
        private readonly ModelDescriptor _modelDescriptor;

        public TypeBinder(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            var modelDescriptorType = typeof(ModelDescriptor<>).MakeGenericType(type);

            _modelDescriptor = (ModelDescriptor)Activator.CreateInstance(modelDescriptorType);

            _modelBinder = BindingExtensions.CreateBinder((dynamic)_modelDescriptor);
        }

        public object CreateInstance(InvocationContext context)
        {
            return _modelBinder.CreateInstance(context.BindingContext);
        }

        public IEnumerable<Option> BuildOptions()
        {
            var optionSet = new SymbolSet();

            if (_modelDescriptor.ConstructorDescriptors.Count == 1)
            {
                var ctorDescriptor = _modelDescriptor.ConstructorDescriptors[0];

                foreach (var parameterDescriptor in ctorDescriptor.ParameterDescriptors
                                                                  .Where(p => !_typesThatDoNotGenerateOptions.Contains(p.Type)))
                {
                    optionSet.Add(BuildOption(parameterDescriptor));
                }
            }

            foreach (var propertyDescriptor in _modelDescriptor.PropertyDescriptors
                                                               .Where(p => !_typesThatDoNotGenerateOptions.Contains(p.Type)))
            {
                var option = BuildOption(propertyDescriptor);

                if (!optionSet.Contains(option.Name))
                {
                    optionSet.Add(option);
                }
            }

            return optionSet.Cast<Option>();

            Option BuildOption(IValueDescriptor valueDescriptor)
            {
                var option = new Option(
                                 Binder.BuildAlias(valueDescriptor.Name),
                                 valueDescriptor.Name)
                             {
                                 Argument = new Argument
                                            {
                                                ArgumentType = valueDescriptor.Type
                                            }
                             };

                if (valueDescriptor.HasDefaultValue)
                {
                    option.Argument.SetDefaultValue(valueDescriptor.GetDefaultValue);
                }

                return option;
            }
        }
    }
}
