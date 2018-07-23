using System;
using System.IO;

namespace System.CommandLine.Rendering
{
    public static class Ansi
    {
        public static class Text
        {
            public static string BlinkOff { get; } = $"{Esc}[25m";
            public static string BlinkOn { get; } = $"{Esc}[5m";
            public static string BoldOff { get; } = $"{Esc}[22m";
            public static string BoldOn { get; } = $"{Esc}[1m";
            public static string HiddenOn { get; } = $"{Esc}[8m";
            public static string ReverseOn { get; } = $"{Esc}[7m";
            public static string ReversOff { get; } = $"{Esc}[27m";
            public static string StandoutOff { get; } = $"{Esc}[23m";
            public static string StandoutOn { get; } = $"{Esc}[3m";
            public static string UnderlinedOff { get; } = $"{Esc}[24m";
            public static string UnderlinedOn { get; } = $"{Esc}[4m";
        }

        public static class Color
        {
            public static string Off { get; } = $"{Esc}[0m";

            public class Background
            {
                public static string Default => $"{Esc}[49m";

                public static string Black { get; } = $"{Esc}[40m";
                public static string Red { get; } = $"{Esc}[41m";
                public static string Green { get; } = $"{Esc}[42m";
                public static string Yellow { get; } = $"{Esc}[43m";
                public static string Blue { get; } = $"{Esc}[44m";
                public static string Magenta { get; } = $"{Esc}[45m";
                public static string Cyan { get; } = $"{Esc}[46m";
                public static string White { get; } = $"{Esc}[47m";
                public static string DarkGray { get; } = $"{Esc}[100m";
                public static string LightRed { get; } = $"{Esc}[101m";
                public static string LightGreen { get; } = $"{Esc}[102m";
                public static string LightYellow { get; } = $"{Esc}[103m";
                public static string LightBlue { get; } = $"{Esc}[104m";
                public static string LightMagenta { get; } = $"{Esc}[105m";
                public static string LightCyan { get; } = $"{Esc}[106m";
                public static string LightGray { get; } = $"{Esc}[107m";

                public static string Rgb(byte r, byte g, byte b) => $"{Esc}[48;2;{r};{g};{b}m";
            }

            public static class Foreground
            {
                public static string Default => $"{Esc}[39m";

                public static string Black { get; } = $"{Esc}[30m";
                public static string Red { get; } = $"{Esc}[31m";
                public static string Green { get; } = $"{Esc}[32m";
                public static string Yellow { get; } = $"{Esc}[33m";
                public static string Blue { get; } = $"{Esc}[34m";
                public static string Magenta { get; } = $"{Esc}[35m";
                public static string Cyan { get; } = $"{Esc}[36m";
                public static string White { get; } = $"{Esc}[37m";
                public static string DarkGray { get; } = $"{Esc}[90m";
                public static string LightRed { get; } = $"{Esc}[91m";
                public static string LightGreen { get; } = $"{Esc}[92m";
                public static string LightYellow { get; } = $"{Esc}[93m";
                public static string LightBlue { get; } = $"{Esc}[94m";
                public static string LightMagenta { get; } = $"{Esc}[95m";
                public static string LightCyan { get; } = $"{Esc}[96m";
                public static string LightGray { get; } = $"{Esc}[97m";

                public static string Rgb(byte r, byte g, byte b) => $"{Esc}[38;2;{r};{g};{b}m";
            }
        }

        public static class Cursor
        {
            public static class Move
            {
                public static string Up(int lines) => $"{Esc}[{lines}A";
                public static string Down(int lines) => $"{Esc}[{lines}B";
                public static string Right(int columns) => $"{Esc}[{columns}C";
                public static string Left(int columns) => $"{Esc}[{columns}D";
                public static string NextLine(int columns) => $"{Esc}E";
                public static string ToUpperLeftCorner { get; } = $"{Esc}[H";
                public static string ToLocation(int? line = null, int? column = null) => $"{Esc}[{line};{column}H";
            }

            public class Scroll
            {
                public static string UpOne { get; } = $"{Esc}D";

                public static string DownOne { get; } = $"{Esc}M";
            }

            public static string Hide { get; } = $"{Esc}[?25l";

            public static string Show { get; } = $"{Esc}[?25h";

            public static string SavePositionAndAttributes { get; } = $"{Esc}7";

            public static string SavePosition { get; } = $"{Esc}[s";

            public static string RestorePositionAndAttributes { get; } = $"{Esc}8";

            public static string RestorePosition { get; } = $"{Esc}[u";
        }

        public static class Clear
        {
            public static string EntireScreen { get; } = $"{Esc}[2J";
            public static string Line { get; } = $"{Esc}[2K";
            public static string ToBeginningOfLine { get; } = $"{Esc}[1K";
            public static string ToBeginningOfScreen { get; } = $"{Esc}[1J";
            public static string ToEndOfLine { get; } = $"{Esc}[K";
            public static string ToEndOfScreen { get; } = $"{Esc}[J";
        }

        public const string Esc = "\x1b";

        public static void WriteAt(
            this TextWriter writer,
            string value,
            int? line = null,
            int? column = null)
        {
            writer.Write(Cursor.SavePosition);

            writer.Write(Cursor.Move.ToLocation(line, column));

            writer.Write(value);

            writer.Write(Cursor.RestorePosition);
        }
    }
}
