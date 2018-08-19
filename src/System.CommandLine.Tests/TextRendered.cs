using System.Drawing;

namespace System.CommandLine.Tests
{
    public class TextRendered
    {
        public TextRendered(string text, int x, int y)
        {
            Text = text;
            Position = new Point(x, y);
        }

        public TextRendered(string text, Point position)
        {
            Text = text;
            Position = position;
        }

        public string Text { get; }

        public Point Position { get; }
    }
}