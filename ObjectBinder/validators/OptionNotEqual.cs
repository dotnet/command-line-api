namespace ObjectBinder
{
    public class OptionNotEqual<T> : IOptionValidator<T>
    {
        private T _checkValue;

        public OptionNotEqual(T checkValue)
        {
            _checkValue = checkValue;
        }

        public bool IsValid(T toCheck) => !_checkValue.Equals(toCheck);

        public string GetErrorMessage(T toCheck) =>
            IsValid(toCheck) ? null : $"{toCheck} equals {_checkValue}";
    }
}