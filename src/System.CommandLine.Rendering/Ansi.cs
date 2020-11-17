// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics;

namespace System.CommandLine.Rendering
{
    [DebuggerStepThrough]
    public static class Ansi
    {
        [DebuggerStepThrough]
        public static class Text
        {
            public static AnsiControlCode AttributesOff { get; } = $"{Esc}[0m";
            public static AnsiControlCode BlinkOff { get; } = $"{Esc}[25m";
            public static AnsiControlCode BlinkOn { get; } = $"{Esc}[5m";
            public static AnsiControlCode BoldOff { get; } = $"{Esc}[22m";
            public static AnsiControlCode BoldOn { get; } = $"{Esc}[1m";
            public static AnsiControlCode HiddenOn { get; } = $"{Esc}[8m";
            public static AnsiControlCode ReverseOn { get; } = $"{Esc}[7m";
            public static AnsiControlCode ReverseOff { get; } = $"{Esc}[27m";
            public static AnsiControlCode StandoutOff { get; } = $"{Esc}[23m";
            public static AnsiControlCode StandoutOn { get; } = $"{Esc}[3m";
            public static AnsiControlCode UnderlinedOff { get; } = $"{Esc}[24m";
            public static AnsiControlCode UnderlinedOn { get; } = $"{Esc}[4m";
        }

        [DebuggerStepThrough]
        public static class Color
        {
            [DebuggerStepThrough]
            public static class Background
            {
                public static AnsiControlCode Default { get; } = $"{Esc}[49m";

                public static AnsiControlCode Black => $"{Esc}[40m";

                public static AnsiControlCode Red { get; } = $"{Esc}[41m";
                public static AnsiControlCode Green { get; } = $"{Esc}[42m";
                public static AnsiControlCode Yellow { get; } = $"{Esc}[43m";
                public static AnsiControlCode Blue { get; } = $"{Esc}[44m";
                public static AnsiControlCode Magenta { get; } = $"{Esc}[45m";
                public static AnsiControlCode Cyan { get; } = $"{Esc}[46m";
                public static AnsiControlCode White { get; } = $"{Esc}[47m";
                public static AnsiControlCode DarkGray { get; } = $"{Esc}[100m";
                public static AnsiControlCode LightRed { get; } = $"{Esc}[101m";
                public static AnsiControlCode LightGreen { get; } = $"{Esc}[102m";
                public static AnsiControlCode LightYellow { get; } = $"{Esc}[103m";
                public static AnsiControlCode LightBlue { get; } = $"{Esc}[104m";
                public static AnsiControlCode LightMagenta { get; } = $"{Esc}[105m";
                public static AnsiControlCode LightCyan { get; } = $"{Esc}[106m";
                public static AnsiControlCode LightGray { get; } = $"{Esc}[107m";

                public static AnsiControlCode Rgb(byte r, byte g, byte b) => $"{Esc}[48;2;{r.ToString()};{g.ToString()};{b.ToString()}m";
            }

            [DebuggerStepThrough]
            public static class Foreground
            {
                public static AnsiControlCode Default => $"{Esc}[39m";

                public static AnsiControlCode Black { get; } = $"{Esc}[30m";
                public static AnsiControlCode Red { get; } = $"{Esc}[31m";
                public static AnsiControlCode Green { get; } = $"{Esc}[32m";
                public static AnsiControlCode Yellow { get; } = $"{Esc}[33m";
                public static AnsiControlCode Blue { get; } = $"{Esc}[34m";
                public static AnsiControlCode Magenta { get; } = $"{Esc}[35m";
                public static AnsiControlCode Cyan { get; } = $"{Esc}[36m";
                public static AnsiControlCode White { get; } = $"{Esc}[37m";
                public static AnsiControlCode DarkGray { get; } = $"{Esc}[90m";
                public static AnsiControlCode LightRed { get; } = $"{Esc}[91m";
                public static AnsiControlCode LightGreen { get; } = $"{Esc}[92m";
                public static AnsiControlCode LightYellow { get; } = $"{Esc}[93m";
                public static AnsiControlCode LightBlue { get; } = $"{Esc}[94m";
                public static AnsiControlCode LightMagenta { get; } = $"{Esc}[95m";
                public static AnsiControlCode LightCyan { get; } = $"{Esc}[96m";
                public static AnsiControlCode LightGray { get; } = $"{Esc}[97m";

                public static AnsiControlCode Rgb(byte r, byte g, byte b) => $"{Esc}[38;2;{r.ToString()};{g.ToString()};{b.ToString()}m";
            }
        }

        // see: https://docs.microsoft.com/en-us/windows/console/console-virtual-terminal-sequences
        [DebuggerStepThrough]
        public static class Cursor
        {
            [DebuggerStepThrough]
            public static class Move
            {
                public static AnsiControlCode Up(int lines = 1) => $"{Esc}[{lines.ToString()}A";
                public static AnsiControlCode Down(int lines = 1) => $"{Esc}[{lines.ToString()}B";
                public static AnsiControlCode Right(int columns = 1) => $"{Esc}[{columns.ToString()}C";
                public static AnsiControlCode Left(int columns = 1) => $"{Esc}[{columns.ToString()}D";
                public static AnsiControlCode NextLine(int line = 1) => $"{Esc}[{line.ToString()}E";
                public static AnsiControlCode ToUpperLeftCorner { get; } = $"{Esc}[H";
                public static AnsiControlCode ToLocation(int? left = null, int? top = null) => $"{Esc}[{top.GetValueOrDefault(1)};{left.GetValueOrDefault(1).ToString()}H";
            }

            [DebuggerStepThrough]
            public static class Scroll
            {
                public static AnsiControlCode UpOne { get; } = $"{Esc}[S";

                public static AnsiControlCode DownOne { get; } = $"{Esc}[T";
            }

            public static AnsiControlCode Hide { get; } = $"{Esc}[?25l";

            public static AnsiControlCode Show { get; } = $"{Esc}[?25h";

            public static AnsiControlCode SavePosition { get; } = $"{Esc}7";

            public static AnsiControlCode RestorePosition { get; } = $"{Esc}8";
        }

        [DebuggerStepThrough]
        public static class Clear
        {
            public static AnsiControlCode EntireScreen { get; } = $"{Esc}[2J";
            public static AnsiControlCode Line { get; } = $"{Esc}[2K";
            public static AnsiControlCode ToBeginningOfLine { get; } = $"{Esc}[1K";
            public static AnsiControlCode ToBeginningOfScreen { get; } = $"{Esc}[1J";
            public static AnsiControlCode ToEndOfLine { get; } = $"{Esc}[K";
            public static AnsiControlCode ToEndOfScreen { get; } = $"{Esc}[J";
        }

        public static string Esc { get; } = "\u001b";
    }
}
