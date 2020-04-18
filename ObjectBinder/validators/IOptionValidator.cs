namespace ObjectBinder
{
    public interface IOptionValidator<in T>
    {
        bool IsValid( T toCheck );
        string GetErrorMessage( T toCheck );
    }
}