namespace System.CommandLine.Rendering
{
    public interface IConsoleWriter : IFormatProvider
    {
        void Write(object value);
        void WriteLine(object value);
        void WriteLine();

        string Format(object value);
    }
}
