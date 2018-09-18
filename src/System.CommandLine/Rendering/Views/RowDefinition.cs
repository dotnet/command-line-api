namespace System.CommandLine.Rendering.Views
{
    public class RowDefinition
    {
        public SizeMode SizeMode { get; }

        public RowDefinition(double starSize)
        {
            StarSize = starSize;
        }

        public double StarSize { get; }

        public static RowDefinition Star(double weight) => new RowDefinition(weight);
    }
}
