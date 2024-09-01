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
        ValueSource = valueSource;
    }

    /// <summary>
    /// Create a CalculatedValue.
    /// </summary>
    /// <typeparam name="T">The type of the value that can be retrieved via this calculated value.</typeparam>
    // TODO: Provide name lookup of CalculatedValues
    /// <param name="name">The name of the calculated value</param>
    /// <param name="valueSource">The ValueSource used to retrieve the value. If there are defaults/fallbacks, this will be an aggregate value source.</param>
    /// <returns></returns>
    public static CalculatedValue<T> CreateCalculatedValue<T>(string name, ValueSource<T> valueSource)
       => new(name, valueSource);

    // TODO: This feels backwards. Should probably appear on the string array used as the data source.
    public IEnumerable<CliValueSymbol>? AppearAs {  get; set; }

    /// <summary>
    /// The ValueSource used to retrieve the value. If there are defaults/fallbacks, this will be an
    /// aggregate value source.
    /// </summary>
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
    
    /// <inheritdoc/>
    public override Type ValueType { get; } = typeof(T);
}
