namespace System.CommandLine.Views
{
    public interface IConsoleWriter : IFormatProvider
    {
        void Write(object value);
        void WriteLine(object value);
        void WriteLine();
    }
}
