namespace System.CommandLine.Rendering
{
    public class BackgroundColorSpan : ColorSpan
    {
        public BackgroundColorSpan(string name) : base(name)
        {
        }

        public BackgroundColorSpan(RgbColor rgbColor) : base(rgbColor)
        {
        }

        public BackgroundColorSpan(byte r, byte g, byte b) : this(new RgbColor(r, g, b))
        {
        }

        public static BackgroundColorSpan Reset { get; } = new BackgroundColorSpan(nameof(Reset));

        public static BackgroundColorSpan Black { get; } = new BackgroundColorSpan(nameof(Black));

        public static BackgroundColorSpan Red { get; } = new BackgroundColorSpan(nameof(Red));

        public static BackgroundColorSpan Green { get; } = new BackgroundColorSpan(nameof(Green));

        public static BackgroundColorSpan Yellow { get; } = new BackgroundColorSpan(nameof(Yellow));

        public static BackgroundColorSpan Blue { get; } = new BackgroundColorSpan(nameof(Blue));

        public static BackgroundColorSpan Magenta { get; } = new BackgroundColorSpan(nameof(Magenta));

        public static BackgroundColorSpan Cyan { get; } = new BackgroundColorSpan(nameof(Cyan));

        public static BackgroundColorSpan White { get; } = new BackgroundColorSpan(nameof(White));

        public static BackgroundColorSpan DarkGray { get; } = new BackgroundColorSpan(nameof(DarkGray));

        public static BackgroundColorSpan LightRed { get; } = new BackgroundColorSpan(nameof(LightRed));

        public static BackgroundColorSpan LightGreen { get; } = new BackgroundColorSpan(nameof(LightGreen));

        public static BackgroundColorSpan LightYellow { get; } = new BackgroundColorSpan(nameof(LightYellow));

        public static BackgroundColorSpan LightBlue { get; } = new BackgroundColorSpan(nameof(LightBlue));

        public static BackgroundColorSpan LightMagenta { get; } = new BackgroundColorSpan(nameof(LightMagenta));

        public static BackgroundColorSpan LightCyan { get; } = new BackgroundColorSpan(nameof(LightCyan));

        public static BackgroundColorSpan LightGray { get; } = new BackgroundColorSpan(nameof(LightGray));

        public static BackgroundColorSpan Rgb(byte r, byte g, byte b) => new BackgroundColorSpan(r, g, b);
    }
}
