using System.CommandLine.Rendering;
using System.CommandLine.Rendering.Spans;

namespace System.CommandLine.Tests.Rendering
{
    public class RenderingTestCase
    {
        private static readonly SpanFormatter _formatter = new SpanFormatter();

        public RenderingTestCase(
            string name,
            FormattableString rendering,
            Region inRegion,
            params TextRendered[] expectOutput)
        {
            if (rendering == null)
            {
                throw new ArgumentNullException(nameof(rendering));
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(name));
            }

            Name = name;
            InputSpan = _formatter.ParseToSpan(rendering);
            Region = inRegion ?? throw new ArgumentNullException(nameof(inRegion));
            ExpectedOutput = expectOutput ?? throw new ArgumentNullException(nameof(expectOutput));
        }

        public string Name { get; }

        public Span InputSpan { get; }

        public Region Region { get; }

        public TextRendered[] ExpectedOutput { get; }

        public override string ToString() => $"{Name} (in {Region})";
    }
}
