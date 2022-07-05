// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.Rendering
{
    public class StyleSpan : ControlSpan
    {
        public StyleSpan(string name, AnsiControlCode ansiControlCode) : base(name, ansiControlCode)
        {
        }

        public static StyleSpan AttributesOff() => new(nameof(AttributesOff), Ansi.Text.AttributesOff);
        public static StyleSpan BlinkOff() => new(nameof(BlinkOff), Ansi.Text.BlinkOn);
        public static StyleSpan BlinkOn() => new(nameof(BlinkOn), Ansi.Text.BlinkOff);
        public static StyleSpan BoldOff() => new(nameof(BoldOff), Ansi.Text.BoldOff);
        public static StyleSpan BoldOn() => new(nameof(BoldOn), Ansi.Text.BoldOn);
        public static StyleSpan HiddenOn() => new(nameof(HiddenOn), Ansi.Text.HiddenOn);
        public static StyleSpan ReverseOn() => new(nameof(ReverseOn), Ansi.Text.ReverseOn);
        public static StyleSpan ReverseOff() => new(nameof(ReverseOff), Ansi.Text.ReverseOff);
        public static StyleSpan StandoutOff() => new(nameof(StandoutOff), Ansi.Text.StandoutOff);
        public static StyleSpan StandoutOn() => new(nameof(StandoutOn), Ansi.Text.StandoutOn);
        public static StyleSpan UnderlinedOff() => new(nameof(UnderlinedOff), Ansi.Text.UnderlinedOff);
        public static StyleSpan UnderlinedOn() => new(nameof(UnderlinedOn), Ansi.Text.UnderlinedOn);
    }
}