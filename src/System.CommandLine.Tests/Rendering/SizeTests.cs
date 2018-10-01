using System.CommandLine.Rendering;
using FluentAssertions;
using Xunit;

namespace System.CommandLine.Tests.Rendering
{
    public class SizeTests
    {
        [Fact]
        public void Width_cannot_be_negative()
        {
            Action createSize = () => new Size(-1, 0);
            createSize.Should().Throw<ArgumentOutOfRangeException>().Where(exception => exception.ParamName == "width");
        }

        [Fact]
        public void Height_cannot_be_negative()
        {
            Action createSize = () => new Size(0, -1);
            createSize.Should().Throw<ArgumentOutOfRangeException>().Where(exception => exception.ParamName == "height");
        }

        [Fact]
        public void Width_and_height_can_be_set_on_size()
        {
            var size = new Size(20, 15);
            size.Width.Should().Be(20);
            size.Height.Should().Be(15);
        }
    }
}
