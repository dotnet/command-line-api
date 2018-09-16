namespace System.CommandLine.Rendering
{
    internal static class StringExtensions
    {
        public static bool EndsWithWhitespace(this string value) =>
            value.Length > 0
            && Char.IsWhiteSpace(value[value.Length - 1]);

        public static bool StartsWithWhitespace(this string value) =>
            value.Length > 0
            && Char.IsWhiteSpace(value[0]);

        public static bool IsNewLine(this string value) => value == "\n" || value == "\r\n";
    }
}
