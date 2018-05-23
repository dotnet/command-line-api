namespace System.CommandLine
{
    public enum ResponseFileHandling
    {
        // Empty lines and lines beginning with # are skipped.

        // Each line in the file is treated as a single argument, regardless of whitespace on the line.
        ParseArgsAsLineSeparated,

        // Arguments are separated by whitespace (spaces and/or new-lines)
        ParseArgsAsSpaceSeparated,

        // Do not parse response files or treat arguments with &apos;@' as a response file
        Disabled
    }
}
