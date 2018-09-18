namespace System.CommandLine.Rendering.Views
{
    public class ColumnDefinition
    {
        public SizeMode SizeMode { get; }

        public double Value { get; }

        private ColumnDefinition(SizeMode sizeMode, double value)
        {
            //TODO: Validation
            SizeMode = sizeMode;
            Value = value;
        }

        public static ColumnDefinition Fixed(int size)
        {
            if (size < 0.0)
            {
                throw new ArgumentException("Fixed size cannot be negative", nameof(size));
            }
            return new ColumnDefinition(SizeMode.Fixed, size);
        }

        public static ColumnDefinition Star(double weight)
        {
            if (weight < 0.0)
            {
                throw new ArgumentException("Weight cannot be negative", nameof(weight));
            }
            return new ColumnDefinition(SizeMode.Star, weight);
        }

        public static ColumnDefinition SizeToContent() => new ColumnDefinition(SizeMode.SizeToContent, 0);
    }
}
