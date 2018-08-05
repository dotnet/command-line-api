using System.Linq;

namespace System.CommandLine.Rendering
{
    public static class ConsoleWriterExtensions
    {
        public static void WriteLine(this ConsoleWriter writer)
        {
            switch (writer.Mode)
            {
                case OutputMode.NonAnsi:
                    writer.Console.Out.WriteLine();
                    break;
                case OutputMode.Ansi:
                    writer.Console.Out.Write(Ansi.Cursor.Move.Down());
                    writer.Console.Out.Write(Ansi.Cursor.Move.NextLine(1));
                    break;
                case OutputMode.File:
                    writer.Console.Out.WriteLine();
                    break;
            }
        }
    }
}
