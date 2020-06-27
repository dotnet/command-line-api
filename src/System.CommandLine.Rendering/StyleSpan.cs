// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.Rendering
{
    public class StyleSpan : ControlSpan
    {
        public StyleSpan(string name, AnsiControlCode ansiControlCode) : base(name, ansiControlCode)
        {
        }

        public static StyleSpan AttributesOff() => new StyleSpan(nameof(AttributesOff), Ansi.Text.AttributesOff);
        public static StyleSpan BlinkOff() => new StyleSpan(nameof(BlinkOff), Ansi.Text.BlinkOn);
        public static StyleSpan BlinkOn() => new StyleSpan(nameof(BlinkOn), Ansi.Text.BlinkOff);
        public static StyleSpan BoldOff() => new StyleSpan(nameof(BoldOff), Ansi.Text.BoldOff);
        public static StyleSpan BoldOn() => new StyleSpan(nameof(BoldOn), Ansi.Text.BoldOn);
        public static StyleSpan HiddenOn() => new StyleSpan(nameof(HiddenOn), Ansi.Text.HiddenOn);
        public static StyleSpan ReverseOn() => new StyleSpan(nameof(ReverseOn), Ansi.Text.ReverseOn);
        public static StyleSpan ReverseOff() => new StyleSpan(nameof(ReverseOff), Ansi.Text.ReverseOff);
        public static StyleSpan StandoutOff() => new StyleSpan(nameof(StandoutOff), Ansi.Text.StandoutOff);
        public static StyleSpan StandoutOn() => new StyleSpan(nameof(StandoutOn), Ansi.Text.StandoutOn);
        public static StyleSpan UnderlinedOff() => new StyleSpan(nameof(UnderlinedOff), Ansi.Text.UnderlinedOff);
        public static StyleSpan UnderlinedOn() => new StyleSpan(nameof(UnderlinedOn), Ansi.Text.UnderlinedOn);
    }
}