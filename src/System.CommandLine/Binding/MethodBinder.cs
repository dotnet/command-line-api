using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace System.CommandLine.Binding
{
    public class MethodBinder
    {
        public MethodBinder(MethodInfo methodInfo) : this(new MethodInfoHandlerDescriptor(methodInfo))
        {
            if (methodInfo is null)
            {
                throw new ArgumentNullException(nameof(methodInfo));
            }
        }

        public MethodBinder(IMethodDescriptor methodDescriptor)
        {
            MethodDescriptor = methodDescriptor;
        }

        public IMethodDescriptor MethodDescriptor { get; }

        public bool EnforceExplicitBinding { get; set; }

        internal Dictionary<IValueDescriptor, Func<BindingContext, ModelBinder>> ParameterBindingSources { get; } =
            new Dictionary<IValueDescriptor, Func<BindingContext, ModelBinder>>();

        protected IValueDescriptor GetParameterDescriptor(
            Type parameterType, string parameterName)
        {
            return MethodDescriptor.ParameterDescriptors
                .FirstOrDefault(desc =>
                    desc.ValueType == parameterType &&
                    string.Equals(desc.ValueName, parameterName, StringComparison.Ordinal)
                    );
        }

        internal object?[] GetInvocationArguments(BindingContext bindingContext)
        {
            var parameterBinders = MethodDescriptor.ParameterDescriptors
                           .Select(p => GetModelBinder(p, ParameterBindingSources, bindingContext))
                           .ToList();

            return parameterBinders
                    .Select(parameterBinder => parameterBinder.CreateInstance(bindingContext))
                    .ToArray();

            static ModelBinder GetModelBinder(ParameterDescriptor p, Dictionary<IValueDescriptor, Func<BindingContext, ModelBinder>> parameterBindingSources, BindingContext bindingContext)
            {
                if (parameterBindingSources.TryGetValue(p, out var valueSource))
                {
                    return valueSource(bindingContext);
                }
                return bindingContext.GetModelBinder(p);
            }
        }
        //public void BindParameterFromValue(ParameterInfo parameter, IValueDescriptor valueDescriptor, Type modelType)
        //{
        //    if (!(parameter.Member is MethodInfo methodInfo))
        //        throw new ArgumentException(paramName: nameof(parameter),
        //            message: "Parameter must be declared on a method that is not a constructor.");

        //    var paramDesc = MethodDescriptor.ParameterDescriptors[parameter.Position];
        //   // ParameterBindingSources[paramDesc] = bindingContext => bindingContext.GetModelBinder(modelType);
        //}

        public void BindParameterFromValue(ParameterInfo parameter, IValueDescriptor valueDescriptor)
        {
            if (!(parameter.Member is MethodInfo methodInfo))
                throw new ArgumentException(paramName: nameof(parameter),
                    message: "Parameter must be declared on a method that is not a constructor.");

            var paramDesc = MethodDescriptor.ParameterDescriptors[parameter.Position];
            ParameterBindingSources[paramDesc] = bindingContext => bindingContext.GetModelBinder(valueDescriptor);
        }

        //private IReadOnlyList<BoundValue> GetValues(
        //    IDictionary<IValueDescriptor, IValueSource>? bindingSources,
        //    BindingContext bindingContext,
        //    IReadOnlyList<IValueDescriptor> valueDescriptors,
        //    bool includeMissingValues)
        //{
        //    var values = new List<BoundValue>();

        //    for (var index = 0; index < valueDescriptors.Count; index++)
        //    {
        //        var valueDescriptor = valueDescriptors[index];

        //        var valueSource = GetValueSource(bindingSources, bindingContext, valueDescriptor);

        //        BoundValue? boundValue;
        //        if (valueSource is null)
        //        {
        //            // If there is no source to bind from, no value can be bound.
        //            boundValue = null;
        //        }
        //        else if (!bindingContext.TryBindToScalarValue(
        //                valueDescriptor,
        //                valueSource,
        //                out boundValue) && valueDescriptor.HasDefaultValue)
        //        {
        //            boundValue = BoundValue.DefaultForValueDescriptor(valueDescriptor);
        //        }

        //        if (boundValue is null)
        //        {
        //            if (includeMissingValues)
        //            {
        //                if (valueDescriptor is ParameterDescriptor parameterDescriptor &&
        //                    parameterDescriptor.Parent is ConstructorDescriptor constructorDescriptor)
        //                {
        //                    if (parameterDescriptor.HasDefaultValue)
        //                        boundValue = BoundValue.DefaultForValueDescriptor(parameterDescriptor);
        //                    else if (parameterDescriptor.AllowsNull &&
        //                        ShouldPassNullToConstructor(constructorDescriptor.Parent, constructorDescriptor))
        //                        boundValue = BoundValue.DefaultForType(valueDescriptor);
        //                }
        //            }
        //        }

        //        if (boundValue != null)
        //        {
        //            values.Add(boundValue);
        //        }
        //    }

        //    return values;
        //}

        //private IValueSource? GetValueSource(
        //    IDictionary<IValueDescriptor, IValueSource>? bindingSources,
        //    BindingContext bindingContext,
        //    IValueDescriptor valueDescriptor)
        //{
        //    if (!(bindingSources is null) &&
        //        bindingSources.TryGetValue(valueDescriptor, out IValueSource? valueSource))
        //    {
        //        return valueSource;
        //    }

        //    if (bindingContext.TryGetValueSource(valueDescriptor, out valueSource))
        //    {
        //        return valueSource;
        //    }

        //    if (!EnforceExplicitBinding)
        //    {
        //        // Return a value source that will match from the parseResult
        //        // by name and type (or a possible conversion)
        //        return new ParseResultMatchingValueSource();
        //    }

        //    return null;
        //}

        public override string ToString() =>
            $"{MethodDescriptor.MethodInfo.Name }";

    }
}
