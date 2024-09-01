using System.CommandLine.ValueSources;

namespace System.CommandLine;

/// <summary>
/// CalculatedValueSymbol lets ValueSource contribute to the data space of the application
/// with many standard features such as default values and the ability to participate in help.
/// </summary>
/// <remarks>
/// Known scenarios:
/// <list type=">">
/// <item>`dotnet nuget why`: where for historic reasons the first argument is optional and the second is required.</item>
/// <item>Completions: where the number of items to be displayed may come from an environment variable or a config file.</item>
/// <item>Compound types: where multiple value symbols contribute to the creation of a type, like -x and -y creating a point.</item>
/// <item>Reusing a value, for example to avoid multiple reads of a file</item>
/// </list>
/// CalculatedValueSymbol's should be available to subsystems, including invocation and conditions.
/// </remarks>
public abstract class CalculatedValue : CliValueSymbol
{
    protected CalculatedValue(string name, ValueSource valueSource)
        :base(name)
    {
        //Name = name;
        ValueSource = valueSource;
    }

    public static CalculatedValue<T> CreateCalculatedValue<T>(string name, ValueSource<T> valueSource)
       => new(name, valueSource);

    ///// <summary>
    ///// Gets the name of the symbol.
    ///// </summary>
    //public string Name { get; }

    ///// <summary>
    ///// Gets or sets the <see cref="Type" /> that the argument's parsed tokens will be converted to.
    ///// </summary>
    //public abstract Type ValueType { get; }

    public IEnumerable<CliValueSymbol>? AppearAs {  get; set; }

    public ValueSource ValueSource { get; }

    internal bool TryGetValue<T>(PipelineResult pipelineResult, out T? calculatedValue)
    {
        if (ValueSource.TryGetValue(pipelineResult, out object? objectValue))
        {
            calculatedValue = (T?)objectValue;
            return true;
        }
        calculatedValue = default;
        return false;
    }
}

/// <summary>
/// CalculatedValueSymbol lets ValueSource contribute to the data space of the application
/// with many standard features such as default values and the ability to participate in help.
/// </summary>
/// <typeparam name="T">The value type of the symbol.</typeparam>
public class CalculatedValue<T> : CalculatedValue
{
    internal CalculatedValue(string name, ValueSource<T> valueSource)
        : base(name, valueSource)
    { } 
    
    public override Type ValueType { get; } = typeof(T);
}
