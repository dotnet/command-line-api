namespace System.CommandLine.Binding;

/// <summary>
/// Supports binding of custom types.
/// </summary>
/// <typeparam name="T">The type to be bound.</typeparam>
/// <seealso href="/dotnet/standard/commandline/model-binding#model-binding-more-than-16-options-and-arguments">Model binding more than 16 options and arguments</seealso>

public abstract class BinderBase<T> :
    IValueDescriptor<T>,
    IValueSource
{
    /// <summary>
    /// Gets a value from the binding context.
    /// </summary>
    /// <param name="bindingContext"></param>
    /// <returns></returns>
    /// <seealso href="/dotnet/standard/commandline/model-binding#model-binding-more-than-16-options-and-arguments">Model binding more than 16 options and arguments</seealso>
    protected abstract T GetBoundValue(BindingContext bindingContext);

    string IValueDescriptor.ValueName => GetType().Name;

    Type IValueDescriptor.ValueType => typeof(T);

    bool IValueDescriptor.HasDefaultValue => false;

    object? IValueDescriptor.GetDefaultValue() => default(T);

    bool IValueSource.TryGetValue(IValueDescriptor valueDescriptor, BindingContext? bindingContext, out object? boundValue)
    {
        if (bindingContext is null)
        {
            boundValue = default;
            return false;
        }

        boundValue = GetBoundValue(bindingContext);
        return true;
    }
}