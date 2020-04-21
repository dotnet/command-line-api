namespace J4JSoftware.CommandLine
{
    public interface IOptionValidator<in T>
    {
        bool IsValid( T toCheck );
        string GetErrorMessage( string optionName, T toCheck );
    }
}