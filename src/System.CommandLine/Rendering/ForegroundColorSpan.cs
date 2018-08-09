namespace System.CommandLine.Rendering
{
    public class ForegroundColorSpan : ColorSpan
    {
        public ForegroundColorSpan(string name) : base(name)
        {
        }

        public ForegroundColorSpan(RgbColor rgbColor) : base(rgbColor)
        {
        }

        public ForegroundColorSpan(byte r, byte g, byte b) : this(new RgbColor(r, g, b))
        {
        }

        public static ForegroundColorSpan Reset { get; } = new ForegroundColorSpan(nameof(Reset));

        public static ForegroundColorSpan Black { get; } = new ForegroundColorSpan(nameof(Black));
        public static ForegroundColorSpan Red { get; } = new ForegroundColorSpan(nameof(Red));
        public static ForegroundColorSpan Green { get; } = new ForegroundColorSpan(nameof(Green));
        public static ForegroundColorSpan Yellow { get; } = new ForegroundColorSpan(nameof(Yellow));
        public static ForegroundColorSpan Blue { get; } = new ForegroundColorSpan(nameof(Blue));
        public static ForegroundColorSpan Magenta { get; } = new ForegroundColorSpan(nameof(Magenta));
        public static ForegroundColorSpan Cyan { get; } = new ForegroundColorSpan(nameof(Cyan));
        public static ForegroundColorSpan White { get; } = new ForegroundColorSpan(nameof(White));
        public static ForegroundColorSpan DarkGray { get; } = new ForegroundColorSpan(nameof(DarkGray));
        public static ForegroundColorSpan LightRed { get; } = new ForegroundColorSpan(nameof(LightRed));
        public static ForegroundColorSpan LightGreen { get; } = new ForegroundColorSpan(nameof(LightGreen));
        public static ForegroundColorSpan LightYellow { get; } = new ForegroundColorSpan(nameof(LightYellow));
        public static ForegroundColorSpan LightBlue { get; } = new ForegroundColorSpan(nameof(LightBlue));
        public static ForegroundColorSpan LightMagenta { get; } = new ForegroundColorSpan(nameof(LightMagenta));
        public static ForegroundColorSpan LightCyan { get; } = new ForegroundColorSpan(nameof(LightCyan));
        public static ForegroundColorSpan LightGray { get; } = new ForegroundColorSpan(nameof(LightGray));

        public static ForegroundColorSpan Rgb(byte r, byte g, byte b) => new ForegroundColorSpan(r, g, b);
    }
}