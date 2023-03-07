
using System;
using System.CommandLine;
using System.Reflection;

internal static class ArgumentBuilder
{
    private static readonly ConstructorInfo _ctor;

    static ArgumentBuilder()
    {
        _ctor = typeof(Argument<string>).GetConstructor(new[] { typeof(string) });
    }

    public static Argument CreateArgument(Type valueType, string name = "value")
    {
        var argumentType = typeof(Argument<>).MakeGenericType(valueType);

#if NET6_0_OR_GREATER
        var ctor = (ConstructorInfo)argumentType.GetMemberWithSameMetadataDefinitionAs(_ctor);
#else
        var ctor = argumentType.GetConstructor(new[] { typeof(string) });
#endif

        return (Argument)ctor.Invoke(new object[] { name });
    }

    internal static Argument CreateArgument(ParameterInfo argsParam)
    {
        if (!argsParam.HasDefaultValue)
        {
            return CreateArgument(argsParam.ParameterType, argsParam.Name);
        }

        var argumentType = typeof(Bridge<>).MakeGenericType(argsParam.ParameterType);

        var ctor = argumentType.GetConstructor(new[] { typeof(string), argsParam.ParameterType });

        return (Argument)ctor.Invoke(new object[] { argsParam.Name, argsParam.DefaultValue });
    }

    private sealed class Bridge<T> : Argument<T>
    {
        public Bridge(string name, T defaultValue)
            : base(name)
        {
            // this type exists only for an easy T => Func<ArgumentResult, T> transformation
            DefaultValueFactory = (_) => defaultValue;
        }
    }
}