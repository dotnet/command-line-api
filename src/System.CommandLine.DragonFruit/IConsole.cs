using System.IO;

namespace System.CommandLine.DragonFruit
{
    /// <summary>
    /// An abstract console for testing purposes only.
    /// </summary>
    internal interface IConsole
    {
        TextWriter Out { get; }

        TextWriter Error { get; }

        ConsoleColor ForegroundColor { get; set; }

        void ResetColor();
    }
}
