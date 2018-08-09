namespace System.CommandLine.Rendering
{
    public class StyleSpan : FormatSpan
    {
        public StyleSpan(string name) : base(name)
        {
        }

        public static StyleSpan BlinkOff => new StyleSpan(nameof(BlinkOff));
        public static StyleSpan BlinkOn => new StyleSpan(nameof(BlinkOn));
        public static StyleSpan BoldOff => new StyleSpan(nameof(BoldOff));
        public static StyleSpan BoldOn => new StyleSpan(nameof(BoldOn));
        public static StyleSpan HiddenOn => new StyleSpan(nameof(HiddenOn));
        public static StyleSpan ReverseOn => new StyleSpan(nameof(ReverseOn));
        public static StyleSpan ReversOff => new StyleSpan(nameof(ReversOff));
        public static StyleSpan StandoutOff => new StyleSpan(nameof(StandoutOff));
        public static StyleSpan StandoutOn => new StyleSpan(nameof(StandoutOn));
        public static StyleSpan UnderlinedOff => new StyleSpan(nameof(UnderlinedOff));
        public static StyleSpan UnderlinedOn => new StyleSpan(nameof(UnderlinedOn));
    }
}