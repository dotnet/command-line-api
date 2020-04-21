namespace J4JSoftware.CommandLine
{
    public class OptionNotEqual<T> : IOptionValidator<T>
    {
        private T _checkValue;

        public OptionNotEqual(T checkValue)
        {
            _checkValue = checkValue;
        }

        public bool IsValid(T toCheck) => !_checkValue.Equals(toCheck);

        public string GetErrorMessage( string optionName, T toCheck) =>
            IsValid(toCheck) ? null : $"{optionName}: {toCheck} equals {_checkValue}";
    }
}