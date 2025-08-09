
using System;
using System.CommandLine;
using System.Reflection;

internal static class ArgumentBuilder
{

    internal static Argument CreateArgument(ParameterInfo argsParam)
    {
        if (!argsParam.HasDefaultValue)
        {
            var genType = typeof(Argument<>).MakeGenericType(argsParam.ParameterType);
            var ctor = genType.GetConstructor(new[] { typeof(string) });
            return (Argument)ctor.Invoke(new object[] { argsParam.Name });
        }

        var argumentType = typeof(Bridge<>).MakeGenericType(argsParam.ParameterType);
        var ctorWithDefault = argumentType.GetConstructor(new[] { typeof(string), argsParam.ParameterType });
        return (Argument)ctorWithDefault.Invoke(new object[] { argsParam.Name, argsParam.DefaultValue });
    }

    internal static Argument<T> CreateArgument<T>()
    {
        return new Argument<T>(typeof(T).Name.ToLowerInvariant());
    }

    internal static Argument CreateArgument(Type type)
    {
        var genType = typeof(Argument<>).MakeGenericType(type);
        var ctor = genType.GetConstructor(new[] { typeof(string) });
        return (Argument)ctor.Invoke(new object[] { type.Name.ToLowerInvariant() });
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