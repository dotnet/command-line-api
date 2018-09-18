namespace System.CommandLine.Rendering.Views
{
    public class ColumnDefinition
    {
        public SizeMode SizeMode { get; }

        public double Value { get; }

        public ColumnDefinition(SizeMode sizeMode, double value)
        {
            //TODO: Validation
            SizeMode = sizeMode;
            Value = value;
        }

        public static ColumnDefinition Fixed(int size) => new ColumnDefinition(SizeMode.Fixed, size);

        public static ColumnDefinition Star(double weight) => new ColumnDefinition(SizeMode.Star, weight);

        public static ColumnDefinition SizeToContent() => new ColumnDefinition(SizeMode.SizeToContent, 0);
    }
}
