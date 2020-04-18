namespace ObjectBinder
{
    public class OptionEqual<T> : IOptionValidator<T>
    {
        private T _checkValue;

        public OptionEqual( T checkValue )
        {
            _checkValue = checkValue;
        }

        public bool IsValid( T toCheck ) => _checkValue.Equals( toCheck );

        public string GetErrorMessage( T toCheck ) =>
            IsValid( toCheck ) ? null : $"{toCheck} doesn't equal {_checkValue}";
    }
}