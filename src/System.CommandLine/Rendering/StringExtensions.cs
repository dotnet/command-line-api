namespace System.CommandLine.Rendering
{
    internal static class StringExtensions
    {
        public static bool EndsWithWhitespace(this string value) =>
            value.Length > 0
            && char.IsWhiteSpace(value[value.Length - 1]);

        public static bool StartsWithWhitespace(this string value) =>
            value.Length > 0
            && char.IsWhiteSpace(value[0]);
    }
}
